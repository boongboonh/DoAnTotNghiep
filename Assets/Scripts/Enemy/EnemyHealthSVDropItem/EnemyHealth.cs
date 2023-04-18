using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int healthEnemyMax = 5;

    public int healthEnemy;
    [SerializeField] protected GameObject[] ItemsDrop;
    [SerializeField] private int dameTake = 1 ;
    [SerializeField] private GameObject EffectRingEnemyDie;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            EnemyTakeDame(dameTake);
        }
    }

    public void EnemyTakeDame(int damePlayer)
    {
        
        if (healthEnemy <= 1)
        {
            healthEnemy -= damePlayer;
            enemyDie();
        }
        else
        {
            healthEnemy -= damePlayer;
        }
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
