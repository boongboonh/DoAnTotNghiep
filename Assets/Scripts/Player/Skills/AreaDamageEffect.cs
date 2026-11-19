using System;
using UnityEngine;

/// <summary>
/// Hieu ung skill: gay sat thuong len moi muc tieu trong mot vung chu nhat truoc mat player.
/// Tam vung tinh theo local cua player nen tu dao theo huong nhin. Dung HitScanner chung voi danh thuong.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Skill Effects/Area Damage", fileName = "AreaDamageEffect")]
public class AreaDamageEffect : SkillEffect
{
    public int damage = 2;
    [Tooltip("Tam vung, local cua player (x huong ve phia truoc).")]
    public Vector2 offset = new Vector2(1.5f, 0.3f);
    public Vector2 size = new Vector2(3f, 2f);
    [Tooltip("Hieu ung tai diem trung (prefab, tu huy sau 1 giay). Co the de trong.")]
    public GameObject hitEffect;
    public string[] targetTags = { "Enemy", "Boss" };
    public LayerMask targetMask = ~0;

    [NonSerialized] private HitScanner scanner;

    public override void Execute(in SkillContext context)
    {
        if (scanner == null) scanner = new HitScanner(targetTags, targetMask);
        scanner.Reset();

        Vector2 center = context.Transform.TransformPoint(offset);
        scanner.Scan(center, size, damage, hitEffect, null);
    }
}
