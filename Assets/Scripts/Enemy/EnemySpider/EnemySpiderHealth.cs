using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpiderHealth : EnemyHealth
{
    [Header("pos play sound death")]
    [SerializeField] private Transform pointPlayDeathSound;

    protected override void PlayDieSound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, pointPlayDeathSound.position);
    }

    public override void EnemyTakeDame(int damePlayer)
    {
        LeanTween.moveLocalX(gameObject, 0.4f, 0.2f).setEaseShake();
        base.EnemyTakeDame(damePlayer);
    }
}
