using UnityEngine;

/// <summary>
/// AI enemy rush dang state machine:
///   Patrol -> nhay qua lai giua LimitLeft / LimitRight.
///   Leap   -> player trong tam (dang cham dat): nhay vot ve phia player. Qua dinh nhay (roi xuong) -> Hover.
///   Hover  -> treo lo lung (gravity = 0) 1 giay de gong don (anim DelayAttack).
///   Rush   -> lao thang toi vi tri player luc bat dau lao; toi noi / qua xa thi ve Patrol (roi Leap lai neu con trong tam).
/// Ten field public giu nguyen de khong hong prefab/anim.
/// </summary>
public class EnemyJumpMove : StateMachineOwner<EnemyJumpMove>
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
    public Transform LimitRight;
    public Transform LimitLeft;

    [HideInInspector] public bool inRange = false;      // EnemyJumptriggerAreaCheck set
    public float DelayAttack = 1f;                      // (chua dung, giu lai de khong mat du lieu prefab)

    private float gravity;

    private Vector2 playerPosClone;
    Animator _animator;
    private Rigidbody2D _rb;
    public LayerMask layerGround;
    Collider2D colli;

    [Header("sound enemy rush")]
    [SerializeField] private AudioSource jumpMoveSound;
    [SerializeField] private AudioSource attackSound;

    private const float HoverTime = 1f;
    private const float LeapFallSpeed = -0.2f;
    private const float RushReachDistance = 0.1f;
    private const float RushMaxDistance = 13f;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        colli = GetComponent<Collider2D>();
        gravity = _rb.gravityScale;
    }

    protected override void OnEnable()
    {
        player = null;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_rb != null) _rb.gravityScale = gravity;
    }

    protected override void SetupStates(StateMachine<EnemyJumpMove> m)
    {
        m.AddState<PatrolState>();
        m.AddState<LeapState>();
        m.AddState<HoverState>();
        m.AddState<RushState>();
        m.SetInitialState<PatrolState>();

        m.AddTransition<PatrolState, LeapState>(() => inRange && IsGround());
        m.AddTransition<LeapState, HoverState>(() => _rb.linearVelocity.y < LeapFallSpeed);
        m.AddTransition<HoverState, RushState>(() => Machine.TimeInState >= HoverTime);
        m.AddTransition<RushState, PatrolState>(RushFinished);
    }

    protected override void Update()
    {
        base.Update();
        updateAnimation();
    }

    // ---------------- States ----------------

    private class PatrolState : State<EnemyJumpMove>
    {
        public override void Tick()
        {
            if (!Owner.IsGround()) return;

            if (Owner.transform.position.x - Owner.LimitLeft.position.x <= 0)
            {
                Owner.direction = Mathf.Abs(Owner.direction);
            }
            if (Owner.transform.position.x - Owner.LimitRight.position.x >= 0)
            {
                Owner.direction = -1f;
            }
            Owner.jumpNomal();
        }
    }

    private class LeapState : State<EnemyJumpMove>
    {
        public override void Tick()
        {
            if (!Owner.IsGround()) return;

            Owner.RefreshPlayer();
            Owner.JumpAttack();
        }
    }

    private class HoverState : State<EnemyJumpMove>
    {
        public override void Enter()
        {
            Owner._rb.linearVelocity = Vector2.zero;
            Owner._rb.gravityScale = 0f;
        }

        public override void Exit()
        {
            Owner._rb.gravityScale = Owner.gravity;
        }
    }

    private class RushState : State<EnemyJumpMove>
    {
        public override void Enter()
        {
            Owner.RefreshPlayer();
            Owner.playerPosClone = Owner.player != null ? (Vector2)Owner.player.position : (Vector2)Owner.transform.position;

            if (Owner.attackSound != null) AudioManager.PlaySource(Owner.attackSound);
        }

        public override void Tick()
        {
            Owner.transform.position = Vector2.MoveTowards(
                Owner.transform.position, Owner.playerPosClone, Owner.speedRushAttack * Time.deltaTime);
        }
    }

    // ---------------- Helpers ----------------

    private bool RushFinished()
    {
        float dist = Vector2.Distance(transform.position, playerPosClone);
        return dist < RushReachDistance || dist >= RushMaxDistance;
    }

    private void RefreshPlayer()
    {
        if (GetPosPlayer.Instance != null) player = GetPosPlayer.Instance.PlayerPos;
    }

    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(colli.bounds.center, colli.bounds.size, 0f, Vector2.down, ExtraHight, layerGround);
        return raycastHit2D.collider != null;
    }

    void jumpNomal()
    {
        _rb.linearVelocity = new Vector2(ForceJumpMovex * direction, ForceJumpMovey);

        //chay am thanh nhay
        playJumpMove();
    }

    private void playJumpMove()
    {
        AudioManager.PlaySource(jumpMoveSound, true);
    }

    void JumpAttack()
    {
        _rb.linearVelocity = new Vector2(ForceJumpx * checkdirection(direction), ForceJumpy);
    }

    float checkdirection(float direc)
    {
        if (player == null || transform.position.x - player.position.x <= 0)
        {
            return Mathf.Abs(direc); // player o ben phai
        }
        return -1; // player o ben trai
    }

    void updateAnimation()
    {
        bool readyAttack = Machine.IsIn<HoverState>();
        bool attackNow = Machine.IsIn<RushState>();

        if (_rb.linearVelocity.y > .1f)
        {
            _animator.SetBool("EnemyRushIdle", false);
            _animator.SetBool("EnemyRushJump", true);
        }
        else if (_rb.linearVelocity.y < -.1f)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, RushMaxDistance);
    }
}
