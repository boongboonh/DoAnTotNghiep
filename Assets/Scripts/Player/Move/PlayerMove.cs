using UnityEngine;

/// <summary>
/// Di chuyen player dang state machine.
///   Idle / Run / Jump / Fall -> trang thai di chuyen, suy ra tu van toc, mat dat va input (giong logic anim cu).
///   SpecialIdle              -> dung yen lau (timeStartIdleSpecial) thi chay anim nhan doi; dung lai khi co input/di chuyen.
///   Dash                     -> chuot phai, dieu khien qua PlayerDash (gravity = 0, lao ngang trong DashingTime).
/// Thiet ke:
///   - Update: doc input (chot lai de FixedUpdate dung), lat mat, danh gia transition.
///   - FixedUpdate: kiem tra mat dat 1 lan / tick (cache), ap dung nhay + di chuyen len Rigidbody2D.
///   - Animator chi SetInteger khi doi state (Enter), dung hash, khong goi moi frame.
/// API ben ngoai giu nguyen: Direction, canMove, IsGround(), player_jump_acti().
/// </summary>
public class PlayerMove : StateMachineOwner<PlayerMove>
{
    [SerializeField] private GameObject PlayerIMG;
    [SerializeField] private LayerMask jumpableGround;

    //chay animation nhan doi
    [SerializeField] private float timeStartIdleSpecial = 10f;
    [SerializeField] private float specialIdleHold = 1.4f;          // >= do dai clip player_idle_special (1.32s)

    [Header("move setting")]
    [HideInInspector] public bool canMove = true;

    //toc do di chuyen
    [SerializeField] private float speedMove = 7f;

    [Header("Jump setting")]
    [SerializeField] private float jumpStrong = 7f;
    public float jumpTime;

    // anim state (giu nguyen gia tri cua Animator controller)
    private const int AnimIdle = 0, AnimRun = 1, AnimJump = 2, AnimFall = 3, AnimDash = 4, AnimSpecialIdle = 5;
    private const float VerticalThreshold = 0.2f;
    private const float GroundCheckExtra = 0.03f;
    private static readonly int StateParam = Animator.StringToHash("state");

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Animator animator;
    private PlayerDash dash;

    private bool faceRight = true;
    public bool Direction => faceRight;

    // input (doc o Update, dung o FixedUpdate)
    private float dirX;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpReleased;

    // trang thai vat ly (cache)
    private bool grounded;
    private Vector2 velocity;

    // nhay
    private bool isJumping;
    private float jumpTimeCounter;

    // idle dac biet
    private float idleTimer;
    private bool specialIdleReady;

    protected override void Awake()
    {
        base.Awake();
        canMove = true;
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        dash = GetComponent<PlayerDash>();
        animator = PlayerIMG.GetComponent<Animator>();
        animator.keepAnimatorStateOnDisable = true;               //dat gia tri animation mac dinh de khi set active thi no dat ve mac dinh
    }

    protected override void OnEnable()
    {
        dirX = 0f;
        jumpPressed = jumpHeld = jumpReleased = false;
        isJumping = false;
        idleTimer = 0f;
        specialIdleReady = false;
        grounded = CheckGround();
        velocity = rb.linearVelocity;

        base.OnEnable();
    }

    // ---------------- Setup state machine ----------------

    protected override void SetupStates(StateMachine<PlayerMove> m)
    {
        m.AddState<IdleState>();
        m.AddState<RunState>();
        m.AddState<JumpState>();
        m.AddState<FallState>();
        m.AddState<SpecialIdleState>();
        m.AddState<DashState>();
        m.SetInitialState<IdleState>();

        // Thu tu = do uu tien (AnyTransition duoc danh gia theo thu tu dang ky)
        m.AddAnyTransition<DashState>(DashRequested);
        m.AddAnyTransition<SpecialIdleState>(() => Free() && specialIdleReady && Machine.IsIn<IdleState>());
        m.AddAnyTransition<JumpState>(() => Free() && velocity.y > VerticalThreshold);
        m.AddAnyTransition<FallState>(() => Free() && velocity.y < -VerticalThreshold);
        m.AddAnyTransition<RunState>(() => Free() && InVerticalBand() && dirX != 0f && grounded);
        m.AddAnyTransition<IdleState>(() => Free() && InVerticalBand() && !(dirX != 0f && grounded) && !SpecialIdleBlocking());
    }

    private bool InVerticalBand() => Mathf.Abs(velocity.y) <= VerticalThreshold;

    // khong dang dash, hoac dash da het gio
    private bool Free() => !Machine.IsIn<DashState>() || DashFinished();

    private bool DashFinished() => dash == null || Machine.TimeInState >= dash.DashingTime;

    // dang chay anim nhan doi va chua het gio -> chua ve Idle (Run/Jump/Fall/Dash van ngat duoc)
    private bool SpecialIdleBlocking() => Machine.IsIn<SpecialIdleState>() && Machine.TimeInState < specialIdleHold;

    private bool DashRequested()
    {
        return canMove && dash != null && !Machine.IsIn<DashState>()
               && Input.GetMouseButtonDown(1) && dash.CanDash;
    }

    // ---------------- Unity loop ----------------

    protected override void Update()
    {
        if (Input.anyKeyDown)
        {
            idleTimer = 0f;
            specialIdleReady = false;
        }

        velocity = rb.linearVelocity;

        if (canMove)
        {
            dirX = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
            jumpHeld = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyUp(KeyCode.Space)) jumpReleased = true;

            checkFlip();
        }

        base.Update();      // danh gia transition + State.Tick
    }

    protected override void FixedUpdate()
    {
        grounded = CheckGround();
        base.FixedUpdate(); // State.FixedTick
    }

    // ---------------- States ----------------

    /// <summary>Cac state di chuyen binh thuong: nhay + chay ngang.</summary>
    private abstract class LocomotionState : State<PlayerMove>
    {
        public override void FixedTick()
        {
            Owner.ApplyJump();
            Owner.ApplyMove();
        }
    }

    private class IdleState : LocomotionState
    {
        public override void Enter()
        {
            Owner.SetAnim(AnimIdle);
            Owner.idleTimer = 0f;
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // dem thoi gian dung yen de chay anim nhan doi
            if (Owner.rb.linearVelocity.sqrMagnitude > 0.0001f) return;

            Owner.idleTimer += Time.fixedDeltaTime;
            if (Owner.idleTimer >= Owner.timeStartIdleSpecial)
            {
                Owner.idleTimer = 0f;
                Owner.specialIdleReady = true;
            }
        }
    }

    private class RunState : LocomotionState
    {
        public override void Enter() => Owner.SetAnim(AnimRun);

        public override void FixedTick()
        {
            base.FixedTick();
            if (PlayerSounds.instance != null) PlayerSounds.instance.PlayRunAudio();
        }
    }

    private class JumpState : LocomotionState
    {
        public override void Enter() => Owner.SetAnim(AnimJump);
    }

    private class FallState : LocomotionState
    {
        public override void Enter() => Owner.SetAnim(AnimFall);
    }

    private class SpecialIdleState : LocomotionState
    {
        public override void Enter()
        {
            Owner.specialIdleReady = false;
            Owner.SetAnim(AnimSpecialIdle);
        }
    }

    private class DashState : State<PlayerMove>
    {
        public override void Enter()
        {
            Owner.jumpPressed = false;
            Owner.isJumping = false;
            Owner.SetAnim(AnimDash);
            Owner.dash.BeginDash(Owner.faceRight ? 1 : -1);
        }

        public override void FixedTick()
        {
            // khong nhay / di chuyen khi dang dash; bo input nhay bam trong luc dash
            Owner.jumpPressed = false;
            Owner.jumpReleased = false;
        }

        public override void Exit() => Owner.dash.EndDash();
    }

    // ---------------- Movement ----------------

    private void ApplyJump()
    {
        if (!canMove)
        {
            jumpPressed = jumpReleased = false;
            return;
        }

        //dieu khien nhay bang nut space
        if (jumpPressed)
        {
            jumpPressed = false;
            if (grounded)
            {
                //chay am thanh nhay
                effectSoundJump();

                isJumping = true;
                jumpTimeCounter = jumpTime;
                player_jump_acti(jumpStrong);
            }
        }

        if (jumpHeld && isJumping)
        {
            if (jumpTimeCounter > 0f)
            {
                player_jump_acti(jumpStrong);
                jumpTimeCounter -= Time.fixedDeltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (jumpReleased)
        {
            jumpReleased = false;
            isJumping = false;
        }
    }

    private void ApplyMove()
    {
        if (!canMove) return;
        rb.linearVelocity = new Vector2(dirX * speedMove, rb.linearVelocity.y);
    }

    public void player_jump_acti(float strong)
    {
        rb.linearVelocity = new Vector2(0f, strong);
    }

    private void checkFlip()
    {
        // check quay dau
        if (dirX > 0f && !faceRight || dirX < 0f && faceRight)
        {
            flip();
        }
    }

    private void flip()
    {
        faceRight = !faceRight;
        transform.Rotate(0f, 180f, 0f);
    }

    // ---------------- Helpers ----------------

    private void SetAnim(int value)
    {
        animator.SetInteger(StateParam, value);
    }

    private bool CheckGround()
    {
        Bounds b = coll.bounds;
        return Physics2D.BoxCast(b.center, b.size, 0f, Vector2.down, GroundCheckExtra, jumpableGround).collider != null;
    }

    /// <summary>Mat dat (cache theo FixedUpdate). Dung cho ben ngoai, vd CreateCheckPoint.</summary>
    public bool IsGround()
    {
        return grounded;
    }

    private void effectSoundJump()
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.PlayJumpAudio();
    }
}
