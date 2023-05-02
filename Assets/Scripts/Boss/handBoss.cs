using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handBoss : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<HealthPlayer>().takeDame(2);             //gay dam neu cham phai
        }

        if (collision.CompareTag("PlayerBullet"))
        {
            collision.GetComponent<bulletPlayer>().destroyBullet();         //pha huy dan ban den neu cham chung
        }

    }
}
