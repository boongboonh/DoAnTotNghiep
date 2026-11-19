using UnityEngine;

/// <summary>
/// Mot vu khi cua player, luu thanh asset. Them vu khi moi = tao asset moi (Create > Dang Dang > Weapon),
/// khong phai viet them script. Combo (hitbox, nhip danh, pose) nam trong <see cref="AttackComboData"/>;
/// WeaponData gom combo voi phan nhan dang va hinh dang cam tren tay.
/// Ky nang dac biet de them sau, goi qua phim ky nang cua PlayerInputReader.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Weapon", fileName = "Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Nhan dang")]
    public string displayName = "Weapon";
    public Sprite icon;
    [TextArea] public string description;

    [Header("Danh can chien")]
    public AttackComboData combo;
    [Tooltip("Cong them vao sat thuong cua moi don trong combo.")]
    [Min(0)] public int bonusDamage = 0;
    [Tooltip("Hieu ung trung don mac dinh, dung khi don trong combo khong co hieu ung rieng.")]
    public GameObject defaultHitEffect;

    [Header("Hinh dang")]
    [Tooltip("Prefab cam tren tay (gan vao weaponSocket cua PlayerCombat khi trang bi). Co the de trong.")]
    public GameObject heldVisual;
}
