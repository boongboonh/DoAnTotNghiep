using UnityEngine;

/// <summary>
/// Spirit Sentry (kieu Ori 2): dat mot thap tu dong xuong dat, ton 1 o nang luong. Thap tu ban dich gan nhat trong tam
/// (co dua tia kiem tra bi chan tuong) trong 10 giay roi bien mat. Chi co 1 thap cung luc; dat thap moi thi thap cu mat.
/// </summary>
public sealed class PrototypeSpiritSentry : MonoBehaviour
{
    public const float EnergyCost = 1f;
    public const float Lifetime = 10f;
    private const float Cooldown = .8f;

    private PlayerController player;
    private PrototypeSpiritSlots slots;
    private PrototypeSpiritArc arc;
    private Sprite square;
    private float cooldownUntil;

    public PrototypeSentryTurret Turret { get; private set; }
    public bool TurretActive => Turret != null;
    public float TurretRemaining => Turret != null ? Turret.Remaining : 0f;
    public bool CanAfford => arc != null && arc.Energy >= EnergyCost - .0001f;
    public int PlacedCount { get; private set; }

    public void Initialize(PlayerController owner, Sprite shapeSprite, PrototypeSpiritArc energy)
    {
        player = owner; square = shapeSprite; arc = energy;
        slots = player.GetComponent<PrototypeSpiritSlots>();
    }

    private void OnDestroy() => Clear();

    public void Clear()
    {
        if (Turret != null) Destroy(Turret.gameObject);
        Turret = null;
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
        if (!slots.Pressed(SpiritId.Sentry) || !Free() || Time.timeScale <= 0f || Time.time < cooldownUntil) return;
        if (!CanAfford) return;
        Vector2 spot;
        if (!FindGround(out spot)) return;
        arc.TrySpend(EnergyCost);
        cooldownUntil = Time.time + Cooldown;
        Place(spot);
    }

    /// <summary>Tim mat dat ngay duoi chan (hoac truoc mat mot chut) de dat thap.</summary>
    private bool FindGround(out Vector2 spot)
    {
        spot = default;
        float dir = player.Motor.FacingRight ? 1f : -1f;
        Vector2 from = player.Motor.Position + new Vector2(dir * .8f, .4f);
        var filter = new ContactFilter2D(); filter.SetLayerMask(1); filter.useTriggers = false;
        var results = new RaycastHit2D[8];
        int count = Physics2D.Raycast(from, Vector2.down, filter, results, 4f);
        for (int i = 0; i < count; i++)
        {
            var c = results[i].collider;
            if (c == null || c.attachedRigidbody != null && c.attachedRigidbody.CompareTag("Player")) continue;
            if (results[i].normal.y < .5f) continue;
            spot = results[i].point + Vector2.up * .3f;
            return true;
        }
        return false;
    }

    private void Place(Vector2 spot)
    {
        Clear();
        PlacedCount++;
        var root = new GameObject("Spirit Sentry");
        root.transform.position = spot;
        Turret = root.AddComponent<PrototypeSentryTurret>();
        Turret.Initialize(square, Lifetime);
    }
}

/// <summary>Than thap: quet dich, ban mui ten nang luong, het gio thi tu huy.</summary>
public sealed class PrototypeSentryTurret : MonoBehaviour
{
    private const float Range = 7f, FireInterval = .7f;
    private const int Damage = 1;

    private Sprite square;
    private SpriteRenderer barrel;
    private SpriteRenderer body;
    private float life, age, nextFire;
    private Vector2 aim = Vector2.right;
    private readonly Collider2D[] buffer = new Collider2D[32];

    public float Remaining => Mathf.Max(0f, life - age);
    public int ShotCount { get; private set; }

    public void Initialize(Sprite shapeSprite, float lifetime)
    {
        square = shapeSprite; life = lifetime; age = 0f; nextFire = .35f;
        body = PrototypeGeometry.Rectangle(transform, square, "Sentry Body", new Vector2(.5f, .5f), Vector2.zero, new Color(.4f, .9f, .6f), 14);
        barrel = PrototypeGeometry.Rectangle(transform, square, "Sentry Barrel", new Vector2(.6f, .12f), Vector2.zero, new Color(.9f, 1f, .8f), 15);
    }

    private void Update()
    {
        age += Time.deltaTime;
        if (age >= life) { Destroy(gameObject); return; }
        // Nhap nhay khi sap het gio.
        if (body != null)
        {
            float left = life - age;
            bool blink = left < 2f && Mathf.FloorToInt(left * 6f) % 2 == 0;
            body.color = blink ? new Color(.4f, .9f, .6f, .35f) : new Color(.4f, .9f, .6f);
        }
        if (Time.timeScale <= 0f) return;

        Vector2 origin = transform.position;
        Collider2D target = FindTarget(origin);
        if (target == null)
        {
            Aim(aim);
            return;
        }
        Vector2 center = target.bounds.center;
        Vector2 dir = (center - origin).normalized;
        Aim(dir);
        if (age < nextFire) return;
        nextFire = age + FireInterval;
        ShotCount++;
        var shot = new GameObject("Sentry Shot");
        shot.transform.position = origin + dir * .5f;
        PrototypeGeometry.Rectangle(shot.transform, square, "Shot", new Vector2(.4f, .1f), Vector2.zero, new Color(.7f, 1f, .75f), 12);
        shot.AddComponent<PrototypeArcShot>().Initialize(dir, Damage);
    }

    private void Aim(Vector2 dir)
    {
        if (dir.sqrMagnitude < .01f) return;
        aim = dir;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        barrel.transform.SetPositionAndRotation((Vector2)transform.position + dir * .35f, Quaternion.Euler(0f, 0f, angle));
    }

    private Collider2D FindTarget(Vector2 origin)
    {
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(~0);
        int count = Physics2D.OverlapCircle(origin, Range, filter, buffer);
        Collider2D best = null;
        float bestDistance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            var c = buffer[i];
            if (c == null || !c.enabled) continue;
            var receiver = c.GetComponentInParent<ICombatReceiver>();
            if (receiver == null || receiver.Faction != CombatFaction.Enemy) continue;
            // Bo qua cac muc tieu chua hoat dong (vd boss dang ngu).
            var boss = c.GetComponentInParent<PrototypeBoss>();
            if (boss != null && !boss.Active) continue;
            float d = ((Vector2)c.bounds.center - origin).sqrMagnitude;
            if (d >= bestDistance) continue;
            if (CombatOcclusion.Blocked(origin, c.bounds.center, null, c.attachedRigidbody)) continue;
            bestDistance = d; best = c;
        }
        return best;
    }
}
