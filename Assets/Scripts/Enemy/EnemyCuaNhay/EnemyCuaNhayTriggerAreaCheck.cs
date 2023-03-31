using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCuaNhayTriggerAreaCheck : MonoBehaviour
{
    private EnemyCuaNhayAI enemyChildren;
    private void Awake()
    {
        enemyChildren = GetComponentInChildren<EnemyCuaNhayAI>() as EnemyCuaNhayAI;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            enemyChildren.inRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            enemyChildren.inRange = false;
        }
    }
}
