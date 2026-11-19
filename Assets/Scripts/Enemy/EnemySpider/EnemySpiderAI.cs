using UnityEngine;

/// <summary>
/// AI enemy spider dang state machine:
///   Idle   -> khong co player: dau tro ve goc 0, anim 0.
///   Alert  -> player trong tam: dau xoay theo player, dem thoi gian hoi chieu, anim 1.
///   Attack -> het hoi chieu: anim 2 (animation event Attack ban dan, resetCondown dat lai hoi chieu).
/// Ten field va ham animation event giu nguyen de khong hong prefab/anim.
/// </summary>
public class EnemySpiderAI : StateMachineOwner<EnemySpiderAI>
{
    [SerializeField] GameObject bullet;
    [SerializeField] float timeAttackRate = 1.5f;
    [SerializeField] float speedRotate = 3f;

    [SerializeField] private Transform pointShooting;
    [SerializeField] private Transform pointHead;

    [HideInInspector] public bool inRange = false;      // EnemySpiderIstriggerArea set

    [Header("audio sound")]
    [SerializeField] private AudioSource idleSound;
    [SerializeField] private AudioSource AttackSound;

    private static readonly int StateParam = Animator.StringToHash("state");

    private float timer = 0f;
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    protected override void SetupStates(StateMachine<EnemySpiderAI> m)
    {
        m.AddState<IdleState>();
        m.AddState<AlertState>();
        m.AddState<AttackState>();
        m.SetInitialState<IdleState>();

        m.AddTransition<IdleState, AlertState>(() => inRange);
        m.AddTransition<AlertState, IdleState>(() => !inRange);
        m.AddTransition<AlertState, AttackState>(() => timer >= timeAttackRate);
        m.AddTransition<AttackState, IdleState>(() => !inRange);
        m.AddTransition<AttackState, AlertState>(() => timer < timeAttackRate);   // sau resetCondown
    }

    // ---------------- States ----------------

    private class IdleState : State<EnemySpiderAI>
    {
        public override void Enter() => Owner.SetAnim(0);
        public override void Tick() => Owner.RotateHead(false);
    }

    private class AlertState : State<EnemySpiderAI>
    {
        public override void Enter() => Owner.SetAnim(1);

        public override void Tick()
        {
            Owner.timer += Time.deltaTime;
            Owner.RotateHead(true);
            Owner.PlayIdleSound();
        }
    }

    private class AttackState : State<EnemySpiderAI>
    {
        public override void Enter() => Owner.SetAnim(2);

        public override void Tick()
        {
            Owner.timer += Time.deltaTime;
            Owner.RotateHead(true);
            Owner.PlayIdleSound();
        }
    }

    // ---------------- Helpers ----------------

    private void SetAnim(int value)
    {
        if (animator != null) animator.SetInteger(StateParam, value);
    }

    private void PlayIdleSound()
    {
        AudioManager.PlaySource(idleSound, true);
    }

    private void RotateHead(bool trackPlayer)
    {
        if (pointHead == null) return;

        Quaternion q = Quaternion.identity;
        if (trackPlayer)
        {
            if (GetPosPlayer.Instance == null || GetPosPlayer.Instance.PlayerPos == null) return;
            Vector2 vectorToTarget = GetPosPlayer.Instance.PlayerPos.position - pointHead.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + 90;
            q = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        pointHead.rotation = Quaternion.Slerp(pointHead.rotation, q, Time.deltaTime * speedRotate);
    }

    // ---------------- Ham goi tu animation event ----------------

    public void resetCondown()
    {
        timer = 0f;
    }

    public void Attack()
    {
        if (AttackSound != null) AudioManager.PlaySource(AttackSound);
        Instantiate(bullet, pointShooting.position, pointShooting.rotation);
    }
}
