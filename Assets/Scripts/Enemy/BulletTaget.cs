using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTaget : MonoBehaviour
{
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float timeMaintain = 6f;
    private Rigidbody2D rb;

    private Transform playerTaget;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTaget = GetPosPlayer.Instance.PlayerPos;
        moveDirection = (playerTaget.transform.position - transform.position).normalized * moveSpeed;
        rb.velocity = new Vector2(moveDirection.x, moveDirection.y);
        Destroy(gameObject, timeMaintain);


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Player"))
        {
            Debug.Log("bullet taget player take dame");
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
