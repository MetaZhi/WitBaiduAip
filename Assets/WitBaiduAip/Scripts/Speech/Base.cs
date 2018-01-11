/*
 * 作者：关尔
 * 时间：2017年12月1日
 * 关注“洪流学堂”公众号，让你快人几步
 */

using System.Collections;
using UnityEngine;

namespace Wit.BaiduAip.Speech
{
    public class Base
    {
        public string SecretKey { get; private set; }

        public string APIKey { get; private set; }
        public string Token { get; private set; }

        public Base(string apiKey, string secretKey)
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
    }
}