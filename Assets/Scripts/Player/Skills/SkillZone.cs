using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vung skill ton tai mot luc (vung anh nhin, song am...). Tao boi <see cref="SpawnZoneEffect"/>, tu huy sau
/// <see cref="Duration"/>. Moi <see cref="tickInterval"/> giay: (tuy chon) gay sat thuong len muc tieu trong vung va
/// bao cho moi <see cref="IWorldReactive"/> trong vung. Trang thai nam o object nay, khong nam tren asset effect.
/// </summary>
public class SkillZone : MonoBehaviour
{
    private const int MaxResults = 32;

    private SkillData source;
    private Vector2 size;
    private float duration;
    private float tickInterval;
    private int tickDamage;
    private HitScanner scanner;
    private readonly Collider2D[] buffer = new Collider2D[MaxResults];
    private readonly HashSet<IWorldReactive> notified = new HashSet<IWorldReactive>();
    private ContactFilter2D reactiveFilter;
    private float age;
    private float nextTick;

    public float Duration => duration;
    public float TimeLeft => Mathf.Max(0f, duration - age);
    public SkillData Source => source;

    public void Init(SkillData skill, Vector2 zoneSize, float lifetime, float interval, int damage, string[] targetTags, LayerMask mask)
    {
        source = skill;
        size = zoneSize;
        duration = Mathf.Max(0.05f, lifetime);
        tickInterval = Mathf.Max(0.05f, interval);
        tickDamage = damage;
        scanner = damage > 0 ? new HitScanner(targetTags, mask) : null;
        reactiveFilter = new ContactFilter2D { useLayerMask = true, layerMask = mask, useTriggers = true };
        age = 0f;
        nextTick = 0f;
    }

    private void Update()
    {
        age += Time.deltaTime;
        if (age >= nextTick)
        {
            nextTick += tickInterval;
            Tick();
        }
        if (age >= duration) Destroy(gameObject);
    }

    private void Tick()
    {
        Vector2 center = transform.position;

        // Moi nhip danh lai duoc muc tieu cu (sat thuong theo thoi gian).
        if (scanner != null)
        {
            scanner.Reset();
            scanner.Scan(center, size, tickDamage, null, null);
        }

        notified.Clear();
        int count = Physics2D.OverlapBox(center, size, 0f, reactiveFilter, buffer);
        for (int i = 0; i < count; i++)
        {
            if (buffer[i] == null) continue;
            var reactive = buffer[i].GetComponentInParent<IWorldReactive>();
            if (reactive != null && notified.Add(reactive)) reactive.OnSkillZone(source, this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.4f);
        Gizmos.DrawWireCube(transform.position, size);
    }
}
