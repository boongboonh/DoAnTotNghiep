using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Mot don trong chuoi combo. Moi moc thoi gian tinh bang giay ke tu luc bat dau don.</summary>
[Serializable]
public class AttackStep
{
    public string name = "Slash";
    [Tooltip("Ten pose trong PoseSet de PosePlayer phu len trong luc danh.")]
    public string poseName = "Attack1";

    [Header("Thoi gian")]
    [Tooltip("Tong thoi gian cua don (khong chain thi het gio la ket thuc).")]
    public float duration = 0.35f;
    [Tooltip("Tu moc nay tro di, bam danh se xep hang don ke tiep.")]
    public float chainFrom = 0.15f;
    public float hitStart = 0.08f;
    public float hitEnd = 0.2f;

    [Header("Sat thuong")]
    public int damage = 1;
    [Tooltip("Tam hitbox, local cua player (x huong ve phia truoc, tu dao theo huong nhin).")]
    public Vector2 hitOffset = new Vector2(1f, 0.2f);
    public Vector2 hitSize = new Vector2(1.6f, 1.2f);
    [Tooltip("Hieu ung tai diem trung don (prefab, tu huy sau 1 giay). Co the de trong.")]
    public GameObject hitEffect;

    [Header("Di chuyen khi danh")]
    [Tooltip("Lao ve phia truoc (don vi/giay) trong lungeTime dau.")]
    public float lungeSpeed = 3f;
    public float lungeTime = 0.1f;
    [Tooltip("Phan tram dieu khien ngang con lai sau khi het lunge (0 = dung yen, 1 = di chuyen binh thuong).")]
    [Range(0f, 1f)] public float moveControl = 0f;
}

/// <summary>
/// Chuoi combo cua player, luu thanh asset: them / bot don, chinh hitbox va nhip danh khong can sua code.
/// Mac dinh co 3 don.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Attack Combo", fileName = "AttackCombo")]
public class AttackComboData : ScriptableObject
{
    [Tooltip("Sau khi ket thuc don, qua khoang nay (giay) ma khong danh tiep thi combo ve don dau.")]
    public float comboResetTime = 0.6f;
    [Tooltip("Cho phep bat dau don khi dang tren khong.")]
    public bool allowAirborne = true;

    [SerializeField]
    private List<AttackStep> steps = new List<AttackStep>
    {
        new AttackStep { name = "Slash1", poseName = "Attack1", duration = 0.35f, chainFrom = 0.15f, hitStart = 0.08f, hitEnd = 0.2f, damage = 1, lungeSpeed = 3f },
        new AttackStep { name = "Slash2", poseName = "Attack2", duration = 0.35f, chainFrom = 0.15f, hitStart = 0.08f, hitEnd = 0.2f, damage = 1, lungeSpeed = 3f },
        new AttackStep { name = "Slash3", poseName = "Attack3", duration = 0.5f,  chainFrom = 0.2f,  hitStart = 0.12f, hitEnd = 0.28f, damage = 2, lungeSpeed = 5f, hitSize = new Vector2(2f, 1.4f) }
    };

    public int StepCount => steps.Count;
    public AttackStep GetStep(int index) => steps[index];
}
