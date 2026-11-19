using UnityEngine;

/// <summary>
/// AI cua nhay mini dang state machine. Chi co mot hanh vi (luon nhay ve phia player) nen chi co state Chase;
/// them state moi (vd Idle, Stunned) bang AddState / AddTransition trong SetupStates.
/// </summary>
public class EnemyCuaNhayMiniAI : StateMachineOwner<EnemyCuaNhayMiniAI>
{
    [Header("setting jump move")]
    public float ForceJumpMovex = 2f;
    public float ForceJumpMovey = 5f;
    private float direction = 1f;

    private Transform targetObj;

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

    protected override void SetupStates(StateMachine<EnemyCuaNhayMiniAI> m)
    {
        m.AddState<ChaseState>();
        m.SetInitialState<ChaseState>();
    }

    protected override void FixedUpdate()
    {
        updateAnimation();
        base.FixedUpdate();
    }

    // ---------------- States ----------------

    private class ChaseState : State<EnemyCuaNhayMiniAI>
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

    private bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(colli.bounds.center, colli.bounds.size, 0f, Vector2.down, ExtraHight, layerGround);
        return raycastHit2D.collider != null;
    }

    private void jump(float directionJump) // truyen vao huowng nhay - trai,+ phai;
    {
        _rb.linearVelocity = new Vector2(ForceJumpMovex * directionJump, Random.Range(0.1f, ForceJumpMovey));

        //chay hieu ung am thanh
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

    // kiem tra huong doi tuong nhay den
    private float checkDirectionTarget(float direction)
    {
        if (transform.position.x - targetObj.position.x > 0)
        {
            return -Mathf.Abs(direction); // player o ben trai
        }
        if (transform.position.x - targetObj.position.x < 0)
        {
            return Mathf.Abs(direction); // player o ben phai
        }
        return direction;
    }
}
