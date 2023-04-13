using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockDropTrap : MonoBehaviour
{
    [SerializeField] float speedUp = 4f;
    [SerializeField] float speedDown = 8f;
    [SerializeField] Transform pointUp;
    [SerializeField] Transform pointDown;
    bool drop;

    private void FixedUpdate()
    {
        logic();
    }

    private void logic()
    {
        if (transform.position.y >= pointUp.position.y)
        {
            drop = true;
        }
        if(transform.position.y <= pointDown.position.y)
        {
            drop = false;
        }
        if (drop)
        {
            transform.position = Vector2.MoveTowards(transform.position, pointDown.position, speedDown * Time.deltaTime);
        }else
        {
            transform.position = Vector2.MoveTowards(transform.position, pointUp.position, speedUp * Time.deltaTime);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<HealthPlayer>().killPlayer();
        }
    }


}
