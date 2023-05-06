using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerknockBack : MonoBehaviour
{

    [SerializeField] private float knockbackForce = 40f;

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Bullet"))
        {
            knockBackFuntion(collision.transform);
        }
    }

    public void knockBackFuntion(Transform damageObject)
    { 
        //tinhs goc nhann dame
        Vector2 pushDirection = (transform.position - damageObject.position).normalized;

        //pushDirection = new Vector2(Mathf.Clamp(pushDirection.x, -0.8f, 0.8f), Mathf.Clamp(pushDirection.y, 0.2f, 0.8f));
        //dat gravity ve nhu cu
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 3f;

        gameObject.GetComponent<Rigidbody2D>().AddForce(Vector3.ClampMagnitude(pushDirection * knockbackForce, 15f), ForceMode2D.Impulse);
    }
}
