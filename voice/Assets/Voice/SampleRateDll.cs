using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;
using System.IO;
using UnityEngine;

namespace Water
{
    enum SampleRateConvertType
      {
        SRC_SINC_BEST_QUALITY = 0,
        SRC_SINC_MEDIUM_QUALITY = 1,
        SRC_SINC_FASTEST = 2,
        SRC_ZERO_ORDER_HOLD = 3,
        SRC_LINEAR = 4
    };

    public class SampleRateDll
    {
        const string SampleRateDLLName = "samplerate";

#if !UNITY_IPHONE
        [SuppressUnmanagedCodeSecurity]
#endif

        [DllImport(SampleRateDLLName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_is_valid_ratio(double ratio);

        [DllImport(SampleRateDLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int src_simple_plain(float* data_in, float* data_out, int input_frames, int output_frames, float src_ratio, int converter_type, int channels);

        public static unsafe int call_src_simple_plain(float[] data_in, out float[] data_out, int input_frames, out int output_frames, float src_ratio, int converter_type, int channels)
        {
            output_frames = (int)(src_ratio * input_frames);
            data_out = new float[output_frames];


            //Debug.Log("input frames: " + input_frames + ", src ration: " + src_ratio + ", convert type: " + converter_type + ", channels: " + channels);
            //if(input_frames >= 2)
            //{
            //    Debug.LogFormat("info: {0}, {1}", data_in[0], data_in[1]);
            //}

            // validate for null and Length < 64
            fixed (float* pin = data_in)
            fixed (float* pout = data_out)
            return src_simple_plain(pin, pout, input_frames, output_frames, src_ratio, converter_type, channels);
        }

        public static void convertPCMFloatToInt16(float[] inSamples, int sampleCnt, out byte[] outSamples)
        {
            outSamples = new byte[sampleCnt * 2];
            using (var stream = new MemoryStream(outSamples))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < sampleCnt; ++i)
                    {
                        float sample = inSamples[i];
                        Int16 val = (Int16)(sample * 32767);
                        writer.Write(val);
                    }
                }
            }
        }

        public static void convertPCMInt16ToFloat(byte[] inSamples, int sampleCnt, out float[] outSamples)
        {
            outSamples = new float[sampleCnt];
            using (var stream = new MemoryStream(inSamples))
            {
                using (var reader = new BinaryReader(stream))
                {
                    for (int i = 0; i < sampleCnt; ++i)
                    {
                        Int16 sample = reader.ReadInt16();
                        float val = (float)sample / 32768.0f;
                        outSamples[i] = val;
                    }
                }
            }
        }
    }
}
