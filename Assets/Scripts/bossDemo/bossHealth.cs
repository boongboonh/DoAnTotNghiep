using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossHealth : MonoBehaviour
{
    public int healthBoss = 20;

    public int currentHealthBoss = 0;
    [SerializeField] protected GameObject[] ItemsDrop;
   
    private void OnEnable()
    {
        currentHealthBoss = healthBoss;
    }

    
    void takeDame(int dame)
    {
        if(currentHealthBoss > 1)
        {
            currentHealthBoss -= dame;
        }
        else
        {
            bossDie();
        }
    }

    void bossDie()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //trung dan tru 1 hp
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            takeDame(1);
        }
    }

    protected virtual void dropItem()
    {
        for (int i = 0; i < ItemsDrop.Length; i++)
        {
            Instantiate(ItemsDrop[i], transform.position + new Vector3(Random.Range(-0.7f, 0.7f), 1, 0), Quaternion.identity);
        }
    }


}
