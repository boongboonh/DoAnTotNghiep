using UnityEngine;

public class MenuSoundEffects : MonoBehaviour
{
    public void PlayButtonSound() => AudioManager.Play(SoundId.UIOpen);
}
