using System;
using System.Collections;
using UnityEngine;

public class EnemyBoomMoveAI : MonoBehaviour
{
    public float attackDistance = 2f;// gioi han tan cong
    public float moveSpeed;
    private float moveSpeedClone;
    public Transform LimitRight;
    public Transform LimitLeft;
    [HideInInspector] public Transform target;
    [HideInInspector] public bool inRange = false;
    [HideInInspector] public Transform player;

    [SerializeField] private bool attackComple = true;

    private Animator anim;

    [Header("enemy boom attack sound")]
    [SerializeField] private AudioSource boomEnemyAttackSound;

    private void OnEnable()
    {
        player = null;
        attackComple = true;
        if (moveSpeed != 0) return;

        moveSpeed = moveSpeedClone;
      
    }

    private void Start()
    {
        attackComple = true;
        target = LimitRight;
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (player == null && inRange)
        {
            player = GetPosPlayer.Instance.PlayerPos;
            return;
        }

        if (!attackComple) return; //bien check xem da thuc hien tan cong xong chua

        if (!inRange)               //khong co player thuc hien run binh thuong
        {
            Move();

            SelectTarget();
        }
        else
        {
            player = GetPosPlayer.Instance.PlayerPos;
            EnemyLogic();
        }
    }

    private void EnemyLogic()
    {
        if (!checkAttackMode())
        {
            truyduoi();
        }
        else
        {
            Attack();
        }
    }


    //kiem tra khoang cach de tan cong
    private bool checkAttackMode()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackDistance)
        {
            return false;
        }
        if (distance <= attackDistance)
        {
            return true;
        }
        return false;
    }


    // di chuyen tuan tra
    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.position.x, transform.position.y), moveSpeed * Time.deltaTime);
    }


    //doi huong khi di chuyen den gioi han khu vuc
    public void SelectTarget()
    {
        if (Mathf.Abs(transform.position.x - LimitLeft.position.x) <= .1f)
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

    private void Attack()
    {
        anim.SetInteger("state", 1);
    }

    public void stopMoveDelayAttack()
    {
        attackComple = false;
    }
    public void stopAttack()
    {
        
        anim.SetInteger("state", 0);
        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
        attackComple = true;
    }

    public void playSoundAttack()
    {
        boomEnemyAttackSound.Play();
    }
}
