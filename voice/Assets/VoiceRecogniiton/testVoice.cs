using UnityEngine;
using UnityEngine.UI;
using Water;
using System.Collections.Generic;
using System.IO;
using System;

public class testVoice : MonoBehaviour {
    public TextAsset pcmFile;

    public Image _btn;
    public Text _txt;

	// Use this for initialization
	void Start () {
        //testSampleRate();
        testRecognition();

        UIEventTriggerListener.Get(_btn.gameObject).onDown = onPointerDown;
        UIEventTriggerListener.Get(_btn.gameObject).onUp = onPointerUp;
    }

    void testRecognition()
    {
        VoiceRecognition v = GetComponent<VoiceRecognition>();
        Debug.Log("aaaaaaaa");
        v.init((bool inited_)=> {
            Debug.Log("bbbbbbbb: " + inited_);
            // convert pcm to float
            int total = pcmFile.bytes.Length / 2;
            float[] pcms;
            SampleRateDll.convertPCMInt16ToFloat(pcmFile.bytes, total, out pcms);

            // upsample to 44100
            //int upSampleCnt = (int)(44100.0f / 8000.0f * total);
            //float[] upBuffer = new float[upSampleCnt];
            int upSampleCnt;
            float[] upBuffer;
            upSampleCnt = SampleRateDll.call_src_simple_plain(pcms, out upBuffer, total, out upSampleCnt, 44100f / 8000f, (int)SampleRateConvertType.SRC_SINC_FASTEST, 1);

            // downsample to 8000
            //int downSampleCnt = (int)(upSampleCnt * (8000f / 44100f));
            //float[] downBuffer = new float[downSampleCnt];
            int downSampleCnt;
            float[] downBuffer;
            downSampleCnt = SampleRateDll.call_src_simple_plain(upBuffer, out downBuffer, upSampleCnt, out downSampleCnt, 8000f / 44100f, (int)SampleRateConvertType.SRC_SINC_FASTEST, 1);

            // convert pcm to int16
            byte[] final;
            SampleRateDll.convertPCMFloatToInt16(downBuffer, (int)downSampleCnt, out final);

            v.getVoiceStr(final, (string result_) =>
            {
                Debug.Log("result: " + result_);

                v.getVoiceStr(final, (string another_) =>
                {
                    Debug.Log("another: " + another_);
                });
            });
        });
    }

    void testSampleRate()
    {
        int ret = SampleRateDll.src_is_valid_ratio(0.05f);

        int inCnt = 44100;
        float[] samples = new float[inCnt];
        for(int i = 0; i < inCnt; ++i)
        {
            samples[i] = 0.1f;
        }
        int outCnt;
        float[] outSamples;
        //long outCnt = 8000;
        //float[] outSamples = new float[outCnt];
        int result = SampleRateDll.call_src_simple_plain(samples, out outSamples, inCnt, out outCnt, (float)outCnt / (float)inCnt, (int)SampleRateConvertType.SRC_SINC_FASTEST, 1);

        Debug.Log("finished: " + result + ", " + outSamples[4000]);
    }

    void onPointerDown(GameObject go_)
    {
        Debug.Log("on down");

        VoiceRecognition v = GetComponent<VoiceRecognition>();

        v.startRecording();
    }

    void onPointerUp(GameObject go_)
    {
        Debug.Log("on up");

        VoiceRecognition v = GetComponent<VoiceRecognition>();

        //v.init();
        //string result = v.getVoiceStr(pcmFile.bytes);
        //Debug.Log("result: " + result);

        v.endRecording(onRecordResult);
    }

    void onRecordResult(AudioClip voiceClip_, string txt_, float[] samples_, int sampleCnt_, int freq_)
    {
        _txt.text = "result: " + txt_;
    }
}
