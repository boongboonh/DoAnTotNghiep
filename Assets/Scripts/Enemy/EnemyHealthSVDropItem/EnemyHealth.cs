using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int healthEnemyMax = 10;

    public int healthEnemy;
    [SerializeField] protected GameObject[] ItemsDrop;
    [SerializeField] private GameObject EffectRingEnemyDie;

    [Header("audio enemy")]
    [SerializeField] private AudioSource enemyHurtSound; 
    [SerializeField] private AudioSource enemyDeathSound;

    private AudioClip audioClipDeathClip;

    private void Start()
    {
        audioClipDeathClip = enemyDeathSound.clip;
    }

    protected virtual void OnEnable()
    {
        healthEnemy = healthEnemyMax;
    }
  
    //khi quai chet tao hieu ung song no. tat quai, doi vat pham
    protected virtual void enemyDie()
    {
        GameObject EffectEnemyClone = Instantiate(EffectRingEnemyDie, transform.position, Quaternion.identity);
        Destroy(EffectEnemyClone, 2f);
        gameObject.SetActive(false);
        dropItem();
    }

   /* private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            EnemyTakeDame(dameTake);
        }
    }*/

    public virtual void EnemyTakeDame(int damePlayer)
    {
        
        if (healthEnemy <= damePlayer)
        {
            //am thanh chet
            PlayDieSound(audioClipDeathClip);

            healthEnemy -= damePlayer;
            enemyDie();
        }
        else
        {
            //am thanh take dame
            enemyHurtSound.Play();

            healthEnemy -= damePlayer;
        }
    }

    protected virtual void PlayDieSound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, this.gameObject.transform.position);
    }

    protected virtual void dropItem()
    {
        for (int i = 0; i < ItemsDrop.Length; i++)
        {
            //ti le 75%/1 vat pham roi
            if (Random.Range(0,3) > 0)
            {
                Instantiate(ItemsDrop[i], transform.position + new Vector3(Random.Range(-0.7f, 0.7f), 1, 0), Quaternion.identity);
            }
        }
    }
}
