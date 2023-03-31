using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAreaDashCheck : MonoBehaviour
{
    private EnemyDashAI enemyDashAIChild;
    void Start()
    {
        enemyDashAIChild = GetComponentInChildren<EnemyDashAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            enemyDashAIChild.inRange = true;
        }   
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            enemyDashAIChild.inRange = false;
        }
    }
}
