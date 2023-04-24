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
}
