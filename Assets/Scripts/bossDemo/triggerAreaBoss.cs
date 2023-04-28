using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerAreaBoss : MonoBehaviour
{
    [SerializeField] GameObject bossCore;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //kich hoat boss
        bossCore.GetComponent<BossLogic>().ActiveAttackBoss();
        bossCore.GetComponent<BossLogic>().inRanger = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //vo hieu hoa boss
        bossCore.GetComponent<BossLogic>().inRanger = false;
    }
}
