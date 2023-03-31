using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpiderAI : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] float timeAttackRate = 1.5f;
    [SerializeField] float speedRotate = 3f;

    [SerializeField] private Transform pointShooting;
    [SerializeField] private Transform pointHead;


    [HideInInspector] public bool inRange = false;


    private float timer = 0f;
    private Transform playerTarget;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        updateHead();
        updateAnimation();


        if (!inRange) return;
       

        if (inRange && timer >= timeAttackRate)
        {
            animator.SetInteger("state", 2);
        }

    }

    private void updateAnimation()
    {
        if (!inRange)
        {
            animator.SetInteger("state", 0);
        }
        else
        {
            animator.SetInteger("state", 1);
            timer += Time.deltaTime;
        }
    }

    // ham goi ben animation
    private void updateHead()
    {
        if(inRange)
        {
            playerTarget = GetPosPlayer.Instance.PlayerPos; //lay vi tri nguowi choi tu getposplayer

            //tinh goc
            Vector2 vectorToTarget = playerTarget.position - pointHead.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + 90;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            pointHead.rotation = Quaternion.Slerp(pointHead.rotation, q, Time.deltaTime * speedRotate);
        }
        else
        {
            pointHead.rotation = Quaternion.Slerp(pointHead.rotation, Quaternion.AngleAxis(0,Vector3.forward), Time.deltaTime * speedRotate);
        }
    }

    public void resetCondown()
    {
        timer = 0f;
    }
    public void Attack()
    {
        Instantiate(bullet, pointShooting.position,pointShooting.rotation);
    }
}
