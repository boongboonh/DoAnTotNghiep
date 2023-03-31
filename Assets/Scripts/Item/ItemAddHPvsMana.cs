using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAddHPvsMana : MonoBehaviour
{
    [SerializeField] private GameObject effectRingPower;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthPlayer>().addHealth();
            collision.gameObject.GetComponent<ManaManager>().addMana();
            GameObject cloneEffect = Instantiate(effectRingPower, transform.position, Quaternion.identity);
            Destroy(cloneEffect, 2f);
            Destroy(gameObject);
        }
    }
    
}
