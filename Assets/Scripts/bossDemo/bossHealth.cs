using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossHealth : MonoBehaviour
{
    [SerializeField] private GameObject effectBossDie;
    [SerializeField] private int healthBossMax = 5;

    public int HealthBossMax => healthBossMax;

    public int healthEnemy;
    [SerializeField] protected GameObject[] ItemsDrop;
    [SerializeField] private int dameTake = 1;

    [Header("sound setting")]
    [SerializeField] AudioSource hurtBossSound;
    [SerializeField] AudioSource dieBossSound;

    private AudioClip audioClipDeathClip;

    private void Start()
    {
        audioClipDeathClip = dieBossSound.clip;
    }

    protected virtual void OnEnable()
    {
        healthEnemy = healthBossMax;
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet")||collision.CompareTag("PlayerSword"))
        {
            EnemyTakeDame(dameTake);
        }
    }

    public void EnemyTakeDame(int damePlayer)
    {
        //am thanh
        soundHurt();

        if (healthEnemy <= damePlayer)
        {
            healthEnemy -= damePlayer;
            enemyDie();

        }
        else
        {
            healthEnemy -= damePlayer;
        }
    }

    protected void enemyDie()
    {
        //am thanh
        soundDie();


        GameObject EffectEnemyClone = Instantiate(effectBossDie, transform.position, Quaternion.identity);
        Destroy(EffectEnemyClone, 2f);
        gameObject.SetActive(false);
        dropItem();
    }

    protected void dropItem()       
    {
        for (int i = 0; i < ItemsDrop.Length; i++)
        {
            Instantiate(ItemsDrop[i], transform.position + new Vector3(Random.Range(-0.7f, 0.7f), 1, 0), Quaternion.identity);
        }
    }

    private void soundHurt()
    {
        if (hurtBossSound.isPlaying) return;
        hurtBossSound.Play();
    }

    private void soundDie()
    {
        AudioSource.PlayClipAtPoint(audioClipDeathClip, gameObject.transform.position);
    }

}
