using UnityEngine;

/// <summary>
/// Vat the co the nhan sat thuong (quai, boss, vat the pha duoc...). Combat chi can biet interface nay,
/// them loai muc tieu moi chi viec implement, khong phai sua code danh.
/// </summary>
public interface IDamageable
{
    /// <param name="amount">Luong sat thuong.</param>
    /// <param name="hitPoint">Diem trung don (the gioi), dung cho hieu ung.</param>
    void TakeDamage(int amount, Vector2 hitPoint);
}
