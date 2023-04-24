using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBarrieAll : EnemyHealth
{
    [SerializeField] private AudioSource idleEnemySound;

    [Header("pos play sound death")]
    [SerializeField] private Transform pointPlayDeathSound;

    private void FixedUpdate()
    {
        idleEnemySoundRun();
    }

    private void idleEnemySoundRun()
    {
        if (idleEnemySound.isPlaying) return;  //kiem tra am thanh co dang duoc phat hay khong
        idleEnemySound.Play();
    }


    protected override void PlayDieSound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, pointPlayDeathSound.position);
        //enemyDeathSound.Play();
    }
}
