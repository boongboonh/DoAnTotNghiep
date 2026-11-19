using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Mot hieu ung kich hoat tai moc <see cref="time"/> giay ke tu luc bat dau niem.</summary>
[Serializable]
public class SkillEffectEntry
{
    [Tooltip("Thoi diem kich hoat (giay) ke tu luc bat dau niem.")]
    public float time = 0.15f;
    public SkillEffect effect;
}

/// <summary>
/// Mot skill cua player, luu thanh asset: chi phi, hoi chieu, thoi gian niem va danh sach hieu ung theo moc thoi gian.
/// Tao skill moi bang cach tao asset (Create > Dang Dang > Skill) roi ghep cac SkillEffect, khong can viet script
/// tru khi can mot loai hieu ung moi (khi do ke thua <see cref="SkillEffect"/>).
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Skill", fileName = "Skill")]
public class SkillData : ScriptableObject
{
    public const int MaxTier = 3;

    [Header("Nhan dang")]
    [Tooltip("Dinh danh dung cho save va bang ky nang. None = skill khong di qua he tien trinh.")]
    public SkillId id = SkillId.None;
    public string displayName = "Skill";
    public Sprite icon;
    [TextArea] public string description;
    [Tooltip("Goi y hien tren bang ky nang khi chua mo khoa (vd: Nhan o Dong Co Nhin). De trong = dau ???.")]
    public string unlockHint;

    [Header("Chi phi")]
    [Tooltip("Thoi gian hoi chieu (giay), tinh tu luc bat dau niem.")]
    public float cooldown = 3f;
    [Tooltip("So mana tieu hao. 0 = khong ton mana.")]
    [Min(0)] public int manaCost = 1;
    public bool allowAirborne = true;

    [Header("Niem")]
    [Tooltip("Tong thoi gian niem (giay). Player khong dieu khien duoc trong luc nay (tru moveControl).")]
    public float duration = 0.5f;
    [Tooltip("Phan tram dieu khien ngang con lai trong luc niem (0 = dung yen).")]
    [Range(0f, 1f)] public float moveControl = 0f;
    [Tooltip("Ten pose trong PoseSet de PosePlayer phu len trong luc niem.")]
    public string poseName = "Cast";

    [Header("Hieu ung")]
    [SerializeField] private List<SkillEffectEntry> effects = new List<SkillEffectEntry>();

    [Header("Nang cap")]
    [Tooltip("Bac 2: thay doi cong them vao thong so goc.")]
    public SkillTier tier2 = new SkillTier { sapCost = 3 };
    [Tooltip("Bac 3: chon 1 trong 2 nhanh (phan tu 0 va 1).")]
    public SkillTier[] tier3Branches = { new SkillTier { sapCost = 6 }, new SkillTier { sapCost = 6 } };

    public int EffectCount => effects.Count;
    public SkillEffectEntry GetEffect(int index) => effects[index];

    /// <summary>Nhanh bac 3 theo chi so (0 hoac 1); null neu khong hop le.</summary>
    public SkillTier GetBranch(int branch)
    {
        if (tier3Branches == null || branch < 0 || branch >= tier3Branches.Length) return null;
        return tier3Branches[branch];
    }

    /// <summary>Thong so thuc te o bac va nhanh cho truoc (bac 1 = thong so goc).</summary>
    public SkillStats Resolve(int tier = 1, int branch = -1) => new SkillStats(this, tier, branch);
}
