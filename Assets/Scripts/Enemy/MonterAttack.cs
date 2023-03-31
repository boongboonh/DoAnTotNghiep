using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonterAttack : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] float AttackRate=1.5f;
    [SerializeField] float distanceBetween;

    private Transform playerTaget;
    private float distance;
    private float cooldow=0f;

    private void Start()
    {
        //playerTaget = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        playerTaget = GetPosPlayer.Instance.PlayerPos; //lay vi tri nguowi choi tu getposplayer

        cooldow += Time.deltaTime;

        if (playerTaget!=null && checkTime())
        {
            distance = Vector2.Distance(transform.position, playerTaget.transform.position);
            if (distance < distanceBetween)
            {
                Attack();
                cooldow = 0f;
            }
            
        }
    }

    private void Attack()
    {
        Instantiate(bullet, transform.position, Quaternion.identity);
    }

    private bool checkTime()
    {
        if(cooldow>= AttackRate)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceBetween);
    }
}
