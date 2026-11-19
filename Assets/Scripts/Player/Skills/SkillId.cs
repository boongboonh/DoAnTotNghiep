/// <summary>
/// Dinh danh on dinh cua skill, dung de luu save. Gia tri so KHONG duoc doi hoac tai su dung
/// (doi ten asset hay ten hien thi khong anh huong tien trinh da luu). Them skill moi = them cuoi danh sach.
/// </summary>
public enum SkillId
{
    None = 0,

    // Skill goc (moi vung chinh mot skill)
    GiuNhin = 1,
    NgheRe = 2,
    LapVet = 3,
    SaiBong = 4,
    DaoTrong = 5,
    NhipCham = 6,

    // Skill phu va bi mat
    NuocDung = 7,
    NgungKhoanhKhac = 8,
    Goc = 9
}
