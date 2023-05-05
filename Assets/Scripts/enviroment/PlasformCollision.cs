using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasformCollision : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.SetParent(gameObject.transform, true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.SetParent(GameObject.Find("PlayerPos").transform, true);
        }
    }
}
