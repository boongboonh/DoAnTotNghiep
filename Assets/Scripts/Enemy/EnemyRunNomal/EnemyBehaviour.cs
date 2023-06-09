using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsFacingRight())
        {
            _rb.velocity = new Vector2(moveSpeed, 0f);
        }
        else
        {
            _rb.velocity = new Vector2(-moveSpeed, 0f);
        }

    }
    private bool IsFacingRight()
    {
        return transform.localScale.x > Mathf.Epsilon;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
       
       transform.localScale = new Vector2(-(Mathf.Sign(_rb.velocity.x)), transform.localScale.y);
      
    }
}
