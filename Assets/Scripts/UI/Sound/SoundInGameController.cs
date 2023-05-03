using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInGameController : SoundMenuController
{
    private bool isFighting = false;

    [Header("sound fighting")]

    [SerializeField] private AudioSource soundFighting;


    protected override void Start()
    {
        EventManager.Instance.onMusicFighting += runMusicFighting;
        EventManager.Instance.offMusicFighting += stopMusicFighting;

        isFighting = false;
        base.Start();
    }
    protected override void FixedUpdate()
    {
        if (isFighting)
        {
            runFightingSound();                     //neu trong trang thai chien dau thi bat nhac chien dau
        }
        else
        {
            base.FixedUpdate();                     //neu khong thi thoi
        }
    }


    private void runFightingSound()
    {
        if (soundFighting.isPlaying) return;
        soundFighting.Play();
    }

    private void runMusicFighting()
    {

        StopSoundCurrent();         //tat am thanh hien tai
        isFighting = true;
    }
    private void stopMusicFighting()
    {
        soundFighting.Stop();         //tat am thanh hien tai
        isFighting = false;
    }

    private void OnDestroy()
    {
        EventManager.Instance.onMusicFighting -= runMusicFighting;
        EventManager.Instance.offMusicFighting -= stopMusicFighting;
    }
}
