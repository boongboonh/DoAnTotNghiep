using System;
using System.Collections;
using UnityEngine;

public class EnemyJumpMove : MonoBehaviour
{
    [Header("setting jump move")]
    public float ForceJumpMovex = 2f;
    public float ForceJumpMovey = 5f;

    [Header("setting attack")]
    public float ForceJumpx = 2f;
    public float ForceJumpy = 2f;
    private float direction = 1f;
    public float speedRushAttack = 10f;
    private Transform player;
    bool isattack = false;
    bool attackNow = false;
    bool readyAttack = false;
    public Transform LimitRight;
    public Transform LimitLeft;

    [HideInInspector] public bool inRange = false;
    public float DelayAttack = 1f;

    private float gravity;

    private Vector2 playerPosClone;
    Animator _animator;
    private Rigidbody2D _rb;
    public LayerMask layerGround;
    Collider2D colli;

    [Header("sound enemy rush")]
    [SerializeField] private AudioSource jumpMoveSound;
    [SerializeField] private AudioSource attackSound;



    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        colli = GetComponent<Collider2D>();
        gravity = _rb.gravityScale;
    }

    private void OnEnable()
    {
        player = null;
    }

    private void OnDisable()
    {
        _rb.gravityScale = gravity;
    }
    void Update()
    {
        if (player == null && inRange)
        {
            player = GetPosPlayer.Instance.PlayerPos;
            return;
        }

        if (inRange && IsGround())//neu trong player trong khu vuc thi tan cong
        {
            player = GetPosPlayer.Instance.PlayerPos;
            isattack = true;
            JumpAttack();
        }

        if (!inRange && IsGround())
        {
            if (transform.position.x - LimitLeft.position.x <= 0)
            {
                direction = Mathf.Abs(direction);
            }
            if (transform.position.x - LimitRight.position.x >= 0)
            {
                direction = -1f;
            }
            jumpNomal();
        }

        Attack();

        if (attackNow)
        {
            transform.position = Vector2.MoveTowards(transform.position,playerPosClone, speedRushAttack * Time.deltaTime);
        }

        checkDistancePlayer();

        updateAnimation();
    }

    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(colli.bounds.center, colli.bounds.size, 0f, Vector2.down, ExtraHight, layerGround);
        return raycastHit2D.collider != null;
    }
    void checkDistancePlayer()
    {
        if (Vector2.Distance(transform.position, playerPosClone) < 0.1f)
        {
            attackNow = false;
        }
    }
    void jumpNomal()
    {
        _rb.velocity = new Vector2(ForceJumpMovex* direction, ForceJumpMovey);

        //chay am thanh nhay
        playJumpMove();
    }

    //am thanh
    private void playJumpMove()
    {
        if (jumpMoveSound.isPlaying) return;
        jumpMoveSound.Play();
    }

 
    void JumpAttack()
    {
        _rb.velocity = new Vector2(ForceJumpx * checkdirection(direction), ForceJumpy);

    }
    
     void updateAnimation()
    {
        if (_rb.velocity.y > .1f)
        {
            _animator.SetBool("EnemyRushIdle", false);
            _animator.SetBool("EnemyRushJump",true);
        }
        else if(_rb.velocity.y<-.1f)
        {
            _animator.SetBool("EnemyRushIdle", true);
            _animator.SetBool("EnemyRushJump", false);
        }
  
        if (readyAttack)
        {
            _animator.SetBool("EnemyRushDelayAttack", true);
            _animator.SetBool("EnemyRushJump", false);
        }
        if (attackNow)
        {
            _animator.SetBool("EnemyRushDelayAttack", false);
            _animator.SetBool("EnemyRushIdle", true);
        }

    }

    float checkdirection(float direc)
    {
        if (transform.position.x - player.position.x <= 0)
        {
            return Mathf.Abs(direc); // player o ben phai
        }
        else
        {
            return -1; // player o ben trai
        }
    }
    void Attack()
    {
        if (_rb.velocity.y < -0.2f && isattack)
        {
            isattack = false;
            _rb.velocity = Vector2.zero;
            StartCoroutine(AttackRush());
        }
    }
    IEnumerator AttackRush()
    {
        _rb.gravityScale = 0;
        
        readyAttack = true;
        yield return new WaitForSeconds(1f);

        playerPosClone = player.position;// lay vi tri player

        //chay am thanh tan cong
        attackSound.Play();


        readyAttack = false;
        attackNow = true;
        _rb.gravityScale = gravity;
        
    }

    
}
