/*
 * 作者：关尔
 * 时间：2018年1月11日
 * 关注“洪流学堂”公众号，让你快人几步
 */

using UnityEngine;
using UnityEngine.UI;
using Wit.BaiduAip.Speech;

public class TtsDemo : MonoBehaviour
{
    public string APIKey = "";
    public string SecretKey = "";
    public Button SynthesisButton;
    public InputField Input;
    public Text DescriptionText;

    private Tts _asr;
    private AudioSource _audioSource;
    private bool _startPlaying;

    void Start()
    {
        _asr = new Tts(APIKey, SecretKey);
        StartCoroutine(_asr.GetAccessToken());

        _audioSource = gameObject.AddComponent<AudioSource>();

        DescriptionText.text = "";

        SynthesisButton.onClick.AddListener(OnClickSynthesisButton);
    }

    private void OnClickSynthesisButton()
    {
        SynthesisButton.gameObject.SetActive(false);
        DescriptionText.text = "合成中...";

        StartCoroutine(_asr.Synthesis(Input.text, s =>
        {
            if (s.Success)
            {
                DescriptionText.text = "合成成功，正在播放";
                _audioSource.clip = s.clip;
                _audioSource.Play();

                _startPlaying = true;
            }
            else
            {
                DescriptionText.text = s.err_msg;
				SynthesisButton.gameObject.SetActive(true);
            }
        }));
    }

    void Update()
    {
        if (_startPlaying)
        {
            if (!_audioSource.isPlaying)
            {
                _startPlaying = false;
                DescriptionText.text = "播放完毕，可以修改文本继续测试";
                SynthesisButton.gameObject.SetActive(true);
            }
        }
    }
}