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
        IdleEnemySoundRun();
    }

    private void IdleEnemySoundRun()
    {
        AudioManager.PlaySource(idleEnemySound, true);
    }


    protected override void PlayDieSound(AudioClip clip)
    {
        AudioManager.PlayClipAt(clip, pointPlayDeathSound.position);
        //enemyDeathSound.Play();
    }

    public override void EnemyTakeDame(int damePlayer)
    {
        LeanTween.moveLocalX(gameObject, 0.2f,0.1f).setEaseShake();
        base.EnemyTakeDame(damePlayer);
    }
}
