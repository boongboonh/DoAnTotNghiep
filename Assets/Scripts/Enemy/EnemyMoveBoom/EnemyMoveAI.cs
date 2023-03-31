using System;
using System.Collections;
using UnityEngine;

public class EnemyMoveAI : MonoBehaviour
{
    public float attackDistance = 2f;// gioi han tan cong
    public float moveSpeed;
    private float moveSpeedClone;
    public Transform LimitRight;
    public Transform LimitLeft;
    [HideInInspector] public Transform target;
    [HideInInspector] public bool inRange = false;
    [HideInInspector] public Transform player;

    private bool onAttackMode = false;
    private Animator anim;

    private void Start()
    {
        target = LimitRight;
        anim = GetComponent<Animator>();
    }
    private void Update()
    {

        if (!inRange)
        {
            
            Move();
        }
        else
        {
            player = GetPosPlayer.Instance.PlayerPos;
            EnemyLogic();
        }


        if (onAttackMode)
        {
            anim.SetInteger("state", 2);
        }

        SelectTarget();
    }

    private void EnemyLogic()
    {
        if (!checkAttackMode())
        {
            truyduoi();
        }
        else
        {
            anim.SetInteger("state", 1);
           
        }
    }

    private bool checkAttackMode()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackDistance)
        {
            return false;
        }
        else if (distance <= attackDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.position.x, transform.position.y), moveSpeed * Time.deltaTime);
    }

    public void SelectTarget()
    {
        if (Mathf.Abs(transform.position.x- LimitLeft.position.x) <= .1f)
        {
            target = LimitRight;
        }

        if (Mathf.Abs(transform.position.x - LimitRight.position.x) <= .1f)
        {
            target = LimitLeft;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }



    private void truyduoi()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.position.x, transform.position.y), moveSpeed * Time.deltaTime);
    }
   

    //goi trong animation

    public void stopMoveDelayAttack()
    {
        moveSpeedClone = moveSpeed;
        moveSpeed = 0;
    }

    public void StartAttack()
    {
        onAttackMode = true;
    }
    public void EndAttack()
    {
        onAttackMode = false;
        moveSpeed = moveSpeedClone;
        anim.SetInteger("state", 0);
    }
}
