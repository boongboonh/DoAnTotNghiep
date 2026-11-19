using UnityEngine;

/// <summary>
/// Enemy ban lien tuc dang state machine:
///   Shoot    -> chay anim ban (state 1); ket thuc bang animation event stopAttack.
///   Cooldown -> nghi delayBetweenShoot giay (anim 0, phat am idle) roi ban tiep.
/// Ten field va ham animation event (AttackNow, stopAttack) giu nguyen de khong hong prefab/anim.
/// </summary>
public class EnemyShootAlway : StateMachineOwner<EnemyShootAlway>
{
    [SerializeField] private GameObject BulletNomal;
    [SerializeField] private Transform pointShoot;
    [SerializeField] private float delayBetweenShoot = 1f;
    private Animator animator;

    [Header("audio sound")]
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private AudioSource idleSound;

    private static readonly int StateParam = Animator.StringToHash("state");

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    protected override void SetupStates(StateMachine<EnemyShootAlway> m)
    {
        m.AddState<ShootState>();
        m.AddState<CooldownState>();
        m.SetInitialState<ShootState>();

        // Shoot -> Cooldown do animation event stopAttack
        m.AddTransition<CooldownState, ShootState>(() => Machine.TimeInState >= delayBetweenShoot);
    }

    // ---------------- States ----------------

    private class ShootState : State<EnemyShootAlway>
    {
        public override void Enter() => Owner.SetAnim(1);
    }

    private class CooldownState : State<EnemyShootAlway>
    {
        public override void Enter() => Owner.SetAnim(0);

        public override void Tick()
        {
            //chay am thanh idle khi khong tan cong
            AudioManager.PlaySource(Owner.idleSound, true);
        }
    }

    private void SetAnim(int value)
    {
        if (animator != null) animator.SetInteger(StateParam, value);
    }

    // ---------------- Ham goi tu animation event ----------------

    public void stopAttack()
    {
        if (Machine.IsIn<ShootState>()) Machine.ChangeState<CooldownState>();
    }

    public void AttackNow()
    {
        //chay am thanh ban
        if (shootSound != null) AudioManager.PlaySource(shootSound);

        Quaternion rotation = transform.parent != null ? transform.parent.rotation : transform.rotation;
        Instantiate(BulletNomal, pointShoot.position, rotation);
    }
}
