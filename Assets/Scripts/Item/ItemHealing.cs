using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHealing : MonoBehaviour
{
    //hoi mau cho nguoi choi
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthPlayer>().healing();
            Destroy(gameObject);
        }
    }
}
