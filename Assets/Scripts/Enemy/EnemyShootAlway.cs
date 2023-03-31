using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootAlway : MonoBehaviour
{

    [SerializeField] private GameObject BulletNomal;
    [SerializeField] private Transform pointShoot;
    [SerializeField] private float delayBetweenShoot = 1f;
    private float timer = 0f;
    private bool shoot = true;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        updateAnimation();
        if (shoot) return;
        coolDown();
        if (timer >= delayBetweenShoot)
        {
            shoot = true;
            timer = 0f;
        }
        
    }

    public void coolDown()
    {
        timer += Time.deltaTime;
    }

   

    private void updateAnimation()
    {
        if (shoot)
        {
            animator.SetInteger("state", 1);
        }
        else
        {
            animator.SetInteger("state", 0);
        }
    }


    // cacs ham kich hoat ben animation
    public void stopAttack()
    {
        shoot = false;
    }

    public void AttackNow()
    {
        Instantiate(BulletNomal, pointShoot.position, transform.parent.rotation);
    }
}
