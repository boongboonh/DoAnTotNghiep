using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDashAI : MonoBehaviour
{

    [Header("setting dash attack")]
    public float speedDashAttack = 10f;
    public float speedDashMove = 2f;
    private float speed;
    private float newSpeed;

    
    public Transform LimitRight;
    public Transform LimitLeft;

    [HideInInspector] public bool inRange = false;
    public float cooldowAttack = 1.5f;
    
    private bool isAttack = false;
    private bool delayAttack = false;
    private Transform target;
    private Transform playerPos;

    Animator _animator;
    private void OnEnable()
    {
        playerPos = null;
        target = LimitLeft;

        speed = speedDashMove;

    }

    void Start()
    {
        target = LimitLeft;
        _animator = GetComponent<Animator>();
        speed = speedDashMove;
    }

    void Update()
    {
        if (playerPos == null && inRange)
        {
            playerPos = GetPosPlayer.Instance.PlayerPos;
            return;
        }

        if (!inRange)
        {
            newSpeed = speedDashMove;
            changeObjectTarget();
            TypeMove(speed);
        }
        else
        {
            gameLogic();
        }

        checkTouchTarget();
        updateAnimation();
    }

    void gameLogic()
    {

        if (!isAttack)
        {
            playerPos = GetPosPlayer.Instance.PlayerPos;
            changeOBjectTargetAttack();
            delayAttack = true;
        }
        else
        {
            TypeMove(speed);
        }
    }

    void updateAnimation()
    {
        if (delayAttack)
        {
            _animator.SetInteger("state", 1);
        }
        else
        {
            _animator.SetInteger("state", 0);
        }
    }
    private void changeObjectTarget()
    {
        if (transform.position.x - LimitLeft.position.x <= 0)
        {
            target = LimitRight;
        }
        else if (transform.position.x - LimitRight.position.x >= 0)
        {
            target = LimitLeft;
        }
        Flip();
    }

    private void TypeMove(float moveType)
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.position.x, transform.position.y), moveType * Time.deltaTime);
    }
   
    void checkTouchTarget()
    {
        if (Mathf.Abs(transform.position.x - target.position.x) <= 0.1f)
        {
            speed = newSpeed;
            isAttack = false;
            
        }
    }

    private void changeOBjectTargetAttack()
    {
        if (transform.position.x - playerPos.position.x <= 0 && !isAttack)
        {
            target = LimitRight;
        }
        else if (transform.position.x - playerPos.position.x >= 0 && !isAttack)
        {
            target = LimitLeft;
        }
        Flip();
    }

    public void Flip()
    {
        Vector2 rotation = transform.eulerAngles;
        if (transform.position.x > target.position.x)
        {
            rotation.y = 0f;
        }
        else
        {
            rotation.y = 180f;
        }
        transform.eulerAngles = rotation;
    }

    //ham goij ben animation

    public void startDelayAttack()
    {
        speed = 0f;
    }

    public void endDelayAttack()
    {
        speed = speedDashAttack;
        isAttack = true;
        delayAttack = false;
    }
}
