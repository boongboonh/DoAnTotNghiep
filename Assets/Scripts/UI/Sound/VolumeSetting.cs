using GF.Save;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>Slider am luong trong menu. Luu va ap dung qua AudioManager (cung key PlayerPrefs cu).</summary>
public class VolumeSetting : MonoBehaviour
{
    [SerializeField] AudioMixer musicMixer;         // giu field cu (khong con dung, AudioManager giu mixer)
    [SerializeField] Slider backgroundSlider;
    [SerializeField] Slider effectSlider;

    [SerializeField] float volumeStart = 1f;

    void Start()
    {
        float music = PlayerPrefsSaveSystem.HasKey(AudioManager.KeyMusic) ? AudioManager.GetVolume(SoundBus.Music) : volumeStart;
        float effect = PlayerPrefsSaveSystem.HasKey(AudioManager.KeyEffect) ? AudioManager.GetVolume(SoundBus.Effect) : volumeStart;

        // dat gia tri slider truoc khi gan su kien de khong luu de gia tri ngoai y muon
        backgroundSlider.SetValueWithoutNotify(music);
        effectSlider.SetValueWithoutNotify(effect);
        AudioManager.SetVolume(SoundBus.Music, music);
        AudioManager.SetVolume(SoundBus.Effect, effect);
    }

    //goi trong slider background sound
    public void setBackgroundSound()
    {
        AudioManager.SetVolume(SoundBus.Music, backgroundSlider.value);
    }

    //goi trong slider effect sound
    public void setEffectSound()
    {
        AudioManager.SetVolume(SoundBus.Effect, effectSlider.value);
    }
}
