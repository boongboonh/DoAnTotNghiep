using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPoint : MonoBehaviour
{
    [SerializeField] string isCreateCheckPoint = "isCreateCheckPoint";
    [SerializeField] string CheckPointPosX = "checkPointPosX";
    [SerializeField] string CheckPointPosY = "checkPointPosY";


    private void Start()
    {
        if (!PlayerPrefsSaveSystem.HasKey(CheckPointPosX))
        {
            //transform.position = new Vector2(PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosX"), PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosY"));
            updateNewPos();
        }
        else
        {
            transform.position = new Vector2(PlayerPrefsSaveSystem.LoadFloat(CheckPointPosX), PlayerPrefsSaveSystem.LoadFloat(CheckPointPosY));
        }
    }

    private void FixedUpdate()
    {
        if (PlayerPrefsSaveSystem.LoadInt(isCreateCheckPoint) != 1) return;
        PlayerPrefsSaveSystem.SetInt(isCreateCheckPoint, 0);                  //da cap nhat vi tri, reset bien
        updateNewPos();
    }

    //khoang cach la 5x2.4


    private void updateNewPos()     //di chuyen diem luu game den vi tri nguowi choi luu
    {
        transform.position = new Vector2(PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosX"), PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosY"));
        PlayerPrefsSaveSystem.SetFloat(CheckPointPosX, PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosX"));        //luu vitri checkpoint x
        PlayerPrefsSaveSystem.SetFloat(CheckPointPosY, PlayerPrefsSaveSystem.LoadFloat("FirstPlayPosY"));        //luu vitri y
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
