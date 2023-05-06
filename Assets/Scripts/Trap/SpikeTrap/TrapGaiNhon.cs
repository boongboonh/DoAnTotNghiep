using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapGaiNhon : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerknockBack>().knockBackFuntion(transform);
            collision.GetComponent<HealthPlayer>().takeDame(1);
        }
    }
}
