using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class green_touch : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag =="Player"||collision.gameObject.tag == "Enemy")
        {
            animator.SetTrigger("Green_Touch");
        }
    }

}
