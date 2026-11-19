using UnityEngine;

/// <summary>Thong tin cho mot hieu ung skill khi no kich hoat.</summary>
public readonly struct SkillContext
{
    public readonly PlayerController Player;
    public readonly SkillData Skill;

    /// <summary>+1 nhin phai, -1 nhin trai.</summary>
    public int Direction => Player.Motor.FacingRight ? 1 : -1;
    public Transform Transform => Player.transform;

    public SkillContext(PlayerController player, SkillData skill)
    {
        Player = player;
        Skill = skill;
    }
}

/// <summary>
/// Mot "vien gach" cua skill: sat thuong vung, ban dan, hoi mau, buff, dich chuyen...
/// Moi loai hieu ung la mot ScriptableObject ke thua lop nay; SkillData ghep nhieu hieu ung theo moc thoi gian.
/// Them kieu skill moi = viet 1 lop con SkillEffect (khong phai sua PlayerSkills / PlayerController),
/// roi tao asset va gan vao SkillData.
/// Luu y: asset duoc dung chung, khong luu trang thai theo lan niem tren asset.
/// </summary>
public abstract class SkillEffect : ScriptableObject
{
    public abstract void Execute(in SkillContext context);
}
