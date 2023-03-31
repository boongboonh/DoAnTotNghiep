using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    private Vector2 pointSave;
    //public Transform pointFirtPlay;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        //lay vi tri nguoi choi
        if (collision.CompareTag("Player"))
        {
            pointSave = collision.gameObject.transform.position;
        }
    }


}
