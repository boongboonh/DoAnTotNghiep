using System.Collections.Generic;
using UnityEngine;

/// <summary>Vat pham hoi mau (+1 HP). Chi nhan khi chua day mau; hien lai sau moi lan hoi sinh/reset.</summary>
public sealed class PrototypeHealPickup : MonoBehaviour
{
    private PlayerController player;
    private PrototypePlayerHealth health;
    private SpriteRenderer bar, cross;
    private TextMesh label;
    private bool taken;
    private float messageUntil;

    public bool Taken => taken;

    public void Initialize(Sprite square, PlayerController owner, PrototypePlayerHealth vitality)
    {
        player = owner; health = vitality;
        var green = new Color(.25f, 1f, .4f);
        bar = PrototypeGeometry.Rectangle(transform, square, "Heal Bar", new Vector2(.5f, .16f), Vector2.zero, green, 6);
        cross = PrototypeGeometry.Rectangle(transform, square, "Heal Cross", new Vector2(.16f, .5f), Vector2.zero, green, 6);
        label = PrototypeGeometry.Label(transform, "Heal Label", new Vector2(0f, 1f), .065f);
        ResetPickup();
    }

    public void ResetPickup()
    {
        taken = false;
        messageUntil = 0f;
        Show();
    }

    private void Show()
    {
        if (bar != null) bar.enabled = !taken;
        if (cross != null) cross.enabled = !taken;
        if (label != null)
        {
            bool message = Time.time < messageUntil;
            label.gameObject.SetActive(!taken || message);
            label.text = taken ? "+1 HP" : "HEAL +1 HP";
        }
    }

    private void Update()
    {
        if (messageUntil > 0f && Time.time >= messageUntil) { messageUntil = 0f; Show(); }
        if (taken || player == null || player.IsDead) return;
        // Nhip hoi nhe de nguoi choi thay vat pham.
        float bob = Mathf.Sin(Time.time * 3f) * .08f;
        bar.transform.localPosition = new Vector3(0f, bob, 0f);
        cross.transform.localPosition = new Vector3(0f, bob, 0f);
        Vector2 delta = player.Motor.Position - (Vector2)transform.position;
        // Shard Magnet: bay ve phia Player khi trong 6 don vi (chi khi mau chua day, de khong lang phi).
        if (PrototypeShards.Has(PrototypeShardId.Magnet) && delta.magnitude < 6f && delta.magnitude > .05f && health.Health < health.Maximum)
        {
            transform.position += (Vector3)(delta.normalized * Mathf.Min(7f * Time.deltaTime, delta.magnitude));
            delta = player.Motor.Position - (Vector2)transform.position;
        }
        if (Mathf.Abs(delta.x) > .7f || Mathf.Abs(delta.y) > 1.1f) return;
        if (!health.TryHeal(1))
        {
            // Day mau: giu lai vat pham, nhac nguoi choi.
            if (label != null) { label.gameObject.SetActive(true); label.text = "HP FULL"; }
            return;
        }
        taken = true;
        messageUntil = Time.time + 1.5f;
        Show();
    }
}

/// <summary>Bay gai thoi gian: nghi (an toan) -> bao truoc (nhap nhay vang) -> bung gai (gay sat thuong) -> thu lai.</summary>
public sealed class PrototypeTimedSpike : MonoBehaviour
{
    public enum Phase { Safe, Warning, Active }

    [SerializeField] private float safeTime = 1.6f;
    [SerializeField] private float warningTime = .7f;
    [SerializeField] private float activeTime = .9f;
    private PlayerController player;
    private PrototypePlayerHealth health;
    private SpriteRenderer shape;
    private Vector2 size;
    private float phaseStart;
    private float offset;
    private Collider2D body;

    public Phase Current { get; private set; }

    public void Initialize(Sprite square, PlayerController owner, PrototypePlayerHealth vitality, Vector2 spikeSize, float timeOffset)
    {
        player = owner; health = vitality; size = spikeSize; offset = timeOffset;
        shape = PrototypeGeometry.Rectangle(transform, square, "Timed Spike", size, Vector2.zero, Color.gray, 3);
        body = player.GetComponent<Collider2D>();
        ResetTrap();
    }

    public void ResetTrap()
    {
        // Moi lan reset bat dau lech pha khac nhau theo offset de bay khong dong bo.
        phaseStart = Time.time - offset;
        Current = Phase.Safe;
        Apply();
    }

    private void Apply()
    {
        if (shape == null) return;
        float h = Current == Phase.Active ? size.y : Current == Phase.Warning ? size.y * .35f : size.y * .15f;
        shape.transform.localScale = new Vector3(size.x, h, 1f);
        shape.transform.localPosition = new Vector3(0f, -size.y * .5f + h * .5f, 0f);
        shape.color = Current == Phase.Active ? Color.red : Current == Phase.Warning
            ? (Mathf.Repeat(Time.time, .16f) < .08f ? new Color(1f, .9f, .1f) : new Color(1f, .55f, .1f)) : new Color(.45f, .45f, .5f);
    }

    private void Update()
    {
        float t = Mathf.Repeat(Time.time - phaseStart, safeTime + warningTime + activeTime);
        Current = t < safeTime ? Phase.Safe : t < safeTime + warningTime ? Phase.Warning : Phase.Active;
        Apply();
        if (Current != Phase.Active || player == null || player.IsDead || body == null) return;
        var hit = new Bounds(transform.position, size);
        if (hit.Intersects(body.bounds)) health.TryHit(1, transform.position);
    }
}

/// <summary>Luoi cua di tuan tren mat dat giua hai diem, xoay lien tuc va gay sat thuong khi cham.</summary>
public sealed class PrototypeSawBlade : MonoBehaviour
{
    private PlayerController player;
    private PrototypePlayerHealth health;
    private Collider2D body;
    private Transform visual;
    private Vector2 a, b;
    private float speed;
    private float phase;
    private const float Radius = .45f;

    public void Initialize(Sprite square, PlayerController owner, PrototypePlayerHealth vitality, Vector2 from, Vector2 to, float moveSpeed)
    {
        player = owner; health = vitality; a = from; b = to; speed = moveSpeed;
        body = player.GetComponent<Collider2D>();
        var sprite = PrototypeGeometry.Rectangle(transform, square, "Saw", new Vector2(Radius * 1.5f, Radius * 1.5f), Vector2.zero, new Color(.9f, .3f, .3f), 5);
        var cross = PrototypeGeometry.Rectangle(transform, square, "Saw Cross", new Vector2(Radius * 1.5f, Radius * 1.5f), Vector2.zero, new Color(1f, .75f, .75f), 6);
        visual = sprite.transform;
        cross.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        ResetTrap();
    }

    public void ResetTrap()
    {
        phase = 0f;
        transform.position = a;
    }

    private void Update()
    {
        float length = Mathf.Max(.1f, Vector2.Distance(a, b));
        phase += Time.deltaTime * speed / length;
        transform.position = Vector2.Lerp(a, b, Mathf.PingPong(phase, 1f));
        foreach (Transform child in transform) child.Rotate(0f, 0f, 360f * Time.deltaTime);
        if (player == null || player.IsDead || body == null) return;
        Bounds blade = new Bounds(transform.position, Vector3.one * Radius * 2f);
        if (blade.Intersects(body.bounds)) health.TryHit(1, transform.position);
    }
}

/// <summary>Tao va reset vat pham hoi mau cung bay cua prototype (runtime, khong sua scene).</summary>
public sealed class PrototypeHazards : MonoBehaviour
{
    private readonly List<PrototypeHealPickup> heals = new List<PrototypeHealPickup>();
    private readonly List<PrototypeTimedSpike> spikes = new List<PrototypeTimedSpike>();
    private readonly List<PrototypeSawBlade> saws = new List<PrototypeSawBlade>();
    public IReadOnlyList<PrototypeHealPickup> Heals => heals;
    public IReadOnlyList<PrototypeTimedSpike> Spikes => spikes;
    public IReadOnlyList<PrototypeSawBlade> Saws => saws;

    // Mat san tren cac khu la y ~ -.15.
    private const float FloorTop = -.15f;

    public void Initialize(PlayerController player, PrototypePlayerHealth health, Sprite square)
    {
        AddHeal(square, player, health, new Vector2(-9.5f, FloorTop + .55f));
        AddHeal(square, player, health, new Vector2(30.6f, FloorTop + .55f));
        AddHeal(square, player, health, new Vector2(53.5f, FloorTop + .55f));
        AddHeal(square, player, health, new Vector2(75.2f, FloorTop + .55f));

        AddSpike(square, player, health, new Vector2(7f, FloorTop + .25f), new Vector2(1.3f, .5f), 0f,
            "TIMED SPIKES\nWait for them to retract");
        AddSpike(square, player, health, new Vector2(61f, FloorTop + .25f), new Vector2(1.4f, .5f), .8f, null);
        AddSaw(square, player, health, new Vector2(27.2f, FloorTop + .45f), new Vector2(29.6f, FloorTop + .45f), 2.4f);
    }

    private void AddHeal(Sprite square, PlayerController player, PrototypePlayerHealth health, Vector2 position)
    {
        var g = new GameObject("Heal Pickup");
        g.transform.SetParent(transform, false);
        g.transform.position = position;
        var heal = g.AddComponent<PrototypeHealPickup>();
        heal.Initialize(square, player, health);
        heals.Add(heal);
    }

    private void AddSpike(Sprite square, PlayerController player, PrototypePlayerHealth health, Vector2 position,
        Vector2 size, float offset, string hint)
    {
        var g = new GameObject("Timed Spike Trap");
        g.transform.SetParent(transform, false);
        g.transform.position = position;
        var spike = g.AddComponent<PrototypeTimedSpike>();
        spike.Initialize(square, player, health, size, offset);
        spikes.Add(spike);
        if (!string.IsNullOrEmpty(hint))
        {
            var label = PrototypeGeometry.Label(transform, "Timed Spike Hint", position + Vector2.up * 1.1f, .07f);
            label.text = hint;
        }
    }

    private void AddSaw(Sprite square, PlayerController player, PrototypePlayerHealth health, Vector2 from, Vector2 to, float speed)
    {
        var g = new GameObject("Saw Blade Trap");
        g.transform.SetParent(transform, false);
        var saw = g.AddComponent<PrototypeSawBlade>();
        saw.Initialize(square, player, health, from, to, speed);
        saws.Add(saw);
        var label = PrototypeGeometry.Label(transform, "Saw Hint", (from + to) * .5f + Vector2.up * 1.3f, .07f);
        label.text = "SAW BLADE\nJump over it";
    }

    public void ResetAll()
    {
        foreach (var h in heals) if (h != null) h.ResetPickup();
        foreach (var s in spikes) if (s != null) s.ResetTrap();
        foreach (var s in saws) if (s != null) s.ResetTrap();
    }
}
