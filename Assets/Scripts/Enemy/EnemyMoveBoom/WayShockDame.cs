using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayShockDame : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //neu song va vao nhan vat nhan vat se mat hp
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<HealthPlayer>().takeDame(1);
        }
    }
}
