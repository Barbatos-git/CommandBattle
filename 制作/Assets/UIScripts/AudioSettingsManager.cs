using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    //public Slider masterSlider;
    public Slider bgmSlider;
    //public Slider sfxSlider;

    void Start()
    {
        //float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        //float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        //SetMasterVolume(masterVol);
        SetBGMVolume(bgmVol);
        //SetSFXVolume(sfxVol);

        //masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        //sfxSlider.value = sfxVol;
    }

    /*public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }*/

    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    /*public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }*/
}