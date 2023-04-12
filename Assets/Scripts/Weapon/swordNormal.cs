using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordNormal : MonoBehaviour
{
    [SerializeField] private int ATK = 1;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            //neeu cham vao quai, quais awn 1 dame
            Debug.Log("enemyTakeDame");
            collision.GetComponent<EnemyHealth>().EnemyTakeDame(ATK);
        }
    }
}
