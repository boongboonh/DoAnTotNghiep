using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quet vung hinh chu nhat va gay sat thuong len cac <see cref="IDamageable"/> trong vung.
/// Dung chung cho danh thuong va skill: moi muc tieu chi trung 1 lan giua hai lan <see cref="Reset"/>
/// (vd moi don danh, moi lan niem skill). Khong cap phat bo nho khi quet.
/// </summary>
public sealed class HitScanner
{
    private const int MaxHits = 16;

    private readonly Collider2D[] buffer = new Collider2D[MaxHits];
    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
    private readonly string[] tags;
    private ContactFilter2D filter;

    public HitScanner(string[] targetTags, LayerMask mask)
    {
        tags = targetTags ?? new string[0];
        filter = new ContactFilter2D { useLayerMask = true, layerMask = mask, useTriggers = true };
    }

    /// <summary>Xoa danh sach da trung de bat dau mot dot danh moi.</summary>
    public void Reset() => alreadyHit.Clear();

    /// <summary>Quet va gay sat thuong. Tra ve so muc tieu moi bi trung trong lan quet nay.</summary>
    /// <param name="effect">Hieu ung tai diem trung (tu huy sau 1 giay). Co the null.</param>
    /// <param name="onHit">Goi moi muc tieu trung, truyen collider. Co the null.</param>
    public int Scan(Vector2 center, Vector2 size, int damage, GameObject effect, Action<Collider2D> onHit,
        Vector2? source = null, Rigidbody2D owner = null, bool blockWalls = false)
    {
        int count = Physics2D.OverlapBox(center, size, 0f, filter, buffer);
        int newHits = 0;

        for (int i = 0; i < count; i++)
        {
            Collider2D c = buffer[i];
            if (c == null || (owner != null && c.attachedRigidbody == owner) || !HasTag(c)) continue;

            IDamageable target = c.GetComponentInParent<IDamageable>();
            if (target == null || alreadyHit.Contains(target)) continue;
            Vector2 origin = source ?? center;
            if (blockWalls && CombatOcclusion.Blocked(origin, c.bounds.center, owner, c.attachedRigidbody)) continue;

            Vector2 hitPoint = c.ClosestPoint(center);
            if (target is ICombatReceiver receiver)
            {
                if (!receiver.ReceiveHit(damage, origin, hitPoint, CombatFaction.Player)) continue;
            }
            else target.TakeDamage(damage, hitPoint);
            alreadyHit.Add(target);

            if (effect != null)
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(effect, hitPoint, Quaternion.identity), 1f);

            onHit?.Invoke(c);
            newHits++;
        }
        return newHits;
    }

    private bool HasTag(Collider2D c)
    {
        for (Transform t = c.transform; t != null; t = t.parent)
            for (int i = 0; i < tags.Length; i++)
                if (t.CompareTag(tags[i])) return true;
        return false;
    }
}
