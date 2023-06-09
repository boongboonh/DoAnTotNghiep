﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerDash _playerDash;

    private Animator animator;
    private BoxCollider2D coll;
    private bool faceRight = true;
    
    public bool Direction => faceRight;
    [SerializeField] private GameObject PlayerIMG;

    [SerializeField] private LayerMask jumpableGround;


    //chỉ số di chuyển
    private float dirX = 0f;

    //chay animation nhan doi
    [SerializeField] private float timeStartIdleSpecial = 10f;
    float timeCountDown = 0f;

    [Header("move setting")]
    [HideInInspector] public bool canMove = true;

    //tốc độ di chuyển 
    [SerializeField] private float speedMove = 7f;

    [Header("Jump setting")]
    //sức nhảy
    [SerializeField] private float jumpStrong = 7f;
    private float jumpTimeCounter;
    public float jumpTime;
    private bool isJumping;

    //biến trạng thái
    private enum MovementState { idle, run, jump, fall, dash ,specialIdle }

    private void Awake()
    {
        canMove = true;
        _playerDash = GetComponent<PlayerDash>();
        rb = GetComponent<Rigidbody2D>();
        animator = PlayerIMG.GetComponent<Animator>();
        animator.keepAnimatorControllerStateOnDisable = true;               //dat gia tri animation mac dinh de khi set active thi no dat ve mac dinh
    }

   
    private void Start()
    {
       coll = GetComponent<BoxCollider2D>();
        
    }
 
    private void Update()
    {
        if (!canMove) return;

        //di chuyển trái phải
        dirX = Input.GetAxisRaw("Horizontal");


        //nhay
        if (!_playerDash.IsDashing)
        {
            Jump();
        }

        //di chuyen
        if (!_playerDash.IsDashing)
        {
            Move();
           
        }
        
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

        //điều khiển nhảy bằng nút space => trong edit> project setting
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
        rb.velocity = new Vector2(dirX * speedMove , rb.velocity.y);
    }


    public void player_jump_acti(float strong)
    {
        rb.velocity = new Vector3(0, strong , 0);
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



    //Điều khiển animation
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

        //animation dash
        if (_playerDash.IsDashing)
        {
            state = MovementState.dash;
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
    
    //check mặt đất 
    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, ExtraHight, jumpableGround);
        return raycastHit2D.collider != null;
    }

    /*private bool checkGround(){

        isGrounded = Physics2D.OverlapCircle(feetpos.position, checkradius, layerMask);

    }*/
    private void flip()
    {
        faceRight = !faceRight;

        transform.Rotate(0f, 180f, 0f);

    }

    private void effectSoundRun()
    {
        //âm thanh
        PlayerSounds.instance.PlayRunAudio();
    }
    private void effectSoundJump()
    {
        //âm thanh
        PlayerSounds.instance.PlayJumpAudio();
    }
}
