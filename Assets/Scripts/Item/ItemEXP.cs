using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEXP : MonoBehaviour
{
    //tang kinh nghiem cho nguoi choi
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //collision.gameObject.GetComponent<HealthPlayer>().healing();

            //tang kinh nghiem
            Destroy(gameObject);
        }
    }
}
