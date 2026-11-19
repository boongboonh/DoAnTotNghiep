using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thay doi them vao skill khi len mot bac. Chi la du lieu: cong/tru vao thong so goc va noi them hieu ung.
/// Bac 2 la <see cref="SkillData.tier2"/>; bac 3 la mot trong hai nhanh <see cref="SkillData.tier3Branches"/>.
/// </summary>
[Serializable]
public class SkillTier
{
    [Tooltip("Nhua thoi gian can de len bac nay.")]
    [Min(0)] public int sapCost = 3;
    [Tooltip("Ten nhanh (chi dung cho bac 3), hien tren bang ky nang.")]
    public string branchName;
    [TextArea] public string note;

    [Header("Thay doi thong so (cong vao gia tri goc)")]
    public float cooldownDelta;
    public int manaDelta;
    public float durationDelta;

    [Header("Hieu ung them")]
    public List<SkillEffectEntry> extraEffects = new List<SkillEffectEntry>();
}

/// <summary>
/// Thong so skill sau khi ap dung bac va nhanh. Khong luu trang thai; tao moi qua <see cref="SkillData.Resolve"/>.
/// </summary>
public readonly struct SkillStats
{
    public readonly SkillData Skill;
    public readonly int Tier;
    public readonly int Branch;
    public readonly float Cooldown;
    public readonly int ManaCost;
    public readonly float Duration;

    private readonly SkillTier tier2;
    private readonly SkillTier branchTier;

    public SkillStats(SkillData skill, int tier, int branch)
    {
        Skill = skill;
        Tier = Mathf.Clamp(tier, 1, SkillData.MaxTier);
        Branch = branch;

        SkillTier t2 = Tier >= 2 ? skill.tier2 : null;
        SkillTier t3 = Tier >= 3 ? skill.GetBranch(branch) : null;
        tier2 = t2;
        branchTier = t3;

        float cd = skill.cooldown, dur = skill.duration;
        int mana = skill.manaCost;
        if (t2 != null) { cd += t2.cooldownDelta; mana += t2.manaDelta; dur += t2.durationDelta; }
        if (t3 != null) { cd += t3.cooldownDelta; mana += t3.manaDelta; dur += t3.durationDelta; }

        Cooldown = Mathf.Max(0f, cd);
        ManaCost = Mathf.Max(0, mana);
        Duration = Mathf.Max(0.05f, dur);
    }

    private int ExtraCount(SkillTier t) => t != null && t.extraEffects != null ? t.extraEffects.Count : 0;

    public int EffectCount => Skill.EffectCount + ExtraCount(tier2) + ExtraCount(branchTier);

    /// <summary>Hieu ung thu i: hieu ung goc, roi hieu ung them cua bac 2, roi cua nhanh bac 3.</summary>
    public SkillEffectEntry GetEffect(int index)
    {
        int n = Skill.EffectCount;
        if (index < n) return Skill.GetEffect(index);
        index -= n;

        n = ExtraCount(tier2);
        if (index < n) return tier2.extraEffects[index];
        index -= n;

        return branchTier.extraEffects[index];
    }
}
