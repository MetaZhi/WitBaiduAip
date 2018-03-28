/*
 * 作者：关尔
 * 时间：2018年1月11日
 * 关注“洪流学堂”公众号，让你快人几步
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_STANDALONE || UNITY_EDITOR
using NAudio.Wave;
#endif
using UnityEngine;

namespace Wit.BaiduAip.Speech
{
    /// <summary>
    ///     语音合成结果
    /// </summary>
    public class TtsResponse
    {
        public int err_no;
        public string err_msg;
        public string sn;
        public int idx;

        public bool Success
        {
            get { return err_no == 0; }
        }

        public AudioClip clip;
    }

    public class Tts : Base
    {
        public enum Pronouncer
        {
            Female, // 0为普通女声
            Male, // 1为普通男生
            Duxiaoyao, // 3为情感合成-度逍遥
            Duyaya // 4为情感合成-度丫丫
        }

        private const string UrlTts = "http://tsn.baidu.com/text2audio";

        public Tts(string apiKey, string secretKey) : base(apiKey, secretKey)
        {
        }

        public IEnumerator Synthesis(string text, Action<TtsResponse> callback, int speed = 5, int pit = 5, int vol = 5,
            Pronouncer per = Pronouncer.Female)
        {
			yield return PreAction ();

			if (tokenFetchStatus == Base.TokenFetchStatus.Failed) {
				Debug.LogError("Token was fetched failed. Please check your APIKey and SecretKey");
				callback (new TtsResponse () {
					err_no = -1,
					err_msg = "Token was fetched failed. Please check your APIKey and SecretKey"
				});
				yield break;
			}

            var param = new Dictionary<string, string>();
            param.Add("tex", text);
            param.Add("tok", Token);
            param.Add("cuid", SystemInfo.deviceUniqueIdentifier);
            param.Add("ctp", "1");
            param.Add("lan", "zh");
            param.Add("spd", Mathf.Clamp(speed, 0, 9).ToString());
            param.Add("pit", Mathf.Clamp(pit, 0, 9).ToString());
            param.Add("vol", Mathf.Clamp(vol, 0, 15).ToString());
            param.Add("per", ((int) per).ToString());

            string url = UrlTts;
            int i = 0;
            foreach (var p in param)
            {
                url += i != 0 ? "&" : "?";
                url += p.Key + "=" + p.Value;
                i++;
            }

            WWW www = new WWW(url);
            yield return www;


            if (string.IsNullOrEmpty(www.error))
            {
                var type = www.responseHeaders["Content-Type"];
                Debug.Log("response type: " + type);

                if (type == "audio/mp3")
                {
#if UNITY_STANDALONE || UNITY_EDITOR
                    var response = new TtsResponse {clip = FromMp3Data(www.bytes)};
#else
                    var response = new TtsResponse {clip = www.GetAudioClip(false, true, AudioType.MPEG)};
#endif
                    callback(response);
                }
                else
                {
                    Debug.LogError(www.text);
                    callback(JsonUtility.FromJson<TtsResponse>(www.text));
                }
            }
            else
                Debug.LogError(www.error);
        }


#if UNITY_STANDALONE || UNITY_EDITOR
        /// <summary>
        /// 将mp3格式的字节数组转换为audioclip
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static AudioClip FromMp3Data(byte[] data)
        {
            // Load the data into a stream  
            MemoryStream mp3stream = new MemoryStream(data);
            // Convert the data in the stream to WAV format  
            Mp3FileReader mp3audio = new Mp3FileReader(mp3stream);

            WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3audio);
            // Convert to WAV data  
            Wav wav = new Wav(AudioMemStream(waveStream).ToArray());

            AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            // Return the clip  
            return audioClip;
        }

        private static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            MemoryStream outputStream = new MemoryStream();
            using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                byte[] bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
                waveFileWriter.Write(bytes, 0, bytes.Length);
                waveFileWriter.Flush();
            }
            return outputStream;
        }
#endif
    }
}