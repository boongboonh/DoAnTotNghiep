using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerknockBack : MonoBehaviour
{

    public float knockbackForce = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //tinhs goc nhann dame
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            knockbackDirection.x *= -1;
            GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
