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

    [Header("audio sound")]
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private AudioSource idleSound;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }


    private void FixedUpdate()
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
        timer += Time.fixedDeltaTime;
    }

    private void playIdleSound()
    {
        if (idleSound.isPlaying) return;
        idleSound.Play();
    }
   

    private void updateAnimation()
    {
        if (shoot)
        {
            animator.SetInteger("state", 1);
        }
        else
        {
            //chay am thanh idle khi khoong tan cong
            playIdleSound();

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
        //chay am thanh ban
        shootSound.Play();

        Instantiate(BulletNomal, pointShoot.position, transform.parent.rotation);
    }
}
