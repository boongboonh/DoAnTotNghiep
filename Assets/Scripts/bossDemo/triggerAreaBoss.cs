using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerAreaBoss : MonoBehaviour
{
    [SerializeField] GameObject bossCore;
    [SerializeField] GameObject healthBar;
    private string nameBossPlayerPrefs = "BossPlayerPrefs";      //bien luu trang thai da danh bai boss
    private bool bossDie = false;

    private void Start()
    {


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<CreateCheckPoint>().isDistanceToCreate = false;      //khong cho tao diem luu game


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
            collision.GetComponent<CreateCheckPoint>().isDistanceToCreate = true;      //khong cho tao diem luu game

            bossCore.GetComponent<BossLogic>().inRanger = false;
            //vo hieu hoa boss
            bossCore.SetActive(false);
            healthBar.SetActive(false);
        }
    }
}
