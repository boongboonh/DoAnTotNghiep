using UnityEngine;

/// <summary>
/// Player trong canh truyen (Story), dang state machine: Idle / Run / Jump / Fall / SpecialIdle.
/// Cung thiet ke voi PlayerMove (khong co Dash):
///   - Update: doc input (chot co), lat mat, danh gia transition.
///   - FixedUpdate: kiem tra mat dat 1 lan / tick (cache), ap dung nhay + di chuyen.
///   - Animator chi SetInteger khi doi state.
/// NPC / TeleBoxStory dieu khien bang co isMove (giu nguyen). Ten field serialized giu nguyen.
/// </summary>
public class PlayerMoveStory : StateMachineOwner<PlayerMoveStory>
{
    [SerializeField] private GameObject PlayerIMG;
    [SerializeField] private LayerMask jumpableGround;

    //chay animation nhan doi
    [SerializeField] private float timeStartIdleSpecial = 10f;
    [SerializeField] private float specialIdleHold = 1.4f;          // >= do dai clip player_idle_special (1.32s)

    //toc do di chuyen
    [SerializeField] private float speedMove = 7f;

    public bool isMove = true;

    [Header("Jump setting")]
    [SerializeField] private float jumpStrong = 7f;
    public float jumpTime;

    private const int AnimIdle = 0, AnimRun = 1, AnimJump = 2, AnimFall = 3, AnimSpecialIdle = 5;
    private const float VerticalThreshold = 0.2f;
    private const float GroundCheckExtra = 0.03f;
    private static readonly int StateParam = Animator.StringToHash("state");

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Animator animator;

    private bool faceRight = true;

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
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        animator = PlayerIMG.GetComponent<Animator>();
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

    protected override void SetupStates(StateMachine<PlayerMoveStory> m)
    {
        m.AddState<IdleState>();
        m.AddState<RunState>();
        m.AddState<JumpState>();
        m.AddState<FallState>();
        m.AddState<SpecialIdleState>();
        m.SetInitialState<IdleState>();

        // Thu tu = do uu tien (AnyTransition duoc danh gia theo thu tu dang ky)
        m.AddAnyTransition<SpecialIdleState>(() => specialIdleReady && Machine.IsIn<IdleState>());
        m.AddAnyTransition<JumpState>(() => velocity.y > VerticalThreshold);
        m.AddAnyTransition<FallState>(() => velocity.y < -VerticalThreshold);
        m.AddAnyTransition<RunState>(() => InVerticalBand() && dirX != 0f && grounded);
        m.AddAnyTransition<IdleState>(() => InVerticalBand() && !(dirX != 0f && grounded) && !SpecialIdleBlocking());
    }

    private bool InVerticalBand() => Mathf.Abs(velocity.y) <= VerticalThreshold;

    // dang chay anim nhan doi va chua het gio -> chua ve Idle (Run/Jump/Fall van ngat duoc)
    private bool SpecialIdleBlocking() => Machine.IsIn<SpecialIdleState>() && Machine.TimeInState < specialIdleHold;

    // ---------------- Unity loop ----------------

    protected override void Update()
    {
        if (Input.anyKeyDown)
        {
            idleTimer = 0f;
            specialIdleReady = false;
        }

        velocity = rb.linearVelocity;

        if (isMove)         //bien khong cho di chuyen
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

    private abstract class LocomotionState : State<PlayerMoveStory>
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

    // ---------------- Movement ----------------

    private void ApplyJump()
    {
        if (!isMove)
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
        if (!isMove) return;
        rb.linearVelocity = new Vector2(dirX * speedMove, rb.linearVelocity.y);
    }

    public void player_jump_acti(float strong)
    {
        rb.linearVelocity = new Vector2(0f, strong);
    }

    private void checkFlip()
    {
        if (dirX > 0f && !faceRight || dirX < 0f && faceRight)
        {
            faceRight = !faceRight;
            transform.Rotate(0f, 180f, 0f);
        }
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

    /// <summary>Mat dat (cache theo FixedUpdate).</summary>
    public bool IsGround()
    {
        return grounded;
    }

    private void effectSoundJump()
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.PlayJumpAudio();
    }
}
