using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wayShock : MonoBehaviour
{

    public float speed = 4f;
    public float time = 3f;
    public bool left = true;
    private void Start()
    {
        if (!left)
        {
            speed = -speed;
        }
    }

    void Update()
    {
        move();
        Destroy(gameObject, time);
    }

    private void move()
    {
        transform.Translate(Vector2.left * Time.deltaTime * speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //neu cham vao player tru 1 hp
            collision.GetComponent<HealthPlayer>().takeDame(1);
        }
    }
}
