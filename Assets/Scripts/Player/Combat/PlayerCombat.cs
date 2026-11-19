using System;
using UnityEngine;

public enum MeleeAttackPhase { None, Windup, Active, Recovery }

/// <summary>
/// Combat can chien cua player, dieu khien theo vu khi (<see cref="WeaponData"/>) dang trang bi.
///   - Vu khi la asset: combo (AttackComboData), sat thuong cong them, hieu ung, hinh dang cam tay.
///     Danh sach weapons la cac vu khi player dang co; phim doi vu khi (Q) hoac Equip/Next doi giua chung.
///   - Khong dung Animator hay animation event: moc thoi gian (hitStart/hitEnd/chainFrom) nam trong asset.
///   - Hitbox la OverlapBox theo local cua player (tu dao theo huong nhin), moi muc tieu chi trung 1 lan / don.
///   - Muc tieu la bat ky vat the nao co <see cref="IDamageable"/> va tag nam trong targetTags.
///   - PlayerController (AttackState) goi Begin / Tick / FixedTick / Cancel; script khac (pose, am thanh,
///     rung camera, hit-stop, UI) chi dang ky event StepStarted / HitLanded / Ended / WeaponChanged.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [Tooltip("Cac vu khi player dang co. Phan tu dau la vu khi luc bat dau.")]
    [SerializeField] private WeaponData[] weapons = new WeaponData[0];
    [Tooltip("Noi gan hinh dang vu khi (vd bone ban tay). De trong = khong hien hinh dang.")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private string[] targetTags = { "Enemy", "Boss" };
    [Tooltip("Gioi han layer de quet cho nhanh. De mac dinh (Everything) neu chua can.")]
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField, Range(0f, 1f)] private float airMoveControl = .65f;
    public bool Upward { get; private set; }
    /// <summary>Tren khong + giu xuong: dam kiem thang xuong; trung thi bat len (1 lan moi lan roi).</summary>
    public bool Downward { get; private set; }
    /// <summary>Tren dat + giu xuong: don ngang tam xa hon.</summary>
    public bool Long { get; private set; }
    private const float LongReach = 1.3f, LongWidth = 1.45f;
    public Vector2 HitOffset => Upward ? new Vector2(0f, step.hitOffset.x)
        : Downward ? new Vector2(0f, -step.hitOffset.x)
        : Long ? new Vector2(step.hitOffset.x * LongReach, step.hitOffset.y) : step.hitOffset;
    public Vector2 HitSize => Upward || Downward ? new Vector2(step.hitSize.y, step.hitSize.x)
        : Long ? new Vector2(step.hitSize.x * LongWidth, step.hitSize.y) : step.hitSize;
    public Vector2 HitCenter => controller.Motor.Position + (Vector2)controller.transform.TransformVector(HitOffset);

    private HitScanner scanner;
    private Action<Collider2D> onScanHit;

    private int weaponIndex;
    private GameObject heldInstance;

    private AttackComboData combo;
    private AttackStep step;
    private int index;
    private int nextIndex;
    private float stepTime;
    private float lastEndTime = -100f;
    private bool chainQueued;
    private int lungeDirection = 1;

    public bool Active { get; private set; }
    public float Elapsed => stepTime;
    public MeleeAttackPhase Phase => !Active ? MeleeAttackPhase.None
        : stepTime < step.hitStart ? MeleeAttackPhase.Windup
        : stepTime <= step.hitEnd ? MeleeAttackPhase.Active : MeleeAttackPhase.Recovery;
    public int StepIndex => index;
    public int ComboStepCount => combo != null ? combo.StepCount : 0;
    public bool ChainQueued => Active && chainQueued;
    public bool CanQueueNext => Active && index + 1 < ComboStepCount
        && !chainQueued && stepTime >= step.chainFrom && stepTime < step.duration;
    public AttackStep CurrentStep => step;
    public WeaponData Weapon { get; private set; }
    public int WeaponCount => weapons.Length;

    /// <summary>Bat dau mot don (don thu may do bo dem combo quyet dinh).</summary>
    public event Action<AttackStep, int> StepStarted;
    /// <summary>Trung muc tieu: (don, collider bi trung).</summary>
    public event Action<AttackStep, Collider2D> HitLanded;
    /// <summary>Het don cuoi (hoan thanh hoac bi huy).</summary>
    public event Action Ended;
    /// <summary>Doi vu khi (vu khi moi, co the null).</summary>
    public event Action<WeaponData> WeaponChanged;

    /// <summary>Co the bat dau chuoi danh moi: co vu khi, dang ranh va co lenh danh trong bo dem.</summary>
    public bool CanStart
    {
        get
        {
            if (combo == null || combo.StepCount == 0 || Active) return false;
            if (!combo.allowAirborne && !controller.Motor.Grounded) return false;
            return controller.Controls.AttackBuffered;
        }
    }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        scanner = new HitScanner(targetTags, targetMask);
        onScanHit = c =>
        {
            HitLanded?.Invoke(step, c);
            // Chem xuong trung: bat len keo dai thoi gian tren khong.
            if (Downward) controller.Motor.TryPogo(controller.Config.pogoVelocity);
        };
    }

    private void Start()
    {
        // Trang bi vu khi dau tien sau khi cac script khac da kip dang ky event.
        if (weapons.Length > 0) Equip(0);
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void Update()
    {
        // Chi doi vu khi luc khong dang danh de combo khong bi doi giua chung.
        if (!Active && controller.Controls.ConsumeSwitchWeapon()) Next();
    }

    // ---------------- Vu khi ----------------

    /// <summary>Trang bi vu khi theo vi tri trong danh sach. Bo qua khi dang danh. Tra ve false neu khong doi duoc.</summary>
    public bool Equip(int i)
    {
        if (Active || i < 0 || i >= weapons.Length) return false;
        weaponIndex = i;
        Apply(weapons[i]);
        return true;
    }

    /// <summary>Chuyen sang vu khi ke tiep (quay vong).</summary>
    public bool Next()
    {
        if (weapons.Length < 2) return false;
        return Equip((weaponIndex + 1) % weapons.Length);
    }

    private void Apply(WeaponData weapon)
    {
        Weapon = weapon;
        combo = weapon != null ? weapon.combo : null;
        nextIndex = 0;
        lastEndTime = -100f;

        if (heldInstance != null) Destroy(heldInstance);
        heldInstance = null;
        if (weapon != null && weapon.heldVisual != null && weaponSocket != null)
            heldInstance = Instantiate(weapon.heldVisual, weaponSocket);

        WeaponChanged?.Invoke(weapon);
    }

    // ---------------- Dieu khien tu PlayerController ----------------

    public void Begin()
    {
        if (Active || combo == null || combo.StepCount == 0) return;
        controller.Controls.ConsumeAttack();
        if (Time.time - lastEndTime > combo.comboResetTime) nextIndex = 0;
        StartStep(Mathf.Clamp(nextIndex, 0, combo.StepCount - 1));
    }

    /// <summary>Goi moi Update khi dang o AttackState.</summary>
    public void Tick() => Tick(Time.deltaTime);

    public void Tick(float deltaTime)
    {
        if (!Active || deltaTime <= 0f) return;

        float previousTime = stepTime;
        stepTime += deltaTime;

        if (!chainQueued && index + 1 < combo.StepCount
            && previousTime < step.duration && stepTime >= step.chainFrom && controller.Controls.AttackBuffered)
        {
            controller.Controls.ConsumeAttack();
            chainQueued = true;
        }

        // Quet ca khi mot frame dai di qua toan bo cua so gay sat thuong.
        if (stepTime >= step.hitStart && previousTime <= step.hitEnd) ScanHits();

        if (stepTime >= step.duration)
        {
            if (chainQueued) StartStep(index + 1);
            else End(true);
        }
    }

    /// <summary>Goi moi FixedUpdate khi dang o AttackState: lao ve truoc roi con lai dieu khien ngang.</summary>
    public void FixedTick()
    {
        if (!Active) return;

        if (stepTime < step.lungeTime)
            controller.Motor.SetHorizontalVelocity(lungeDirection * step.lungeSpeed);
        else
            controller.Motor.MoveHorizontal(controller.Controls.Move * (controller.Motor.Grounded ? step.moveControl : airMoveControl));
    }

    /// <summary>Huy don dang chay (dash, bi thuong). Combo ve don dau.</summary>
    public void Cancel()
    {
        if (Active) End(false);
    }

    /// <summary>Reset ca combo dang cho bam tiep, ke ca khi don truoc da hoan thanh.</summary>
    public void ResetCombo()
    {
        Cancel();
        nextIndex = 0;
        chainQueued = false;
        lastEndTime = -100f;
    }

    // ---------------- Noi bo ----------------

    private void StartStep(int i)
    {
        index = i;
        step = combo.GetStep(i);
        stepTime = 0f;
        chainQueued = false;
        scanner.Reset();
        Active = true;
        Upward = controller.Controls.UpHeld;
        Downward = !Upward && controller.Controls.DownHeld && !controller.Motor.Grounded;
        Long = !Upward && controller.Controls.DownHeld && controller.Motor.Grounded;

        // Don moi quay ve phia dang bam (neu co), roi chot huong lao.
        float x = controller.Controls.Move;
        if (x > 0.01f && !controller.Motor.FacingRight) controller.Motor.Face(1);
        else if (x < -0.01f && controller.Motor.FacingRight) controller.Motor.Face(-1);
        lungeDirection = controller.Motor.FacingRight ? 1 : -1;

        StepStarted?.Invoke(step, i);
    }

    private void End(bool completed)
    {
        Active = false;
        lastEndTime = Time.time;
        nextIndex = completed && index + 1 < combo.StepCount ? index + 1 : 0;
        Ended?.Invoke();
    }

    private void ScanHits()
    {
        Vector2 center = HitCenter;
        int damage = step.damage + (Weapon != null ? Weapon.bonusDamage : 0);
        GameObject effect = step.hitEffect != null ? step.hitEffect : (Weapon != null ? Weapon.defaultHitEffect : null);

        scanner.Scan(center, HitSize, damage, effect, onScanHit, controller.Motor.Position,
            controller.GetComponent<Rigidbody2D>(), true);
    }

    private void OnDrawGizmosSelected()
    {
        AttackComboData preview = combo != null ? combo
            : (weapons.Length > 0 && weapons[0] != null ? weapons[0].combo : null);
        if (preview == null || preview.StepCount == 0) return;

        Transform t = controller != null ? controller.transform : transform;
        Gizmos.color = Color.red;
        if (Active)
        {
            Gizmos.DrawWireCube(HitCenter, HitSize);
            return;
        }
        for (int i = 0; i < preview.StepCount; i++)
        {
            AttackStep s = preview.GetStep(i);
            Gizmos.DrawWireCube(t.TransformPoint(s.hitOffset), s.hitSize);
        }
    }
}
