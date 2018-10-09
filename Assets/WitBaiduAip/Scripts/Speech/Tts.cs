/*
 * 作者：关尔
 * 时间：2018年1月11日
 * 关注“洪流学堂”公众号，让你快人几步
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Wit.BaiduAip.Speech
{
    /// <summary>
    ///     语音合成结果
    /// </summary>
    [Serializable]
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
            yield return PreAction();

            if (tokenFetchStatus == Base.TokenFetchStatus.Failed)
            {
                Debug.LogError("Token was fetched failed. Please check your APIKey and SecretKey");
                callback(new TtsResponse()
                {
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
            param.Add("per", ((int)per).ToString());
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_UWP
            param.Add("aue", "6"); // set to wav, default is mp3
#endif

            string url = UrlTts;
            int i = 0;
            foreach (var p in param)
            {
                url += i != 0 ? "&" : "?";
                url += p.Key + "=" + p.Value;
                i++;
            }

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_UWP
            var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
#else
            var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
#endif
            Debug.Log("[WitBaiduAip]" + www.url);
            yield return www.SendWebRequest();


            if (string.IsNullOrEmpty(www.error))
            {
                var type = www.GetResponseHeader("Content-Type");
                Debug.Log("[WitBaiduAip]response type: " + type);

                if (type.Contains("audio"))
                {
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_UWP
                    var clip = DownloadHandlerAudioClip.GetContent(www);
                    var response = new TtsResponse { clip = clip };
#else
                    var response = new TtsResponse {clip = DownloadHandlerAudioClip.GetContent(www) };
#endif
                    callback(response);
                }
                else
                {
                    var textBytes = www.downloadHandler.data;
                    var errorText = Encoding.UTF8.GetString(textBytes);
                    Debug.LogError("[WitBaiduAip]" + errorText);
                    callback(JsonUtility.FromJson<TtsResponse>(errorText));
                }
            }
            else
            {
                Debug.LogError(www.error);
            }
        }
    }
}