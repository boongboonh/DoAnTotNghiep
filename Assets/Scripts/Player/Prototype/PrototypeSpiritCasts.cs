using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cac Spirit tan cong theo nang luong (kieu Ori 2), dung chung o Spirit va nang luong cua Spirit Arc:
/// Blaze (qua cau lua nem cong, no va thieu chay), Spike (day gai moc len theo mat dat phia truoc), Flash (chop sang quanh nguoi,
/// gay sat thuong va xoa dan dich). So lieu la uoc luong cho prototype, khong phai bang gia tri goc cua Ori.
/// </summary>
public sealed class PrototypeSpiritCasts : MonoBehaviour
{
    public const float BlazeCost = 1f, SpikeCost = .5f, FlashCost = 1f;
    private const float Cooldown = .6f;

    private PlayerController player;
    private PrototypeSpiritSlots slots;
    private PrototypeSpiritArc arc;
    private Sprite square;
    private float cooldownUntil;

    public int BlazeCount { get; private set; }
    public int SpikeCount { get; private set; }
    public int FlashCount { get; private set; }
    public float CooldownRemaining => Mathf.Max(0f, cooldownUntil - Time.time);

    public void Initialize(PlayerController owner, Sprite shapeSprite, PrototypeSpiritArc energy)
    {
        player = owner; square = shapeSprite; arc = energy;
        slots = player.GetComponent<PrototypeSpiritSlots>();
    }

    public static float Cost(SpiritId id)
    {
        switch (id)
        {
            case SpiritId.Blaze: return BlazeCost;
            case SpiritId.Spike: return SpikeCost;
            case SpiritId.Flash: return FlashCost;
            default: return 0f;
        }
    }

    private bool Free()
    {
        if (player == null || player.IsDead || player.ControlLocked) return false;
        switch (player.Locomotion)
        {
            case PlayerLocomotion.Idle:
            case PlayerLocomotion.Run:
            case PlayerLocomotion.Rise:
            case PlayerLocomotion.Fall:
                return true;
            default: return false;
        }
    }

    private void Update()
    {
        if (player == null || slots == null || arc == null) return;
        if (Time.timeScale <= 0f || Time.time < cooldownUntil || !Free()) return;
        if (slots.Pressed(SpiritId.Blaze)) Cast(SpiritId.Blaze);
        else if (slots.Pressed(SpiritId.Spike)) Cast(SpiritId.Spike);
        else if (slots.Pressed(SpiritId.Flash)) Cast(SpiritId.Flash);
    }

    private void Cast(SpiritId id)
    {
        if (arc.Energy < Cost(id) - .0001f) return;
        if (id == SpiritId.Spike && !player.Motor.Grounded) return;
        if (!arc.TrySpend(Cost(id))) return;
        cooldownUntil = Time.time + Cooldown;
        switch (id)
        {
            case SpiritId.Blaze: Blaze(); break;
            case SpiritId.Spike: Spike(); break;
            case SpiritId.Flash: Flash(); break;
        }
    }

    // ---- Blaze ----
    public void Blaze()
    {
        BlazeCount++;
        float dir = player.Motor.FacingRight ? 1f : -1f;
        Vector2 velocity = new Vector2(dir * 9f, player.Controls.UpHeld ? 11f : 5f);
        var root = new GameObject("Spirit Blaze");
        root.transform.position = player.Motor.Position + new Vector2(dir * .6f, .3f);
        PrototypeGeometry.Rectangle(root.transform, square, "Fire", new Vector2(.4f, .4f), Vector2.zero, new Color(1f, .55f, .15f), 12);
        root.AddComponent<PrototypeFireball>().Initialize(velocity);
    }

    // ---- Spike ----
    public void Spike()
    {
        SpikeCount++;
        float dir = player.Motor.FacingRight ? 1f : -1f;
        var root = new GameObject("Spirit Spike Wave");
        root.AddComponent<PrototypeSpikeWave>().Initialize(square, player.Motor.Position, dir);
    }

    // ---- Flash ----
    public void Flash()
    {
        FlashCount++;
        Vector2 center = player.Motor.Position + Vector2.up * .1f;
        PrototypeSpiritCasts.DealRadius(center, 3.4f, 2, null);
        foreach (var shot in FindObjectsByType<PrototypeProjectile>(FindObjectsSortMode.None))
            if (shot != null && shot.Faction == CombatFaction.Enemy && !shot.Bashed
                && Vector2.Distance(shot.transform.position, center) <= 3.4f) shot.Despawn();
        var ring = PrototypeGeometry.Rectangle(null, square, "Spirit Flash", Vector2.one, center, new Color(1f, 1f, .8f, .6f), 17);
        ring.gameObject.AddComponent<PrototypeFlashVisual>().Initialize(7f);
    }

    /// <summary>Gay sat thuong len moi muc tieu khong cung phe trong ban kinh. Tra ve so muc tieu trung.</summary>
    public static int DealRadius(Vector2 center, float radius, int damage, HashSet<ICombatReceiver> already)
    {
        var buffer = new Collider2D[32];
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(~0);
        int count = Physics2D.OverlapCircle(center, radius, filter, buffer);
        int hits = 0;
        for (int i = 0; i < count; i++)
        {
            var c = buffer[i];
            if (c == null) continue;
            var receiver = c.GetComponentInParent<ICombatReceiver>();
            if (receiver == null || receiver.Faction == CombatFaction.Player) continue;
            if (already != null && already.Contains(receiver)) continue;
            if (receiver.ReceiveHit(damage, center, c.ClosestPoint(center), CombatFaction.Player))
            {
                already?.Add(receiver);
                hits++;
            }
        }
        return hits;
    }
}

/// <summary>Qua cau lua cua Spirit Blaze: bay cong theo trong luc, cham dat / dich thi no va thieu chay muc tieu trong ban kinh.</summary>
public sealed class PrototypeFireball : MonoBehaviour
{
    public const int ExplosionDamage = 2;
    public const float Radius = 1.6f;
    private Vector2 velocity;
    private float age;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[16];
    public bool Exploded { get; private set; }

    public void Initialize(Vector2 initialVelocity) => velocity = initialVelocity;

    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if (Exploded || dt <= 0f) return;
        age += dt;
        if (age >= 2.5f) { Explode(); return; }
        velocity += Vector2.down * 18f * dt;
        Vector2 from = transform.position;
        Vector2 step = velocity * dt;
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(~0);
        int count = Physics2D.CircleCast(from, .2f, step.normalized, filter, hits, step.magnitude);
        for (int i = 0; i < count; i++)
        {
            var c = hits[i].collider;
            if (c == null || c.gameObject == gameObject) continue;
            var receiver = c.GetComponentInParent<ICombatReceiver>();
            bool solidGround = receiver == null && !c.isTrigger && !(c.attachedRigidbody != null && c.attachedRigidbody.CompareTag("Player"));
            bool enemy = receiver != null && receiver.Faction == CombatFaction.Enemy;
            if (!solidGround && !enemy) continue;
            transform.position = from + step.normalized * hits[i].distance;
            Explode();
            return;
        }
        transform.position = from + step;
    }

    private void Explode()
    {
        if (Exploded) return;
        Exploded = true;
        Vector2 center = transform.position;
        var buffer = new Collider2D[32];
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(~0);
        int count = Physics2D.OverlapCircle(center, Radius, filter, buffer);
        var done = new HashSet<ICombatReceiver>();
        for (int i = 0; i < count; i++)
        {
            var c = buffer[i];
            if (c == null) continue;
            var receiver = c.GetComponentInParent<ICombatReceiver>();
            if (receiver == null || receiver.Faction == CombatFaction.Player || !done.Add(receiver)) continue;
            if (!receiver.ReceiveHit(ExplosionDamage, center, c.ClosestPoint(center), CombatFaction.Player)) continue;
            var host = ((Component)receiver).gameObject;
            var burn = host.GetComponent<PrototypeBurn>();
            if (burn == null) burn = host.AddComponent<PrototypeBurn>();
            burn.Ignite(receiver);
        }
        var puff = PrototypeGeometry.Rectangle(null, GetComponentInChildren<SpriteRenderer>().sprite, "Blaze Burst", Vector2.one, center,
            new Color(1f, .5f, .1f, .6f), 17);
        puff.gameObject.AddComponent<PrototypeFlashVisual>().Initialize(Radius * 3f);
        Destroy(gameObject);
    }
}

/// <summary>Thieu chay: 3 nhip, moi nhip 1 giay gay 1 sat thuong; dot lai thi lam moi.</summary>
public sealed class PrototypeBurn : MonoBehaviour
{
    private ICombatReceiver receiver;
    private int ticksLeft;
    private float nextTick;
    public int TicksLeft => ticksLeft;

    public void Ignite(ICombatReceiver target)
    {
        receiver = target;
        ticksLeft = 3;
        nextTick = Time.time + 1f;
        enabled = true;
    }

    private void Update()
    {
        if (receiver == null || ticksLeft <= 0) { enabled = false; return; }
        if (Time.time < nextTick) return;
        nextTick = Time.time + 1f;
        ticksLeft--;
        receiver.ReceiveHit(1, transform.position, transform.position, CombatFaction.Player);
    }
}

/// <summary>Day gai Spirit Spike: 3 gai moc len lan luot truoc mat Player, moi gai gay 2 sat thuong mot lan.</summary>
public sealed class PrototypeSpikeWave : MonoBehaviour
{
    private const int Count = 3;
    private const float Spacing = 1.5f, Delay = .12f, Hold = .35f;
    private readonly SpriteRenderer[] spikes = new SpriteRenderer[Count];
    private readonly float[] x = new float[Count];
    private readonly float[] groundY = new float[Count];
    private readonly bool[] fired = new bool[Count];
    private readonly HashSet<ICombatReceiver> done = new HashSet<ICombatReceiver>();
    private float age;

    public int HitCount => done.Count;

    public void Initialize(Sprite sprite, Vector2 origin, float dir)
    {
        var filter = new ContactFilter2D(); filter.SetLayerMask(1); filter.useTriggers = false;
        var results = new RaycastHit2D[8];
        for (int i = 0; i < Count; i++)
        {
            x[i] = origin.x + dir * (1.2f + i * Spacing);
            groundY[i] = origin.y - .7f;
            int n = Physics2D.Raycast(new Vector2(x[i], origin.y + .6f), Vector2.down, filter, results, 3f);
            for (int k = 0; k < n; k++)
                if (results[k].collider != null && results[k].normal.y > .5f) { groundY[i] = results[k].point.y; break; }
            spikes[i] = PrototypeGeometry.Rectangle(null, sprite, "Spirit Spike " + i, new Vector2(.5f, 1.4f), new Vector2(x[i], groundY[i] + .7f),
                new Color(.85f, .95f, 1f), 14);
            spikes[i].enabled = false;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < Count; i++) if (spikes[i] != null) Destroy(spikes[i].gameObject);
    }

    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if (dt <= 0f) return;
        age += dt;
        for (int i = 0; i < Count; i++)
        {
            float start = i * Delay;
            bool up = age >= start && age < start + Hold;
            spikes[i].enabled = up;
            if (up && !fired[i])
            {
                fired[i] = true;
                PrototypeSpiritCasts.DealRadius(new Vector2(x[i], groundY[i] + .7f), .7f, 2, done);
            }
        }
        if (age >= (Count - 1) * Delay + Hold) Destroy(gameObject);
    }
}

/// <summary>Hieu ung chop: o vuong mo dan va no ra den kich thuoc muc tieu roi tu huy.</summary>
public sealed class PrototypeFlashVisual : MonoBehaviour
{
    private float targetSize = 4f, age;
    private SpriteRenderer shape;
    private const float Life = .22f;

    public void Initialize(float size) { targetSize = size; shape = GetComponent<SpriteRenderer>(); }

    private void Update()
    {
        age += Time.deltaTime;
        float k = Mathf.Clamp01(age / Life);
        transform.localScale = Vector3.one * Mathf.Lerp(.4f, targetSize, k);
        if (shape != null) { var c = shape.color; c.a = (1f - k) * .6f; shape.color = c; }
        if (age >= Life) Destroy(gameObject);
    }
}
