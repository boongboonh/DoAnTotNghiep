using UnityEngine;

/// <summary>
/// Vat the trong the gioi (hoa, rec an, cong sinh hoc, cong tac...) phan ung voi vung skill.
/// Gan len GameObject co Collider2D (hoac cha cua no). Vung skill goi moi nhip tick khi vat dang nam trong vung.
/// </summary>
public interface IWorldReactive
{
    /// <param name="source">Skill dang tac dong (dua vao id de biet la Giu Nhin, Nghe Re...).</param>
    /// <param name="zone">Vung dang chua vat the nay, dung de doc thoi gian con lai neu can.</param>
    void OnSkillZone(SkillData source, SkillZone zone);
}
