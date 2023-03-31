using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMana : MonoBehaviour
{
    //hoi mana cho nguoi choi
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<ManaManager>().ManaHealing();
            Destroy(gameObject);
        }
    }
}
