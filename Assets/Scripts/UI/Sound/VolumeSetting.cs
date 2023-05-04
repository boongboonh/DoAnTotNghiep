using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSetting : MonoBehaviour
{
    //ham goi trong menu

    [SerializeField] AudioMixer musicMixer;
    [SerializeField] Slider backgroundSlider;
    [SerializeField] Slider effectSlider;

    [SerializeField] float volumeStart = 1f;

    [SerializeField] string nameBackgroundSound = "soundBackgroundMix";
    [SerializeField] string nameEffectSound = "soundEffectMix";
    void Start()
    {
        //kiem tra bien ton tai chua
        if (PlayerPrefs.HasKey("soundBackgroundMix"))
        {
            //load du lieu
            loadVolume();
        }
        else
        {
            // khoi tao gia tri ban dau la .5f;

            PlayerPrefs.SetFloat(nameBackgroundSound, volumeStart);
            PlayerPrefs.SetFloat(nameEffectSound, volumeStart);
            loadVolume();
        }
    }

    //gouj trong slider background sound
    public void setBackgroundSound()
    {
        float volume = backgroundSlider.value;
        musicMixer.SetFloat(nameBackgroundSound, Mathf.Log(volume)*20);         //chuyen doi logarit ve tuyen tinh
        PlayerPrefs.SetFloat(nameBackgroundSound, volume);
    }

    //goi trong slider effect sound
    public void setEffectSound()
    {
        float volume = effectSlider.value;
        musicMixer.SetFloat(nameEffectSound, Mathf.Log(volume)*20);         //chuyen doi logarit ve tuyen tinh
        PlayerPrefs.SetFloat(nameEffectSound, volume);
    }

    //load am thanh khi moi vao
    private void loadVolume()
    {
        backgroundSlider.value = PlayerPrefs.GetFloat(nameBackgroundSound);
        setBackgroundSound();
        
        effectSlider.value = PlayerPrefs.GetFloat(nameEffectSound);
        setEffectSound();
    }
   
}
