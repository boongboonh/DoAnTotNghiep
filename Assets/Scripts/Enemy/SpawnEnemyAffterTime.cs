using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemyAffterTime : MonoBehaviour
{

    //script spawn quai dat o object parent de kiem soat quai
    public float timeSpawn = 10f;
    private float timer;
    public GameObject enemyNeedSpawn;
    public Transform pointSpawn;
    public int hp = 0;

    private void Start()
    {
        timer = timeSpawn;
        // lay hp quai 
        getHealthEnemy();
    }

    private void Update()
    {
        checkHealth();

        //thoi gian = 0 va hp <=0 set active quai true
        if (timer <= 0 && hp <= 0)
        {
            enemyNeedSpawn.transform.SetPositionAndRotation(pointSpawn.position,Quaternion.identity);
            timer = timeSpawn;
            enemyNeedSpawn.SetActive(true);
        }
    }

    //kiem tra hp hien tai neu hp==0 thi chay thoi gian
    private void checkHealth()
    {
        getHealthEnemy();
        if (hp > 0) return;
        coolDownSpawn();
    }

    //chay thoi gian spawn quai
    private void coolDownSpawn()
    {
        timer -= Time.deltaTime;
    }
    
    //lay hp quai
    protected virtual void getHealthEnemy()
    {
        hp = enemyNeedSpawn.GetComponent<EnemyHealth>().healthEnemy;
    }

}
