using UnityEngine;

/// <summary>
/// AI enemy dash dang state machine:
///   Patrol  -> di tuan giua LimitLeft / LimitRight.
///   Windup  -> (player vao tam) dung lai, quay ve phia player, chay anim delay attack.
///              Ket thuc bang animation event endDelayAttack.
///   Dash    -> lao ve phia gioi han doi dien; toi noi thi quay lai Windup (neu player con trong tam) hoac Patrol.
/// Ten field public va 2 ham animation event (startDelayAttack / endDelayAttack) giu nguyen de khong hong prefab/anim.
/// </summary>
public class EnemyDashAI : StateMachineOwner<EnemyDashAI>
{
    [Header("setting dash attack")]
    public float speedDashAttack = 10f;
    public float speedDashMove = 2f;

    public Transform LimitRight;
    public Transform LimitLeft;

    [HideInInspector] public bool inRange = false;     // TriggerAreaDashCheck set
    public float cooldowAttack = 1.5f;                 // (chua dung, giu lai de khong mat du lieu prefab)

    [Header("attack sound")]
    [SerializeField] private AudioSource enemyDashAttack;

    private const float ReachDistance = 0.1f;
    private static readonly int StateParam = Animator.StringToHash("state");

    private Transform target;
    private Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        target = LimitLeft;
        base.OnEnable();
    }

    protected override void SetupStates(StateMachine<EnemyDashAI> m)
    {
        m.AddState<PatrolState>();
        m.AddState<WindupState>();
        m.AddState<DashState>();
        m.SetInitialState<PatrolState>();

        m.AddTransition<PatrolState, WindupState>(() => inRange);
        m.AddTransition<DashState, WindupState>(() => inRange && ReachedTarget());
        m.AddTransition<DashState, PatrolState>(() => !inRange && ReachedTarget());
        // Windup -> Dash do animation event endDelayAttack
    }

    // ---------------- States ----------------

    private class PatrolState : State<EnemyDashAI>
    {
        public override void Enter() => Owner.SetAnim(0);

        public override void Tick()
        {
            Owner.UpdatePatrolTarget();
            Owner.MoveToTarget(Owner.speedDashMove);
        }
    }

    private class WindupState : State<EnemyDashAI>
    {
        public override void Enter() => Owner.SetAnim(1);

        public override void Tick()
        {
            // Player co the doi phia trong luc gong -> luon quay ve phia player (giong logic cu)
            Owner.UpdateAttackTarget();
        }
    }

    private class DashState : State<EnemyDashAI>
    {
        public override void Enter() => Owner.SetAnim(0);

        public override void Tick()
        {
            Owner.MoveToTarget(Owner.speedDashAttack);
        }
    }

    // ---------------- Helpers ----------------

    private void SetAnim(int value)
    {
        if (_animator != null) _animator.SetInteger(StateParam, value);
    }

    private bool ReachedTarget()
    {
        return Mathf.Abs(transform.position.x - target.position.x) <= ReachDistance;
    }

    private void MoveToTarget(float speed)
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            new Vector2(target.position.x, transform.position.y),
            speed * Time.deltaTime);
    }

    private void UpdatePatrolTarget()
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

    private void UpdateAttackTarget()
    {
        if (GetPosPlayer.Instance == null) return;
        var player = GetPosPlayer.Instance.PlayerPos;
        if (player == null) return;

        target = transform.position.x - player.position.x <= 0 ? LimitRight : LimitLeft;
        Flip();
    }

    public void Flip()
    {
        Vector2 rotation = transform.eulerAngles;
        rotation.y = transform.position.x > target.position.x ? 0f : 180f;
        transform.eulerAngles = rotation;
    }

    // ---------------- Ham goi tu animation event (EnemyDashDelayAttack) ----------------

    public void startDelayAttack()
    {
        if (enemyDashAttack != null) AudioManager.PlaySource(enemyDashAttack);
    }

    public void endDelayAttack()
    {
        if (Machine.IsIn<WindupState>()) Machine.ChangeState<DashState>();
    }
}
