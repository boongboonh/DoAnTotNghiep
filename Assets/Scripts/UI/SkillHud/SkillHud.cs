using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Giao dien chung cua cac o skill. Doi hinh khung, mau, co chu o day, khong can sua code.</summary>
[Serializable]
public class SkillHudStyle
{
    public Vector2 slotSize = new Vector2(64f, 64f);
    public float spacing = 10f;
    [Tooltip("Khung o skill. De trong = o vuong mau don.")]
    public Sprite frameSprite;
    public Sprite keycapSprite;
    public float iconPadding = 6f;

    [Header("Mau")]
    public Color frameColor = new Color(1f, 1f, 1f, 0.9f);
    public Color emptyColor = new Color(1f, 1f, 1f, 0.3f);
    public Color cooldownColor = new Color(0f, 0f, 0f, 0.6f);
    public Color unusableColor = new Color(0.5f, 0.5f, 0.55f, 1f);
    public Color failColor = new Color(1f, 0.35f, 0.3f, 1f);
    public Color keyColor = Color.white;
    public Color cooldownTextColor = Color.white;

    [Header("Chu")]
    public int keyFontSize = 16;
    public int nameFontSize = 12;
    public int cooldownFontSize = 24;

    [Header("Hieu ung")]
    [Tooltip("Do nay to ra khi niem skill (0.1 = 10%).")]
    public float castPulse = 0.12f;
    [Tooltip("Bien do rung (pixel) khi bam skill ma khong niem duoc.")]
    public float failShake = 4f;
}

/// <summary>
/// HUD skill: hien cac slot cua PlayerSkills thanh mot hang o, moi o co icon, phim bam, vong hoi chieu
/// va so giay con lai. O toi di khi thieu mana / khong the dung tren khong, rung do khi bam khong niem duoc,
/// nay nhe khi niem.
///   - Dat component nay len mot RectTransform trong Canvas (vd goc duoi man hinh); cac o xep tu trai sang phai.
///   - Tu dung giao dien bang code (khong can prefab, khong dung TextMesh Pro). Doi hinh/mau qua Style.
///   - Tu tim PlayerSkills trong scene; co the gan tay vao Skills.
/// </summary>
public class SkillHud : MonoBehaviour
{
    [Serializable]
    private struct SkillIcon
    {
        public SkillId id;
        public Sprite sprite;
    }

    [SerializeField] private PlayerSkills skills;
    [SerializeField] private PlayerInputReader controls;
    [SerializeField] private SkillHudStyle style = new SkillHudStyle();
    [SerializeField] private string[] unboundKeys = Array.Empty<string>();
    [SerializeField] private SkillIcon[] defaultIcons = Array.Empty<SkillIcon>();

    private const float SearchInterval = 0.5f;

    private readonly List<SkillHudSlot> views = new List<SkillHudSlot>();
    private float nextSearch;
    private PlayerSkills bound;
    private bool showingUnboundSlots;

    private void OnDisable()
    {
        Unbind();
        showingUnboundSlots = false;
        nextSearch = 0f;
    }

    private void Update()
    {
        if (bound == null && !TryBind())
        {
            if (showingUnboundSlots) return;
            Build(unboundKeys.Length, null);
            for (int i = 0; i < views.Count; i++)
                views[i].Refresh(null, 0f, 0f, SkillFailReason.NoSkill);
            showingUnboundSlots = true;
            return;
        }

        for (int i = 0; i < views.Count; i++)
        {
            SkillData skill = bound.GetSlot(i);
            views[i].Refresh(
                skill,
                bound.Cooldown01(i),
                bound.CooldownRemaining(i),
                bound.Check(i),
                FindIcon(skill));
        }
    }

    private Sprite FindIcon(SkillData skill)
    {
        if (skill == null || skill.icon != null) return null;
        foreach (SkillIcon icon in defaultIcons)
            if (icon.id == skill.id) return icon.sprite;
        return null;
    }

    private bool TryBind()
    {
        if (Time.unscaledTime < nextSearch) return false;
        nextSearch = Time.unscaledTime + SearchInterval;

        PlayerSkills found = skills != null ? skills : FindFirstObjectByType<PlayerSkills>();
        if (found == null) return false;

        bound = found;
        PlayerInputReader input = controls != null ? controls : found.GetComponent<PlayerInputReader>();

        bound.SkillStarted += OnSkillStarted;
        bound.SkillFailed += OnSkillFailed;
        Build(bound.SlotCount, input);
        showingUnboundSlots = false;
        return true;
    }

    private void Unbind()
    {
        if (bound == null) return;
        bound.SkillStarted -= OnSkillStarted;
        bound.SkillFailed -= OnSkillFailed;
        bound = null;
    }

    private void Build(int count, PlayerInputReader input)
    {
        for (int i = 0; i < views.Count; i++)
            if (views[i] != null) Destroy(views[i].gameObject);
        views.Clear();

        var root = (RectTransform)transform;
        for (int i = 0; i < count; i++)
        {
            string key = input != null ? input.SkillKeyName(i)
                : i < unboundKeys.Length ? unboundKeys[i] : string.Empty;
            SkillHudSlot view = SkillHudSlot.Create(root, style, key, "Slot" + i);
            ((RectTransform)view.transform).anchoredPosition = new Vector2(i * (style.slotSize.x + style.spacing), 0f);
            views.Add(view);
        }
    }

    private void OnSkillStarted(SkillData skill, int slot)
    {
        if (slot >= 0 && slot < views.Count) views[slot].PlayCast();
    }

    private void OnSkillFailed(int slot, SkillFailReason reason)
    {
        if (slot >= 0 && slot < views.Count) views[slot].PlayFail();
    }
}
