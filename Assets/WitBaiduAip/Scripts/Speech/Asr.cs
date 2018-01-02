/*
 * 作者：关尔
 * 时间：2017年12月1日
 * 关注“洪流学堂”公众号，让你快人几步
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wit.BaiduAip.Speech
{
    /// <summary>
    /// 用户解析token的json数据
    /// </summary>
    class TokenResponse
    {
        public string access_token = null;
    }

    public class Asr
    {
        public string SecretKey { get; private set; }

        public string APIKey { get; private set; }
        public string Token { get; private set; }

        public Asr(string apiKey, string secretKey)
        {
            APIKey = apiKey;
            SecretKey = secretKey;
        }

        public IEnumerator GetAccessToken()
        {
            var uri =
                string.Format(
                    "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}",
                    APIKey, SecretKey);
            var www = new WWW(uri);
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                var result = JsonUtility.FromJson<TokenResponse>(www.text);
                Token = result.access_token;
                Debug.Log("Get access_token successfully");
            }
            else
            {
                Debug.LogError(www.error);
            }
        }

        public IEnumerator Recognize(byte[] data, Action<string> callback)
        {
            var uri =
                string.Format("http://vop.baidu.com/server_api?lan=zh&cuid={0}&token={1}",
                    SystemInfo.deviceUniqueIdentifier, Token);

            var headers = new Dictionary<string, string> {{"Content-Type", "audio/pcm;rate=16000"}};

            var www = new WWW(uri, data, headers);
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.text);
                callback(www.text);
            }
            else
                Debug.LogError(www.error);
        }

        /// <summary>
        /// 将Unity的AudioClip数据转化为PCM格式16bit数据
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static byte[] ConvertAudioClipToPCM16(AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            var samples_int16 = new short[samples.Length];

            for (var index = 0; index < samples.Length; index++)
            {
                var f = samples[index];
                samples_int16[index] = (short) (f * short.MaxValue);
            }

            var byteArray = new byte[samples_int16.Length * 2];
            Buffer.BlockCopy(samples_int16, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }
    }
}