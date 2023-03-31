using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpiderIstriggerArea : MonoBehaviour
{
    private EnemySpiderAI enemySpiderAIChild;
    void Start()
    {
        enemySpiderAIChild = GetComponentInChildren<EnemySpiderAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            enemySpiderAIChild.inRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            enemySpiderAIChild.inRange = false;
        }
    }
}
