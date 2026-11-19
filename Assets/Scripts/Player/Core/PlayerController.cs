using System;
using UnityEngine;

/// <summary>Trang thai di chuyen ma ben ngoai (pose, am thanh, UI) co the doc.</summary>
public enum PlayerLocomotion
{
    Idle,
    Run,
    Rise,
    Fall,
    Dash,
    Attack,
    Cast,
    Hurt,
    Locked,
    Dead,
    WallSlide,
    Bash,
    WallClimb
}

/// <summary>
/// Bo nao cua player: state machine noi PlayerInputReader (y dinh) voi PlayerMotor (vat ly) va PlayerCombat.
///   Idle / Run / Rise / Fall / Dash / Attack / Hurt / Locked.
///   - Ben ngoai chi can doc <see cref="Locomotion"/> va cac event, khong can biet state ben trong.
///   - Khoa dieu khien (cutscene, hoi thoai, het man) bang <see cref="ControlLocked"/>.
///   - Bi thuong: PlayerHurt goi <see cref="EnterHurt"/>. Danh: AttackState chay PlayerCombat (neu co).
///   - Muon them hanh dong moi (leo thang, tuong...): them state + transition trong SetupStates,
///     them module rieng va chi goi qua API cua Motor / Controls.
/// </summary>
[DefaultExecutionOrder(-80)]
public class PlayerController : StateMachineOwner<PlayerController>
{
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerInputReader controls;
    [Tooltip("Tuy chon. Khong co = khong danh duoc.")]
    [SerializeField] private PlayerCombat combat;
    [Tooltip("Tuy chon. Khong co = khong dung skill duoc.")]
    [SerializeField] private PlayerSkills skills;
    [Tooltip("Tuy chon (prototype). Khong co = khong Bash duoc.")]
    [SerializeField] private PrototypeBash bash;

    private const float MoveDeadZone = 0.01f;

    private bool controlLocked;
    private bool dead;
    public bool IsDead => dead;
    private float nextDashTime;
    private int dashDirection = 1;
    private Vector2 hurtKnockback;

    public PlayerMotor Motor => motor;
    public PlayerInputReader Controls => controls;
    public PlayerCombat Combat => combat;
    public PlayerSkills Skills => skills;
    public PrototypeBash Bash => bash;
    /// <summary>Gan Bash tao luc chay (prototype khong sua scene). Goi truoc khi choi.</summary>
    public void SetBash(PrototypeBash value) => bash = value;
    private bool BashActive => bash != null && bash.Busy;
    public PlayerMovementConfig Config => motor.Config;
    public bool DashReady => CanAct() && !Machine.IsIn<DashState>()
        && Time.time >= nextDashTime && (motor.Grounded || motor.AirDashAvailable);

    public PlayerLocomotion Locomotion { get; private set; } = PlayerLocomotion.Idle;
    /// <summary>Thoi gian (giay) dung yen lien tuc o Idle.</summary>
    public float IdleTime { get; private set; }
    public bool IsBored => Locomotion == PlayerLocomotion.Idle && IdleTime >= Config.boredTime;

    /// <summary>Khoa / mo dieu khien. Khoa: dung ngang, bo input, ve state Locked.</summary>
    public bool ControlLocked
    {
        get => controlLocked;
        set
        {
            controlLocked = value;
            if (controls != null) controls.Locked = value;
        }
    }

    /// <summary>(cu, moi).</summary>
    public event Action<PlayerLocomotion, PlayerLocomotion> LocomotionChanged;
    public event Action DashStarted;

    protected override void Awake()
    {
        if (motor == null) motor = GetComponent<PlayerMotor>();
        if (controls == null) controls = GetComponent<PlayerInputReader>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (skills == null) skills = GetComponent<PlayerSkills>();
        if (bash == null) bash = GetComponent<PrototypeBash>();
        base.Awake();
    }

    protected override void OnEnable()
    {
        nextDashTime = 0f;
        IdleTime = 0f;
        if (controls != null)
        {
            controls.Locked = controlLocked;
            controls.SetJumpBuffer(Config.jumpBufferTime);
            controls.SetDashBuffer(Config.dashBufferTime);
        }
        base.OnEnable();
    }

    protected override void Update()
    {
        UpdateFacing();
        base.Update();
    }

    /// <summary>Vao trang thai bi thuong: huy don dang danh / dash, bi day lui voi van toc cho truoc.</summary>
    public void EnterHurt(Vector2 knockbackVelocity)
    {
        if (controlLocked || dead) return;
        hurtKnockback = knockbackVelocity;
        motor.ClearWallMotion();
        Machine.ChangeState<HurtState>(true);
    }

    // ---------------- State machine ----------------

    public void EnterDead()
    {
        if (dead) return;
        dead = true;
        motor.RestoreDropCollision();
        motor.ClearWallMotion();
        controls.Locked = true;
        Machine.ChangeState<DeadState>(true);
    }

    protected override void SetupStates(StateMachine<PlayerController> m)
    {
        m.AddState<IdleState>();
        m.AddState<RunState>();
        m.AddState<RiseState>();
        m.AddState<FallState>();
        m.AddState<DashState>();
        m.AddState<AttackState>();
        m.AddState<CastState>();
        m.AddState<HurtState>();
        m.AddState<LockedState>();
        m.AddState<DeadState>();
        m.AddState<WallSlideState>();
        m.AddState<WallClimbState>();
        m.AddState<BashState>();
        m.SetInitialState<IdleState>();

        // Thu tu = do uu tien. Hurt khong co transition: chi vao qua EnterHurt.
        m.AddAnyTransition<DeadState>(() => dead);
        m.AddAnyTransition<LockedState>(() => !dead && (controlLocked || controls.Locked));
        m.AddAnyTransition<BashState>(BashRequested);
        m.AddAnyTransition<DashState>(DashRequested);
        m.AddAnyTransition<CastState>(SkillRequested);
        m.AddAnyTransition<AttackState>(AttackRequested);
        m.AddAnyTransition<WallClimbState>(() => Free() && motor.CanWallClimb(controls.GrabHeld));
        m.AddAnyTransition<WallSlideState>(() => Free() && motor.CanWallSlide(controls.Move));
        m.AddAnyTransition<RiseState>(() => Free() && !motor.Grounded && motor.Velocity.y > 0f
            && !motor.CanWallClimb(controls.GrabHeld));
        m.AddAnyTransition<FallState>(() => Free() && !motor.Grounded && motor.Velocity.y <= 0f
            && !motor.CanWallSlide(controls.Move) && !motor.CanWallClimb(controls.GrabHeld));
        m.AddAnyTransition<RunState>(() => Free() && motor.Grounded && Mathf.Abs(controls.Move) > MoveDeadZone);
        m.AddAnyTransition<IdleState>(() => Free() && motor.Grounded && Mathf.Abs(controls.Move) <= MoveDeadZone);
    }

    private bool DashActive => Machine.IsIn<DashState>() && Machine.TimeInState < Config.dashTime && !motor.DashBlocked;
    private bool HurtActive => Machine.IsIn<HurtState>() && Machine.TimeInState < Config.hurtStunTime;
    private bool AttackActive => Machine.IsIn<AttackState>() && combat != null && combat.Active;

    /// <summary>Duoc phep chon hanh dong moi: khong khoa, het dash, het choang (don dang danh van bi ngat duoc).</summary>
    private bool CanAct() => !dead && !controlLocked && !controls.Locked && !DashActive && !HurtActive && !BashActive;

    private bool BashRequested()
    {
        return CanAct() && !CastActive && bash != null && bash.CanBegin;
    }

    private bool CastActive => Machine.IsIn<CastState>() && skills != null && skills.Active;

    /// <summary>CanAct va khong dang giua don danh / niem skill: luc nay moi tra quyen cho state chay/nhay.</summary>
    private bool Free() => CanAct() && !AttackActive && !CastActive;

    private bool DashRequested()
    {
        return DashReady && controls.DashBuffered;
    }

    private bool AttackRequested()
    {
        return CanAct() && !CastActive && combat != null && combat.CanStart;
    }

    private bool SkillRequested()
    {
        return CanAct() && skills != null && skills.CanStart;
    }

    private void UpdateFacing()
    {
        if (controlLocked || dead || motor.WallJumpProtected || BashActive) return;
        if (Machine.IsIn<DashState>() || Machine.IsIn<AttackState>() || Machine.IsIn<CastState>()
            || Machine.IsIn<HurtState>()) return;

        float x = controls.Move;
        if (x > MoveDeadZone && !motor.FacingRight) motor.Face(1);
        else if (x < -MoveDeadZone && motor.FacingRight) motor.Face(-1);
    }

    private void SetLocomotion(PlayerLocomotion next)
    {
        if (Locomotion == next) return;
        PlayerLocomotion old = Locomotion;
        Locomotion = next;
        if (next != PlayerLocomotion.Idle) IdleTime = 0f;
        LocomotionChanged?.Invoke(old, next);
    }

    private bool TryJump()
    {
        if (controls.JumpBuffered && controls.DownHeld && motor.CanDropThrough)
        {
            if (!controls.ConsumeJump(out _)) return false;
            controls.ConsumeJumpRelease();
            motor.DropThrough();
            return true;
        }
        if (controls.JumpBuffered && (motor.CanJump || motor.CanWallJump || motor.AirJumpAvailable))
        {
            if (!controls.ConsumeJump(out bool released)) return false;
            if (motor.CanJump) motor.Jump();
            else if (!motor.TryWallJump() && !motor.TryAirJump()) return false;
            // Lenh bam/nha nhanh van phai tao mot cu nhay thap khi buffer duoc thuc thi.
            if (released || !controls.JumpHeld) motor.CutJump();
            return true;
        }
        return false;
    }

    // ---------------- States ----------------

    /// <summary>Cac state di chuyen thuong: chay ngang + nhay.</summary>
    private abstract class LocomotionState : State<PlayerController>
    {
        protected abstract PlayerLocomotion Kind { get; }

        public override void Enter() => Owner.SetLocomotion(Kind);

        public override void FixedTick()
        {
            bool jumped = Owner.TryJump();
            bool released = Owner.controls.ConsumeJumpRelease();
            if (released && !jumped) Owner.motor.CutJump();
            Owner.motor.MoveHorizontal(Owner.controls.Move);
        }
    }

    public void ResetMotion(Vector2 position)
    {
        dead = false;
        if (combat != null) combat.ResetCombo();
        if (skills != null) skills.Cancel();
        if (bash != null) { bash.Cancel(); bash.AbortFloat(); }
        controls.ResetInput();
        controlLocked = false;
        controls.Locked = false;
        Machine.Start();
        motor.ResetMotion(position);
        nextDashTime = 0f;
        motor.Face(1);
    }

    private class IdleState : LocomotionState
    {
        protected override PlayerLocomotion Kind => PlayerLocomotion.Idle;

        public override void Enter()
        {
            base.Enter();
            Owner.IdleTime = 0f;
        }

        public override void Tick()
        {
            base.Tick();
            if (Owner.controls.AnyKeyDown) Owner.IdleTime = 0f;
            else Owner.IdleTime += Time.deltaTime;
        }
    }

    private class RunState : LocomotionState
    {
        protected override PlayerLocomotion Kind => PlayerLocomotion.Run;
    }

    private class RiseState : LocomotionState
    {
        protected override PlayerLocomotion Kind => PlayerLocomotion.Rise;
    }

    private class FallState : LocomotionState
    {
        protected override PlayerLocomotion Kind => PlayerLocomotion.Fall;
    }

    private class WallSlideState : LocomotionState
    {
        protected override PlayerLocomotion Kind => PlayerLocomotion.WallSlide;
        public override void FixedTick()
        {
            base.FixedTick();
            if (Owner.motor.CanWallSlide(Owner.controls.Move)) Owner.motor.SlideOnWall();
        }
    }

    /// <summary>Bam tuong (giu Shift/RT): W/S leo len xuong, Space nhay tuong; het tuong o dinh thi nhay qua mep.</summary>
    private class WallClimbState : LocomotionState
    {
        protected override PlayerLocomotion Kind => PlayerLocomotion.WallClimb;

        public override void Enter()
        {
            base.Enter();
            Owner.motor.BeginWallClimb();
        }

        public override void FixedTick()
        {
            // Nhay tuong truoc: neu nhay duoc thi state ket thuc o lan chuyen state ke tiep.
            if (Owner.TryJump()) return;
            Owner.controls.ConsumeJumpRelease();
            if (Owner.motor.CanWallClimb(Owner.controls.GrabHeld)) Owner.motor.ClimbWall(Owner.controls.ClimbAxis);
        }

        public override void Exit()
        {
            // Roi tuong o dinh khi dang leo len: nhay nhe qua mep thay vi roi lai.
            var c = Owner.controls;
            if (!Owner.dead && c.GrabHeld && c.ClimbAxis > 0f && Owner.motor.WallDirection == 0 && !Owner.motor.Grounded
                && !Owner.motor.WallJumpProtected && Owner.motor.Velocity.y >= 0f)
                Owner.motor.ClimbLedgeHop();
            Owner.motor.EndWallClimb();
        }
    }

    private class DashState : State<PlayerController>
    {
        public override void Enter()
        {
            Owner.controls.ConsumeDash(out int bufferedDirection);
            if (Owner.combat != null) Owner.combat.ResetCombo();
            Owner.controls.ConsumeJumpRelease();
            Owner.dashDirection = bufferedDirection != 0 ? bufferedDirection : Owner.motor.FacingRight ? 1 : -1;
            Owner.motor.Face(Owner.dashDirection);
            Owner.motor.BeginDash(Owner.dashDirection);
            Owner.SetLocomotion(PlayerLocomotion.Dash);
            Owner.DashStarted?.Invoke();
        }

        public override void FixedTick()
        {
            if (Owner.DashActive) Owner.motor.HoldDash(Owner.dashDirection);
        }

        public override void Exit()
        {
            Owner.motor.EndDash();
            Owner.nextDashTime = Time.time + Owner.Config.dashCooldown;
        }
    }

    private class BashState : State<PlayerController>
    {
        public override void Enter()
        {
            Owner.SetLocomotion(PlayerLocomotion.Bash);
            if (Owner.combat != null) Owner.combat.ResetCombo();
            Owner.controls.ConsumeJumpRelease();
            Owner.bash.Begin();
        }

        public override void Tick() => Owner.bash.Tick();

        public override void FixedTick() => Owner.bash.FixedTick();

        // Hurt / Dead / Locked / reset roi state nay -> huy ngam hoac phong, tra time scale va gravity.
        public override void Exit() => Owner.bash.Cancel();
    }

    private class AttackState : State<PlayerController>
    {
        public override void Enter()
        {
            Owner.SetLocomotion(PlayerLocomotion.Attack);
            Owner.controls.ConsumeJumpRelease();
            Owner.combat.Begin();
        }

        public override void Tick()
        {
            Owner.combat.Tick();
            // Lap lai don don sau recovery neu con lenh bam som; khong can doi state de re-enter.
            if (!Owner.combat.Active && Owner.combat.CanStart) Owner.combat.Begin();
        }

        public override void FixedTick() => Owner.combat.FixedTick();

        public override void Exit() => Owner.combat.Cancel();
    }

    private class CastState : State<PlayerController>
    {
        public override void Enter()
        {
            Owner.SetLocomotion(PlayerLocomotion.Cast);
            Owner.controls.ConsumeJumpRelease();
            // Skill ngat don dang danh (neu co).
            if (Owner.combat != null) Owner.combat.ResetCombo();
            Owner.skills.Begin();
        }

        public override void Tick() => Owner.skills.Tick();

        public override void FixedTick() => Owner.skills.FixedTick();

        public override void Exit() => Owner.skills.Cancel();
    }

    private class HurtState : State<PlayerController>
    {
        public override void Enter()
        {
            if (Owner.combat != null) Owner.combat.ResetCombo();
            if (Owner.skills != null) Owner.skills.Cancel();

            // Bo cac lenh dang cho de khong tu dong dash/danh ngay sau khi het choang.
            Owner.controls.ResetInput();

            Owner.motor.SetVelocity(Owner.hurtKnockback);
            Owner.SetLocomotion(PlayerLocomotion.Hurt);
        }

        public override void FixedTick() => Owner.motor.DampHorizontal(Owner.Config.hurtDrag);
    }

    private class LockedState : State<PlayerController>
    {
        public override void Enter()
        {
            Owner.motor.ClearWallMotion();
            Owner.motor.StopHorizontal();
            Owner.SetLocomotion(PlayerLocomotion.Locked);
        }

        // Van de vat ly tu lo (roi xuong) nhung khong cho chay ngang.
        public override void FixedTick() => Owner.motor.MoveHorizontal(0f);
    }

    private class DeadState : State<PlayerController>
    {
        public override void Enter()
        {
            if (Owner.combat != null) Owner.combat.ResetCombo();
            if (Owner.skills != null) Owner.skills.Cancel();
            Owner.controls.ResetInput();
            Owner.motor.SetVelocity(Vector2.zero);
            Owner.motor.SetGravityOverride(0f);
            Owner.SetLocomotion(PlayerLocomotion.Dead);
        }
        public override void FixedTick() => Owner.motor.SetVelocity(Vector2.zero);
        public override void Exit() => Owner.motor.ClearGravityOverride();
    }
}
