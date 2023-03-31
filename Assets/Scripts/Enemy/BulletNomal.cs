using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletNomal : MonoBehaviour
{

    [SerializeField] private float Speed = 5f;
    void Update()
    {
        transform.position += transform.up * Time.deltaTime * Speed;
        Destroy(gameObject, 5f);
    }
   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Debug.Log("bulletNomal player take dame");
            Destroy(gameObject);
        }
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
