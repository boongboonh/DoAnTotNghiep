using UnityEngine;

/// <summary>
/// Con so tam cua nguon nhua thoi gian (chinh o day, khong rai rac). Toan bo skill 6 goc nang het
/// ton khoang 54 nhua (moi skill: bac 2 = 3, bac 3 = 6), chua tinh dat lai nhanh.
/// </summary>
public static class SapEconomy
{
    /// <summary>Ti le quai thuong roi nhua khi chet (0..1).</summary>
    public const float EnemyDropChance = 0.2f;
    public const int EnemyAmount = 1;

    /// <summary>Boss roi chac chan, chi mot lan cho moi boss (luu PlayerPrefs).</summary>
    public const int BossAmount = 5;

    /// <summary>Duong dan Resources (khong co duoi) cua prefab nhua roi ra. Khong co prefab thi cong thang vao nguoi choi.</summary>
    public const string PickupResourcePath = "Sap/SapDrop";

    public const string BossKeyPrefix = "SapBoss_";
    public const string PickupKeyPrefix = "SapPickup_";

    public static bool RollEnemyDrop() => Random.value < EnemyDropChance;
}
