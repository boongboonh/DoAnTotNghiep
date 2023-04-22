using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPoint : MonoBehaviour
{
    [SerializeField] string isCreateCheckPoint = "isCreateCheckPoint";

    private void Start()
    {
        updateNewPos();
    }

    private void FixedUpdate()
    {
        if (PlayerPrefs.GetInt(isCreateCheckPoint) != 1) return;
        PlayerPrefs.SetInt(isCreateCheckPoint, 0);                  //da cap nhat vi tri, reset bien 
        updateNewPos();
    }

    //khoang cach la 5x2.4


    private void updateNewPos()     //di chuyen diem luu game den vi tri nguowi choi luu
    {
        transform.position = new Vector2(PlayerPrefs.GetFloat("FirstPlayPosX"), PlayerPrefs.GetFloat("FirstPlayPosY"));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<CreateCheckPoint>().isDistanceToCreate = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<CreateCheckPoint>().isDistanceToCreate = true;
        }
    }

}
