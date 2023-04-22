using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerknockBack : MonoBehaviour
{

    public float knockbackForce = 20f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Bullet"))
        {
            //tinhs goc nhann dame
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            //gameObject.GetComponent<Rigidbody2D>().AddForce(pushDirection * knockbackForce, ForceMode2D.Impulse);
            gameObject.GetComponent<Rigidbody2D>().AddForce(Vector3.ClampMagnitude(pushDirection * knockbackForce, 15f), ForceMode2D.Impulse);
        }
    }

    
}
