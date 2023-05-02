using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCheckAll : MonoBehaviour
{
    [SerializeField] GameObject triggerBoss;

    private void Start()
    {
        if (PlayerPrefs.GetInt("BoosDie") == 1)         //kiem tra boss da bi tieu diet lan nao chua
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<CreateCheckPoint>().isDistanceToCreate = false;      //khong cho tao diem luu game

            if (checkBoss()) return;                    //neu boss bi tieu diet truoc khi vao khu vuc
            EventManager.Instance.CloseDoor();

            Invoke("OpenTriggerBoss", 2f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<CreateCheckPoint>().isDistanceToCreate = true;      //khong cho tao diem luu game
            

            CloseTriggerBoss();

            if (checkBoss()) return;
            EventManager.Instance.OpenDoor();

        }
    }

    private void OpenTriggerBoss()
    {
        triggerBoss.SetActive(true);                                                //bat boss
    }
    private void CloseTriggerBoss()
    {
        triggerBoss.SetActive(false);                                               //tat boss
    }

    private bool checkBoss()
    {
        if (PlayerPrefs.GetInt("BoosDie") == 1)         //kiem tra boss da bi tieu diet lan nao chua
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
