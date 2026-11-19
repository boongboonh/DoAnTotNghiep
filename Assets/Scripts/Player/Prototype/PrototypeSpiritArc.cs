using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Spirit Arc (kieu Ori 2): giu nut ngam (chuot / can), tha de ban mot mui ten nang luong, ton nang luong.
/// Nang luong hoi khi Spirit Edge (don can chien) trung dich. Ngam khi dang ranh; bi thuong/dash/danh/Bash thi huy.
/// </summary>
public sealed class PrototypeSpiritArc : MonoBehaviour
{
    public const float MaxEnergy = 3f;
    public const float ShotCost = .25f;
    public const float HitGain = .25f;
    private const float Cooldown = .22f;

    private PlayerController player;
    private PrototypeSpiritSlots slots;
    private PlayerCombat combat;
    private Camera aimCamera;
    private Sprite square;
    private SpriteRenderer line;
    private SpriteRenderer tip;
    private float cooldownUntil;
    private Vector2 aim = Vector2.right;
    private Vector2 lastMouse;
    private bool mouseAim = true;

    public float Energy { get; private set; } = MaxEnergy;
    public bool Aiming { get; private set; }
    public Vector2 Direction => aim;
    public int ShotCount { get; private set; }
    public bool CanAfford => Energy >= ShotCost - .0001f;

    public void Initialize(PlayerController owner, Camera camera, Sprite shapeSprite)
    {
        player = owner; aimCamera = camera; square = shapeSprite;
        combat = player.GetComponent<PlayerCombat>();
        if (combat != null) combat.HitLanded += OnMeleeHit;
        slots = player.GetComponent<PrototypeSpiritSlots>();
        if (slots != null) slots.Changed += Cancel;
        line = PrototypeGeometry.Rectangle(null, square, "Arc Aim Line", new Vector2(2.2f, .06f), Vector2.zero,
            new Color(1f, .85f, .4f, .9f), 18);
        tip = PrototypeGeometry.Rectangle(null, square, "Arc Aim Tip", new Vector2(.26f, .26f), Vector2.zero,
            new Color(1f, .85f, .4f), 19);
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (combat != null) combat.HitLanded -= OnMeleeHit;
        if (slots != null) slots.Changed -= Cancel;
        if (line != null) Destroy(line.gameObject);
        if (tip != null) Destroy(tip.gameObject);
    }

    private void OnMeleeHit(AttackStep step, Collider2D target) => AddEnergy(HitGain);

    /// <summary>So o nang luong toi da hien tai (shard Energy them 1 o).</summary>
    public static float Capacity => MaxEnergy + (PrototypeShards.Has(PrototypeShardId.Energy) ? 1f : 0f);

    public void AddEnergy(float amount) => Energy = Mathf.Clamp(Energy + amount, 0f, Capacity);

    /// <summary>Tru nang luong cho Spirit khac (vd Sentry). Tra ve false neu khong du.</summary>
    public bool TrySpend(float amount)
    {
        if (Energy < amount - .0001f) return false;
        Energy = Mathf.Max(0f, Energy - amount);
        return true;
    }

    public void ResetEnergy()
    {
        Energy = Capacity;
        Cancel();
    }

    public void Cancel()
    {
        Aiming = false;
        SetVisible(false);
    }

    private void SetVisible(bool on)
    {
        if (line != null) line.enabled = on;
        if (tip != null) tip.enabled = on;
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
            case PlayerLocomotion.WallSlide:
            case PlayerLocomotion.WallClimb:
                return true;
            default: return false;
        }
    }

    private void Update()
    {
        if (player == null) return;
        if (Energy > Capacity) Energy = Capacity;
        bool held = slots != null && slots.Held(SpiritId.Arc);
        if (!Aiming)
        {
            if (held && slots.Pressed(SpiritId.Arc) &&Free() && CanAfford && Time.time >= cooldownUntil && Time.timeScale > 0f)
            {
                Aiming = true;
                aim = player.Motor.FacingRight ? Vector2.right : Vector2.left;
                mouseAim = true;
                if (Mouse.current != null) lastMouse = Mouse.current.position.ReadValue();
            }
            return;
        }
        // Huy khi het ranh (dash, bi thuong, danh, Bash...) hoac mat input.
        if (!Free() || !player.Controls.InputActive) { Cancel(); return; }
        aim = ReadAim(aim);
        if (Mathf.Abs(aim.x) > .25f) player.Motor.Face(aim.x > 0f ? 1 : -1);
        if (held) { Draw(); return; }
        Fire();
    }

    private Vector2 ReadAim(Vector2 last)
    {
        Vector2 stick = player.Controls.AimStick;
        if (stick.sqrMagnitude > .01f)
        {
            mouseAim = false;
            if (Mouse.current != null) lastMouse = Mouse.current.position.ReadValue();
            return stick;
        }
        if (Mouse.current == null || aimCamera == null) return last;
        Vector2 now = Mouse.current.position.ReadValue();
        if ((now - lastMouse).sqrMagnitude > 16f) mouseAim = true;
        lastMouse = now;
        if (!mouseAim) return last;
        Vector3 world = aimCamera.ScreenToWorldPoint(new Vector3(now.x, now.y, Mathf.Abs(aimCamera.transform.position.z)));
        Vector2 delta = (Vector2)world - player.Motor.Position;
        return delta.sqrMagnitude > .09f ? delta.normalized : last;
    }

    private void Draw()
    {
        Vector2 origin = player.Motor.Position + Vector2.up * .15f;
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        SetVisible(true);
        line.transform.SetPositionAndRotation(origin + aim * 1.8f, Quaternion.Euler(0f, 0f, angle));
        tip.transform.SetPositionAndRotation(origin + aim * 3f, Quaternion.Euler(0f, 0f, 45f));
    }

    private void Fire()
    {
        Aiming = false;
        SetVisible(false);
        if (!CanAfford) return;
        Energy = Mathf.Max(0f, Energy - ShotCost);
        cooldownUntil = Time.time + Cooldown;
        ShotCount++;
        SpawnShot(aim, 2 + PrototypeShards.EnergyDamageBonus, 1f);
        // Shard Splinter: hai mui phu toa ra hai ben, sat thuong 1.
        if (PrototypeShards.Has(PrototypeShardId.Splinter))
        {
            SpawnShot(Quaternion.Euler(0f, 0f, 12f) * aim, 1, .7f);
            SpawnShot(Quaternion.Euler(0f, 0f, -12f) * aim, 1, .7f);
        }
    }

    private void SpawnShot(Vector2 heading, int damage, float scale)
    {
        var root = new GameObject("Spirit Arc Shot");
        root.transform.position = player.Motor.Position + Vector2.up * .15f + heading * .7f;
        PrototypeGeometry.Rectangle(root.transform, square, "Shot", new Vector2(.55f, .12f) * scale, Vector2.zero, new Color(1f, .9f, .5f), 12);
        root.AddComponent<PrototypeArcShot>().Initialize(heading, damage);
    }
}

/// <summary>Mui ten cua Spirit Arc: bay thang, trung dich (khong cung phe) thi gay sat thuong, cham dat thi tan.</summary>
public sealed class PrototypeArcShot : MonoBehaviour
{
    private const float Speed = 18f, Life = 1.1f, Radius = .12f;
    private int Damage = 2;
    private Vector2 direction;
    private float age;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[32];

    public void Initialize(Vector2 heading, int damage = 2)
    {
        Damage = damage;
        direction = heading.sqrMagnitude > .001f ? heading.normalized : Vector2.right;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if (dt <= 0f) return;
        age += dt;
        if (age >= Life) { Destroy(gameObject); return; }
        Vector2 from = transform.position;
        float distance = Speed * dt;
        var filter = new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers = true;
        int count = Physics2D.CircleCast(from, Radius, direction, filter, hits, distance);
        float best = float.PositiveInfinity;
        Collider2D target = null;
        ICombatReceiver receiver = null;
        for (int i = 0; i < count; i++)
        {
            var c = hits[i].collider;
            if (c == null || c.gameObject == gameObject) continue;
            var candidate = c.GetComponentInParent<ICombatReceiver>();
            if (candidate != null && candidate.Faction == CombatFaction.Player) continue;
            if (candidate == null && (c.isTrigger || c.attachedRigidbody != null && c.attachedRigidbody.CompareTag("Player"))) continue;
            if (hits[i].distance >= best) continue;
            best = hits[i].distance; target = c; receiver = candidate;
        }
        if (target != null)
        {
            transform.position = from + direction * best;
            receiver?.ReceiveHit(Damage, from - direction * .2f, target.ClosestPoint(transform.position), CombatFaction.Player);
            Destroy(gameObject);
        }
        else transform.position = from + direction * distance;
    }
}
