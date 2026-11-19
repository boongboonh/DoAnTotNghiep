using UnityEngine;

/// <summary>
/// AI enemy boom dang state machine:
///   Patrol   -> di tuan giua LimitLeft / LimitRight.
///   Chase    -> (player vao tam) duoi theo player.
///   Attack   -> du gan (attackDistance): dung lai, chay anim tan cong. Thoat bang animation event stopAttack.
///   Cooldown -> nghi 1 giay sau khi tan cong roi quay lai Patrol/Chase.
/// Ten field public va cac ham animation event giu nguyen de khong hong prefab/anim.
/// </summary>
public class EnemyBoomMoveAI : StateMachineOwner<EnemyBoomMoveAI>
{
    public float attackDistance = 2f;// gioi han tan cong
    public float moveSpeed;
    public Transform LimitRight;
    public Transform LimitLeft;
    [HideInInspector] public Transform target;
    [HideInInspector] public bool inRange = false;      // TriggerAreaMoveCheck set
    [HideInInspector] public Transform player;

    [Header("enemy boom attack sound")]
    [SerializeField] private AudioSource boomEnemyAttackSound;

    private const float CooldownTime = 1f;
    private const float LimitReachDistance = 0.1f;
    private static readonly int StateParam = Animator.StringToHash("state");

    private Animator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        target = LimitRight;
    }

    protected override void OnEnable()
    {
        player = null;
        base.OnEnable();
    }

    protected override void SetupStates(StateMachine<EnemyBoomMoveAI> m)
    {
        m.AddState<PatrolState>();
        m.AddState<ChaseState>();
        m.AddState<AttackState>();
        m.AddState<CooldownState>();
        m.SetInitialState<PatrolState>();

        m.AddTransition<PatrolState, ChaseState>(() => inRange);
        m.AddTransition<ChaseState, PatrolState>(() => !inRange);
        m.AddTransition<ChaseState, AttackState>(InAttackDistance);
        // Attack -> Cooldown do animation event stopAttack
        m.AddTransition<CooldownState, ChaseState>(() => inRange && CooldownDone());
        m.AddTransition<CooldownState, PatrolState>(() => !inRange && CooldownDone());
    }

    // ---------------- States ----------------

    private class PatrolState : State<EnemyBoomMoveAI>
    {
        public override void Tick()
        {
            Owner.MovePatrol();
            Owner.SelectTarget();
        }
    }

    private class ChaseState : State<EnemyBoomMoveAI>
    {
        public override void Tick()
        {
            Owner.RefreshPlayer();
            Owner.MoveToPlayer();
        }
    }

    private class AttackState : State<EnemyBoomMoveAI>
    {
        public override void Enter() => Owner.SetAnim(1);
    }

    private class CooldownState : State<EnemyBoomMoveAI> { }

    // ---------------- Helpers ----------------

    private void SetAnim(int value)
    {
        if (anim != null) anim.SetInteger(StateParam, value);
    }

    private void RefreshPlayer()
    {
        if (GetPosPlayer.Instance != null) player = GetPosPlayer.Instance.PlayerPos;
    }

    private bool InAttackDistance()
    {
        RefreshPlayer();
        return player != null && Vector2.Distance(transform.position, player.position) <= attackDistance;
    }

    private bool CooldownDone()
    {
        return Machine.TimeInState >= CooldownTime;
    }

    // di chuyen tuan tra
    private void MovePatrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.position.x, transform.position.y), moveSpeed * Time.deltaTime);
    }

    private void MoveToPlayer()
    {
        if (player == null) return;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.position.x, transform.position.y), moveSpeed * Time.deltaTime);
    }

    //doi huong khi di chuyen den gioi han khu vuc
    public void SelectTarget()
    {
        if (Mathf.Abs(transform.position.x - LimitLeft.position.x) <= LimitReachDistance)
        {
            target = LimitRight;
        }

        if (Mathf.Abs(transform.position.x - LimitRight.position.x) <= LimitReachDistance)
        {
            target = LimitLeft;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    // ---------------- Ham goi tu animation event ----------------

    // (EnemyDelayAttack) Attack state da dung yen san, giu ham de anim event khong bao loi
    public void stopMoveDelayAttack() { }

    // (EnemyMoveAttack) ket thuc don danh -> nghi 1 giay
    public void stopAttack()
    {
        SetAnim(0);
        if (Machine.IsIn<AttackState>()) Machine.ChangeState<CooldownState>();
    }

    public void playSoundAttack()
    {
        if (boomEnemyAttackSound != null) AudioManager.PlaySource(boomEnemyAttackSound);
    }
}
