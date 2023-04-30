using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerAreaBoss : MonoBehaviour
{
    [SerializeField] GameObject bossCore;
    [SerializeField] GameObject healthBar;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //kich hoat boss
            bossCore.SetActive(true);
            healthBar.SetActive(true);

            bossCore.GetComponent<BossLogic>().ActiveAttackBoss();
            bossCore.GetComponent<BossLogic>().inRanger = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bossCore.GetComponent<BossLogic>().inRanger = false;
            //vo hieu hoa boss
            bossCore.SetActive(false);
            healthBar.SetActive(false);
        }
    }
}
