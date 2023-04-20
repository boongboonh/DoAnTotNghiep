using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPoint : MonoBehaviour
{

    private void Start()
    {
        transform.position = new Vector2(PlayerPrefs.GetFloat("FirstPlayPosX"), PlayerPrefs.GetFloat("FirstPlayPosY"));
    }

    private void Update()
    {
        
    }


    //khoang cach la 5x2.4

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
