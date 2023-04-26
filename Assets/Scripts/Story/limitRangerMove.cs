using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class limitRangerMove : MonoBehaviour
{
    [SerializeField] private GameObject NotificationUi;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            NotificationUi.SetActive(true);

            //tat thong bao sau 2 giay
            Invoke("hideNotification", 2f);
        }
    }


    //an thong bao
    private void hideNotification()
    {
        NotificationUi.SetActive(false);
    }
}
