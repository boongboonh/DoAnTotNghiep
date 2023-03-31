using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    [SerializeField] private float thrust;
    [SerializeField] private float timeKnockBack = .2f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            KnockBackToCall(gameObject.GetComponent<Rigidbody2D>());
        }
    }


    public void KnockBackToCall(Rigidbody2D objectKnockBack)
    {
        StartCoroutine(KnockCoroutine(objectKnockBack));
    }
    private IEnumerator KnockCoroutine(Rigidbody2D objectKnockBack)
    {
        Vector2 forceDirection = objectKnockBack.transform.position - transform.position;
        Vector2 force = forceDirection.normalized * thrust;

        objectKnockBack.velocity = force;
        yield return new WaitForSeconds(timeKnockBack);

        objectKnockBack.velocity = new Vector2();
    }
}
