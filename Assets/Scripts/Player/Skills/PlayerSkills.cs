using System;
using UnityEngine;

public enum SkillFailReason
{
    None,
    NoSkill,
    Cooldown,
    NotEnoughMana,
    Airborne
}

/// <summary>
/// He skill cua player: cac slot (moi slot gan 1 phim trong PlayerInputReader), hoi chieu, tieu mana, niem skill.
///   - Skill la asset (<see cref="SkillData"/>); slot co the doi luc choi qua <see cref="SetSlot"/> (mo khoa skill moi).
///   - PlayerController (CastState) goi Begin / Tick / FixedTick / Cancel. Script khac (pose, am thanh, UI)
///     chi dang ky event SkillStarted / SkillEnded / SkillFailed / SlotChanged va doc Cooldown01.
///   - Mana lay tu ManaManager (khong co thi coi nhu khong ton mana).
///   - Bi thuong hoac dash ngat skill: hieu ung chua kich hoat thi bo, khong hoan mana va hoi chieu.
/// </summary>
public class PlayerSkills : MonoBehaviour
{
    private const int MaxEffectsPerSkill = 64;

    [SerializeField] private PlayerController controller;
    [Tooltip("Slot i dung phim skillKeys[i] cua PlayerInputReader.")]
    [SerializeField] private SkillData[] slots = new SkillData[0];
    [Tooltip("De trong = dung ManaManager.Instance.")]
    [SerializeField] private ManaManager mana;

    private float[] cooldownEnd;
    private int currentSlot = -1;
    private float castTime;
    private ulong firedMask;

    public bool Active { get; private set; }
    public SkillData Current { get; private set; }
    public int SlotCount => slots.Length;

    /// <summary>(skill, slot) khi bat dau niem.</summary>
    public event Action<SkillData, int> SkillStarted;
    /// <summary>(skill, slot, hoan thanh?) khi het niem hoac bi huy.</summary>
    public event Action<SkillData, int, bool> SkillEnded;
    /// <summary>Bam phim nhung khong niem duoc (hoi chieu, thieu mana, tren khong...).</summary>
    public event Action<int, SkillFailReason> SkillFailed;
    public event Action<int, SkillData> SlotChanged;

    private ManaManager Mana => mana != null ? mana : ManaManager.Instance;

    private SkillStats currentStats;

    /// <summary>Thong so thuc te cua skill theo bac va nhanh da nang cap (bac 1 neu skill khong co id).</summary>
    public SkillStats StatsOf(SkillData skill)
    {
        if (skill.id == SkillId.None) return skill.Resolve(1, -1);
        return SkillProgressManager.Instance.StatsOf(skill);
    }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        cooldownEnd = new float[slots.Length];
    }

    private void OnDisable()
    {
        Active = false;
        Current = null;
    }

    // ---------------- Truy van ----------------

    public SkillData GetSlot(int slot) => slot >= 0 && slot < slots.Length ? slots[slot] : null;

    public float CooldownRemaining(int slot)
    {
        if (slot < 0 || slot >= cooldownEnd.Length) return 0f;
        return Mathf.Max(0f, cooldownEnd[slot] - Time.time);
    }

    /// <summary>0 = san sang, 1 = vua niem xong (de ve vong hoi chieu tren HUD).</summary>
    public float Cooldown01(int slot)
    {
        SkillData s = GetSlot(slot);
        if (s == null) return 0f;
        float cooldown = StatsOf(s).Cooldown;
        if (cooldown <= 0f) return 0f;
        return Mathf.Clamp01(CooldownRemaining(slot) / cooldown);
    }

    public SkillFailReason Check(int slot)
    {
        SkillData s = GetSlot(slot);
        if (s == null) return SkillFailReason.NoSkill;
        if (CooldownRemaining(slot) > 0f) return SkillFailReason.Cooldown;
        if (!s.allowAirborne && !controller.Motor.Grounded) return SkillFailReason.Airborne;
        int manaCost = StatsOf(s).ManaCost;
        if (manaCost > 0 && Mana != null && Mana.NowMana < manaCost) return SkillFailReason.NotEnoughMana;
        return SkillFailReason.None;
    }

    /// <summary>Co phim skill dang cho trong bo dem va niem duoc ngay.</summary>
    public bool CanStart
    {
        get
        {
            if (Active) return false;
            int slot = controller.Controls.BufferedSkillSlot();
            return slot >= 0 && Check(slot) == SkillFailReason.None;
        }
    }

    /// <summary>Doi skill o mot slot (mo khoa, trang bi). Bo qua khi dang niem. Tra ve false neu khong doi duoc.</summary>
    public bool SetSlot(int slot, SkillData skill)
    {
        if (Active || slot < 0 || slot >= slots.Length) return false;
        slots[slot] = skill;
        cooldownEnd[slot] = 0f;
        SlotChanged?.Invoke(slot, skill);
        return true;
    }

    private void Update()
    {
        if (Active) return;

        // Bam phim ma khong niem duoc: bao ly do va bo lenh de khong dong bang trong bo dem.
        int slot = controller.Controls.BufferedSkillSlot();
        if (slot < 0) return;
        SkillFailReason reason = Check(slot);
        if (reason == SkillFailReason.None) return;

        controller.Controls.ConsumeSkill(slot);
        SkillFailed?.Invoke(slot, reason);
    }

    // ---------------- Dieu khien tu PlayerController ----------------

    public void Begin()
    {
        int slot = controller.Controls.BufferedSkillSlot();
        SkillData skill = GetSlot(slot);
        controller.Controls.ConsumeSkill(slot);

        currentStats = StatsOf(skill);
        if (currentStats.ManaCost > 0 && Mana != null)
            for (int i = 0; i < currentStats.ManaCost; i++) Mana.UseMana();
        cooldownEnd[slot] = Time.time + currentStats.Cooldown;

        // Quay ve phia dang bam truoc khi niem.
        float x = controller.Controls.Move;
        if (x > 0.01f && !controller.Motor.FacingRight) controller.Motor.Face(1);
        else if (x < -0.01f && controller.Motor.FacingRight) controller.Motor.Face(-1);

        currentSlot = slot;
        Current = skill;
        castTime = 0f;
        firedMask = 0UL;
        Active = true;

        SkillStarted?.Invoke(skill, slot);
    }

    /// <summary>Goi moi Update khi dang o CastState.</summary>
    public void Tick()
    {
        if (!Active) return;

        castTime += Time.deltaTime;

        int count = Mathf.Min(currentStats.EffectCount, MaxEffectsPerSkill);
        var context = new SkillContext(controller, Current);
        for (int i = 0; i < count; i++)
        {
            ulong bit = 1UL << i;
            if ((firedMask & bit) != 0UL) continue;

            SkillEffectEntry entry = currentStats.GetEffect(i);
            if (castTime < entry.time) continue;

            firedMask |= bit;
            if (entry.effect != null) entry.effect.Execute(context);
        }

        if (castTime >= currentStats.Duration) End(true);
    }

    public void FixedTick()
    {
        if (!Active) return;
        controller.Motor.MoveHorizontal(controller.Controls.Move * Current.moveControl);
    }

    /// <summary>Huy skill dang niem (dash, bi thuong). Mana va hoi chieu khong duoc hoan.</summary>
    public void Cancel()
    {
        if (Active) End(false);
    }

    private void End(bool completed)
    {
        SkillData skill = Current;
        int slot = currentSlot;
        Active = false;
        Current = null;
        currentSlot = -1;
        SkillEnded?.Invoke(skill, slot, completed);
    }
}
