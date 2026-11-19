using UnityEngine;

/// <summary>
/// AI enemy cua nhay dang state machine:
///   Patrol -> nhay qua lai giua LimitLeft / LimitRight.
///   Chase  -> player trong tam: nhay ve phia player.
/// Viec nhay dien ra o FixedTick (chi nhay khi cham dat). Ten field public giu nguyen de khong hong prefab.
/// </summary>
public class EnemyCuaNhayAI : StateMachineOwner<EnemyCuaNhayAI>
{
    [Header("setting jump move")]
    public float ForceJumpMovex = 2f;
    public float ForceJumpMovey = 5f;
    private float direction = 1f;

    public Transform LimitRight;
    public Transform LimitLeft;

    private Transform targetObj;

    [HideInInspector] public bool inRange = false;      // EnemyCuaNhayTriggerAreaCheck set

    private static readonly int StateParam = Animator.StringToHash("state");

    Animator _animator;
    private Rigidbody2D _rb;
    public LayerMask layerGround;
    Collider2D colli;

    [Header("audio sound")]
    [SerializeField] private AudioSource jumpEnemyMove;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        colli = GetComponent<Collider2D>();
    }

    protected override void OnEnable()
    {
        targetObj = LimitRight;
        base.OnEnable();
    }

    protected override void SetupStates(StateMachine<EnemyCuaNhayAI> m)
    {
        m.AddState<PatrolState>();
        m.AddState<ChaseState>();
        m.SetInitialState<PatrolState>();

        m.AddTransition<PatrolState, ChaseState>(() => inRange);
        m.AddTransition<ChaseState, PatrolState>(() => !inRange);
    }

    protected override void FixedUpdate()
    {
        updateAnimation();
        base.FixedUpdate();
    }

    // ---------------- States ----------------

    private class PatrolState : State<EnemyCuaNhayAI>
    {
        public override void Enter()
        {
            // vua roi Chase thi targetObj dang la player -> chon lai gioi han gan nhat
            Owner.ResetPatrolTarget();
        }

        public override void FixedTick()
        {
            if (!Owner.IsGround()) return;
            Owner.checkDirectionMove();
            Owner.jump(Owner.checkDirectionTarget(Owner.direction));
        }
    }

    private class ChaseState : State<EnemyCuaNhayAI>
    {
        public override void FixedTick()
        {
            if (!Owner.IsGround()) return;
            if (GetPosPlayer.Instance == null) return;

            Owner.targetObj = GetPosPlayer.Instance.PlayerPos;
            if (Owner.targetObj == null) return;

            Owner.jump(Owner.checkDirectionTarget(Owner.direction));
        }
    }

    // ---------------- Helpers ----------------

    private void ResetPatrolTarget()
    {
        if (targetObj == LimitLeft || targetObj == LimitRight) return;

        float toLeft = Mathf.Abs(transform.position.x - LimitLeft.position.x);
        float toRight = Mathf.Abs(transform.position.x - LimitRight.position.x);
        targetObj = toLeft < toRight ? LimitLeft : LimitRight;
    }

    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(colli.bounds.center, colli.bounds.size, 0f, Vector2.down, ExtraHight, layerGround);
        return raycastHit2D.collider != null;
    }

    void jump(float directionJump) // truyen vao huowng nhay - trai,+ phai;
    {
        _rb.linearVelocity = new Vector2(ForceJumpMovex * directionJump, ForceJumpMovey);

        //chay am thanh nhay
        playSoundJump();
    }

    private void playSoundJump()
    {
        AudioManager.PlaySource(jumpEnemyMove, true);
    }

    private void updateAnimation()
    {
        if (_rb.linearVelocity.y > .1f)
        {
            _animator.SetInteger(StateParam, 0);
        }

        if (_rb.linearVelocity.y < -.1f)
        {
            _animator.SetInteger(StateParam, 1);
        }
    }

    // doi doi tuong target
    private void checkDirectionMove()
    {
        if (transform.position.x - LimitLeft.position.x <= 0.1f)
        {
            targetObj = LimitRight;
        }
        if (transform.position.x - LimitRight.position.x >= 0.1f)
        {
            targetObj = LimitLeft;
        }
    }

    // kiem tra huong doi tuong nhay den
    private float checkDirectionTarget(float direction)
    {
        if (transform.position.x - targetObj.position.x > 0)
        {
            return -Mathf.Abs(direction); // target o ben trai
        }
        if (transform.position.x - targetObj.position.x < 0)
        {
            return Mathf.Abs(direction); // target o ben phai
        }
        return direction;
    }
}
