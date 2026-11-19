using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyHealth : MonoBehaviour, IDamageable
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
    protected virtual void EnemyDie()
    {
        GameObject EffectEnemyClone = Instantiate(EffectRingEnemyDie, transform.position, Quaternion.identity);
        Destroy(EffectEnemyClone, 2f);
        gameObject.SetActive(false);
        DropItem();
        Died?.Invoke(this);
    }

    /// <summary>Ban khi bat ky quai nao chet (SapDirector va he thong khac nghe de thuong/roi do).</summary>
    public static event System.Action<EnemyHealth> Died;

   /* private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            EnemyTakeDame(dameTake);
        }
    }*/

    /// <summary>IDamageable: combat moi goi vao day, chuyen ve EnemyTakeDame (cac lop con van override nhu cu).</summary>
    public void TakeDamage(int amount, Vector2 hitPoint) => EnemyTakeDame(amount);

    public virtual void EnemyTakeDame(int damePlayer)
    {
        
        if (healthEnemy <= damePlayer)
        {
            //am thanh chet
            PlayDieSound(audioClipDeathClip);

            healthEnemy -= damePlayer;
            EnemyDie();
        }
        else
        {
            //am thanh take dame
            AudioManager.PlaySource(enemyHurtSound);

            healthEnemy -= damePlayer;
        }
    }

    protected virtual void PlayDieSound(AudioClip clip)
    {
        AudioManager.PlayClipAt(clip, this.gameObject.transform.position);
    }

    protected virtual void DropItem()
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
