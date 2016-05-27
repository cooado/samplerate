using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace Water
{
    class AccessTokenMsg
    {
        public string access_token;
    }

    class TextResponseMsg
    {
        public string err_msg;
        public string[] result;
        public string err_no;
    }

    class VoiceRecognition : MonoBehaviour
    {
        void Update()
        {
            // check system init
            if (!_systemInited)
            {
                if(_initCallback != null)
                {
                    // check init thread failed
                    if(_webThread == null)
                    {
                        _initCallback(false);
                        _initCallback = null;
                    }
                }

                return;
            }

            // check init callback
            if (_initCallback != null)
            {
                _initCallback(true);
                _initCallback = null;

                return;
            }

            // skip if idle
            if (_voiceThreadState == VoiceThreadState.E_IDLE) return;

            // check if voice task finished
            if(_voiceThreadState == VoiceThreadState.E_FINISHED)
            {
                _voiceThreadState = VoiceThreadState.E_IDLE;

                if (_getVoiceCallback != null)
                {
                    var cb = _getVoiceCallback;
                    _getVoiceCallback = null;
                    cb(_getVoiceResult);
                }
            }

            // check if voice task time out
            if(_voiceThreadState == VoiceThreadState.E_RUNNING)
            {
                if(this._webThread != null){
                    float now = Time.realtimeSinceStartup;
                    const float voiceTaskTimeout = 10;
                    if (now - _startGetVoiceTime > voiceTaskTimeout)
                    {
                        try
                        {
                            this._webThread.Abort();
                        }
                        catch (Exception e) { };

                        this._webThread = null;

                        // check callback
                        if(_getVoiceCallback != null)
                        {
                            _getVoiceCallback(string.Empty);
                            _getVoiceCallback = null;
                        }

                        _voiceThreadState = VoiceThreadState.E_IDLE;
                    }
                }
            }
        }

        bool _systemInited = false;
        InitCallback _initCallback;
        public delegate void InitCallback(bool result_);
		public void init(InitCallback cb_){
            if (_systemInited)
            {
                cb_(true);
                return;
            }

            _initCallback = cb_;
            if (_webThread == null)
            {
                _webThread = new Thread(new ThreadStart(getTokenThread));
                _webThread.Start();
            }
        }

        Thread _webThread = null;
        void getTokenThread()
        {
            try
            {
                this.API_access_token = this.getStrAccess(this.API_key, this.API_secret_key);

                if (!string.IsNullOrEmpty(this.API_access_token))
                {
                    _systemInited = true;
                }
            }
            catch (Exception e) { };

            _webThread = null;
        }

        /**
         * Request voice recognition service
         */
        public delegate void GetVoiceStrCallback(string str_);
		public void getVoiceStr(byte[] content_, GetVoiceStrCallback cb_){
            if (!_systemInited)
            {
                cb_(null);
                return;
            }

            // check if busy
            if(_webThread != null)
            {
                cb_(null);
                return;
            }

            // check voice task state
            if(_voiceThreadState != VoiceThreadState.E_IDLE)
            {
                cb_(null);
                return;
            }

            _voiceThreadState = VoiceThreadState.E_RUNNING;
            _voiceContents = content_;
            _getVoiceCallback = cb_;
            _startGetVoiceTime = Time.realtimeSinceStartup;
            _webThread = new Thread(new ThreadStart(getVoiceStrThread));
            _webThread.Start();
   //         string result = this.getStrText(this.API_id, this.API_access_token, this.API_language, content_, "pcm", this.API_record_HZ);

			//return result;
		}

        GetVoiceStrCallback _getVoiceCallback;
        byte[] _voiceContents = null;
        float _startGetVoiceTime = 0;
        enum VoiceThreadState
        {
            E_IDLE = 0,
            E_RUNNING,
            E_FINISHED
        }
        // 0: voice thread idle, 1: voice thread running, 2: voice thread finished
        VoiceThreadState _voiceThreadState;
        string _getVoiceResult;
        void getVoiceStrThread()
        {
            try
            {
                string result = this.getStrText(this.API_id, this.API_access_token, this.API_language, _voiceContents, this.API_record_format, this.API_record_HZ);
                _getVoiceResult = result;
                _voiceThreadState = VoiceThreadState.E_FINISHED;
                _voiceContents = null;

                _webThread = null;
            }
            catch (Exception e) { };
        }

        int _maxRecordSec = 10;
        int _recordRate = 8000;
        float _startRecordTime;
        AudioClip _voiceClip;
        public void startRecording()
        {
            string[] devices = Microphone.devices;
            Debug.Log("device cnt: " + devices.Length);

            //AudioSource aud = GetComponent<AudioSource>();
            int minFreq = 0;
            int maxFreq = 0;
            Microphone.GetDeviceCaps(string.Empty, out minFreq, out maxFreq);
            Debug.Log("freq: " + minFreq + ", " + maxFreq);
            _recordRate = 8000;
            if(minFreq != 0 && maxFreq != 0)
            {
                if (_recordRate < minFreq) _recordRate = minFreq;
                if (_recordRate > maxFreq) _recordRate = maxFreq;
            }

            Debug.Log("record freq: " + _recordRate);
            _startRecordTime = Time.realtimeSinceStartup;
            _voiceClip = Microphone.Start(string.Empty, true, _maxRecordSec, _recordRate);
            //aud.Play();
        }

        /**
         *  @param  voiceClip_      audio clip with audio in
         *  @param  text_           recognized text
         *  @param  voiceSamples_   pcm sound samples
         *  @param  sampleCnt_      pcm sample count
         *  @param  freq_           pcm record frequency  
         */
        public delegate void RecordCallback(AudioClip voiceClip_, string text_, float[] voiceSamples_, int sampleCnt, int freq_);
        public void endRecording(RecordCallback cb_)
        {
            Microphone.End(string.Empty);

            Debug.Log("0000000: " + (_voiceClip != null));
            if (!_voiceClip)
            {
                cb_(null, string.Empty, null, 0, 0);
                return;
            };

            // check sound length
            float duration = Time.realtimeSinceStartup - _startRecordTime;
            const float timeThreshold = 0.2f;
            if (duration < timeThreshold) {
                cb_(null, string.Empty, null, 0, 0);
                return;
            }

            //AudioSource aud = GetComponent<AudioSource>();

            Debug.Log("1111111: " + _voiceClip.length + ", " + _voiceClip.samples + ", " + _voiceClip.channels);

            //aud.PlayOneShot(_voiceClip);

            Debug.Log("2222222: ");
            int sampleCnt = _voiceClip.samples * _voiceClip.channels;
            float[] samples = new float[sampleCnt];
            _voiceClip.GetData(samples, 0);

            // cap sample by sound duration
            int sampleByDuration = (int)(duration * _recordRate);
            Debug.LogFormat("sample: {0}, {1}", sampleCnt, sampleByDuration);
            if (sampleByDuration < sampleCnt) sampleCnt = sampleByDuration;

            // resample
            if (_recordRate != 8000)
            {
                float[] buffer;
                int outSampleCnt;
                int bufferLen = SampleRateDll.call_src_simple_plain(samples, out buffer, sampleCnt, out outSampleCnt, 8000.0f / _recordRate, (int)SampleRateConvertType.SRC_SINC_FASTEST, 1);
                if (bufferLen == 0)
                {
                    cb_(null, string.Empty, null, 0, 0);
                    return;
                };

                samples = buffer;
                sampleCnt = bufferLen;
            }

            // store samples
            float[] originalSamples = samples;
            int originalSampleCnt = sampleCnt;

            Debug.Log("3333333: " + sampleCnt);
            // change bit rate
            byte[] final;
            SampleRateDll.convertPCMFloatToInt16(samples, sampleCnt, out final);

            Debug.Log("4444444: ");
            getVoiceStr(final, (string str_) => {
                Debug.Log("record result: " + str_);
                cb_(_voiceClip, str_, originalSamples, originalSampleCnt, 8000);
            });
        }

        string API_id = "8176169";
        string API_record = null;
        string API_record_format = "pcm";
        string API_record_HZ = "8000";
        string API_key = "ja70RWDRbrgR21HOBx2pH6e6";
        string API_secret_key = "454fb99147dab4c196cdff094176fcc2";
        string API_language = "zh";
        string API_access_token = null;
        string strJSON = "";

        public string getStrAccess(string para_API_key, string para_API_secret_key)
        {

            //方法参数说明:
            //para_API_key:API_key(你的KEY)
            //para_API_secret_key(你的SECRRET_KEY)

            //方法返回值说明:
            //百度认证口令码,access_token
            string access_html = null;
            string access_token = null;
            string getAccessUrl = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials" +
           "&client_id=" + para_API_key + "&client_secret=" + para_API_secret_key;
            try
            {
				Debug.Log("token url: " + getAccessUrl);
				ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                HttpWebRequest getAccessRequest = WebRequest.Create(getAccessUrl) as HttpWebRequest;
                //getAccessRequest.Proxy = null;
                //                getAccessRequest.ContentType = "multipart/form-data";
                //                getAccessRequest.Accept = "*/*";
                //                getAccessRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
                getAccessRequest.Timeout = 30000;//30秒连接不成功就中断 
                //                getAccessRequest.Method = "post";

                HttpWebResponse response = getAccessRequest.GetResponse() as HttpWebResponse;
                using (StreamReader strHttpComback = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    access_html = strHttpComback.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
				Debug.LogError(ex.ToString());
            }

            //JObject jo = JObject.Parse(access_html);
            //access_token = jo["access_token"].ToString();//得到返回的toke
            Debug.Log("access html: " + access_html);
            AccessTokenMsg jo = JsonUtility.FromJson<AccessTokenMsg>(access_html);
            access_token = jo.access_token;//得到返回的toke
			Debug.Log("access token: " + access_html);
            return access_token;
        }




        public string getStrText(string para_API_id, string para_API_access_token, string para_API_language, byte[] para_voice, string para_format, string para_Hz)
        {
            /**方法参数说明:
            para_API_id: API_id(你的ID)
                para_API_access_token(getStrAccess(...)方法得到的access_token口令)
                para_API_language(你要识别的语言, zh, en, ct)
                para_API_record(语音文件的路径)
                para_format(语音文件的格式)
                para_Hz(语音文件的采样率 16000或者8000)
    
            //该方法返回值:
            //该方法执行正确返回值是语音翻译的文本,错误是错误号,可以去看百度语音文档,查看对应错误 */

            string strText = null;
            string error = null;
            //FileInfo fi = new FileInfo(para_API_record);
            //FileStream fs = new FileStream(para_API_record, FileMode.Open);
            //byte[] voice = new byte[fs.Length];
            byte[] voice = para_voice;
            //fs.Read(voice, 0, voice.Length);
            //fs.Close();

			try{
				//            string getTextUrl = "http://vop.baidu.com/server_api?lan=" + para_API_language + "&cuid=" + para_API_id + "&token=" + para_API_access_token;
				string getTextUrl = "http://vop.baidu.com/server_api?cuid=" + para_API_id + "&token=" + para_API_access_token;
				HttpWebRequest getTextRequst = WebRequest.Create(getTextUrl) as HttpWebRequest;

				/* getTextRequst.Proxy = null;
             getTextRequst.ServicePoint.Expect100Continue = false;
             getTextRequst.ServicePoint.UseNagleAlgorithm = false;
             getTextRequst.ServicePoint.ConnectionLimit = 65500;
             getTextRequst.AllowWriteStreamBuffering = false;*/

				getTextRequst.ContentType = "audio/" + para_format + ";rate=" + para_Hz;
				getTextRequst.ContentLength = voice.Length;
				getTextRequst.Method = "POST";
				//            getTextRequst.Accept = "*/*";
				//            getTextRequst.KeepAlive = true;
				//            getTextRequst.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
				getTextRequst.Timeout = 30000;//30秒连接不成功就中断 
				using (Stream writeStream = getTextRequst.GetRequestStream())
				{
					writeStream.Write(voice, 0, voice.Length);
				}

				HttpWebResponse getTextResponse = getTextRequst.GetResponse() as HttpWebResponse;
				using (StreamReader strHttpText = new StreamReader(getTextResponse.GetResponseStream(), Encoding.UTF8))
				{
					strJSON = strHttpText.ReadToEnd();
				}
				//JObject jsons = JObject.Parse(strJSON);//解析JSON
				Debug.Log("string json: " + strJSON);
				TextResponseMsg jsons = JsonUtility.FromJson<TextResponseMsg>(strJSON);
				//if (jsons["err_msg"].Value<string>() == "success.")
				if (jsons.err_msg == "success.")
				{
					//strText = jsons["result"][0].ToString();
					strText = jsons.result[0];
					return strText;
				}
				else
				{
					//error = jsons["err_no"].Value<string>() + jsons["err_msg"].Value<string>();
					error = jsons.err_no + jsons.err_msg;
					return error;
				}
			}
			catch(WebException ex){
				Debug.LogError(ex.ToString());

				return "";
			}
        }
    }
}
