using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDashHealth : EnemyHealth
{
    [Header("pos play sound death")]
    [SerializeField] private Transform pointPlayDeathSound;

    protected override void PlayDieSound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, pointPlayDeathSound.position);
    }

    public override void EnemyTakeDame(int damePlayer)
    {
        LeanTween.moveLocalY(gameObject, 0.1f, 0.1f).setEaseShake();
        base.EnemyTakeDame(damePlayer);
    }
}
