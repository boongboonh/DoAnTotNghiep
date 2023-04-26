using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveStory : MonoBehaviour
{
    private Rigidbody2D rb;

    private Animator animator;
    private BoxCollider2D coll;

    [SerializeField] private GameObject PlayerIMG;

    [SerializeField] private LayerMask jumpableGround;

    private bool faceRight = true;

    //chi so di chuyen
    private float dirX = 0f;

    //chay animation nhan doi
    [SerializeField] private float timeStartIdleSpecial = 10f;
    float timeCountDown = 0f;


    //toc do di chuyen
    [SerializeField] private float speedMove = 7f;


    public bool isMove = true;

    [Header("Jump setting")]
    //luc nhay
    [SerializeField] private float jumpStrong = 7f;
    private float jumpTimeCounter;
    public float jumpTime;
    private bool isJumping;

    //bien trang thai
    private enum MovementState { idle, run, jump, fall, dash, specialIdle }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = PlayerIMG.GetComponent<Animator>();
    }

    private void Start()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (!isMove) return;        //bien khong cho di chuyen

        //di chuyen phai di chuyen trai
        dirX = Input.GetAxisRaw("Horizontal");


        //nhay
        Jump();


        //di chuyen
        Move();


        checkFlip();
    }

    private void FixedUpdate()
    {
        if (Input.anyKeyDown)
        {
            timeCountDown = 0f;
        }
        updateAnimation();
    }
    private void Jump()
    {

        //dieu khien nhay trong space => trong edit> project setting
        if (Input.GetKeyDown(KeyCode.Space) && IsGround())
        {
            //chay am thanh  nhay
            effectSoundJump();

            isJumping = true;
            jumpTimeCounter = jumpTime;
            player_jump_acti(jumpStrong);
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                player_jump_acti(jumpStrong);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(dirX * speedMove, rb.velocity.y);
    }

    public void player_jump_acti(float strong)
    {
        rb.velocity = new Vector3(0, strong, 0);
    }

    private void checkFlip()
    {
        // check quay dau
        if (dirX > 0 && !faceRight)
        {
            //state = MovementState.run;
            //sprite.flipX = false;
            flip();
        }
        else if (dirX < 0 && faceRight)
        {
            //state = MovementState.run;
            //sprite.flipX = true;
            flip();
        }
    }

    //dieu khien animation
    private void updateAnimation()
    {
        MovementState state;

        //chay animation
        if (dirX != 0 && IsGround())
        {
            state = MovementState.run;

            //am thanh
            effectSoundRun();
        }
        else
        {
            state = MovementState.idle;
        }

        //animation jump
        if (rb.velocity.y > 0.2f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -0.2f)
        {
            state = MovementState.fall;
        }

        if (rb.velocity == Vector2.zero && CheckTime())
        {
            Debug.Log("chay animtion dacj biet");
            state = MovementState.specialIdle;
        }

        animator.SetInteger("state", (int)state);
    }

    bool CheckTime()
    {
        if (timeStartIdleSpecial > timeCountDown)
        {
            timeCountDown += Time.fixedDeltaTime;
        }
        else
        {
            timeCountDown = 0f;
            return true;
        }
        return false;
    }

    //check cham dat
    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, ExtraHight, jumpableGround);
        return raycastHit2D.collider != null;
    }

    private void flip()
    {
        faceRight = !faceRight;

        transform.Rotate(0f, 180f, 0f);

    }

    private void effectSoundRun()
    {
        //am thanh
        PlayerSounds.instance.PlayRunAudio();
    }
    private void effectSoundJump()
    {
        //am thanh
        PlayerSounds.instance.PlayJumpAudio();
    }
}
