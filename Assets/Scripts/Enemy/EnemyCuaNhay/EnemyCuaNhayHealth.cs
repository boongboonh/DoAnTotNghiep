using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCuaNhayHealth : EnemyHealth
{
   
    [SerializeField] private GameObject[] enemyDrop;

    [Header("pos play sound death")]
    [SerializeField] private Transform pointPlayDeathSound;

    protected override void DropItem()
    {
        base.DropItem();


        for (int i = 0; i < enemyDrop.Length; i++)
        {
            enemyDrop[i].gameObject.SetActive(true);
            enemyDrop[i].transform.position = transform.position + new Vector3(Random.Range(-0.7f, 0.7f), 1, 0);
        }
    }

   

    protected override void PlayDieSound(AudioClip clip)
    {
        AudioManager.PlayClipAt(clip, pointPlayDeathSound.position);
    }

}
