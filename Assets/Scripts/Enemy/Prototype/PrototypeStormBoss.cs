using UnityEngine;

/// <summary>
/// Boss thu 2 "Stormwing" (graybox, khu vuc rieng): boss bay. Lo lung tren cao, bao hieu roi lao xuong cho Player
/// (Swoop) hoac goi mua dan roi tu tren xuong (Rain). Sau Swoop no ha xuong dat va nghi - day la luc danh can chien.
/// Tren cao chi co Spirit Arc / Sentry, nhay doi + danh len hoac dan phan lai bang Bash moi voi toi.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PrototypeStormBoss : MonoBehaviour, ICombatReceiver, IBashTarget
{
    private void OnEnable() => PrototypeBash.Register(this);
    private void OnDisable() => PrototypeBash.Unregister(this);
    public Vector2 BashPosition => body != null ? body.position : (Vector2)transform.position;
    public bool CanBash(Vector2 playerPosition, float range) =>
        health != null && Active && Vector2.Distance(playerPosition, BashPosition) <= range;
    public bool Bash(Vector2 launchDirection)
    {
        if (!Active) return false;
        ReceiveHit(HeavyHit, BashPosition + launchDirection.normalized, BashPosition, CombatFaction.Player);
        return true;
    }

    public enum Mode { Dormant, Hover, Windup, Attack, Recovery, Stagger, Dead }
    public enum Attack { None, Swoop, Rain }

    public const int MaxHealth = 30;
    private const int HeavyHit = 3;
    private const float HoverY = 5f, GroundY = .45f, RainSpawnY = 7f;

    private PlayerController target;
    private PrototypePlayerHealth vitality;
    private Sprite sprite;
    private Rigidbody2D body;
    private BoxCollider2D hurtbox;
    private PrototypeHealthState health;
    private SpriteRenderer shape;
    private SpriteRenderer telegraph;
    private TextMesh label;
    private Vector2 home;
    private float triggerX, minX, maxX;
    private float timer, flashUntil;
    private bool struck;
    private int cycle;
    private Vector2 swoopDir = Vector2.down;
    private Vector2 swoopTarget;
    private float rainCenter;
    private readonly Attack[] cyclePhase1 = { Attack.Swoop, Attack.Rain, Attack.Swoop };
    private readonly Attack[] cyclePhase2 = { Attack.Swoop, Attack.Rain, Attack.Swoop, Attack.Swoop, Attack.Rain };

    public Mode Current { get; private set; }
    public Attack CurrentAttack { get; private set; }
    public int Phase { get; private set; } = 1;
    public int Health => health != null ? health.Current : 0;
    public bool Active => Current != Mode.Dormant && Current != Mode.Dead;
    public bool Defeated => Current == Mode.Dead;
    public CombatFaction Faction => CombatFaction.Enemy;
    public Vector2 Position => body != null ? body.position : (Vector2)transform.position;
    public event System.Action Died;
    public event System.Action Engaged;

    public PrototypeEnemyState State => Current == Mode.Windup ? PrototypeEnemyState.Windup
        : Current == Mode.Attack ? PrototypeEnemyState.Attack
        : Current == Mode.Dead ? PrototypeEnemyState.Dead
        : Current == Mode.Dormant ? PrototypeEnemyState.Idle
        : Current == Mode.Hover ? PrototypeEnemyState.Chase : PrototypeEnemyState.Recovery;

    private bool PlayerAlive => !target.IsDead && !vitality.IsDead;
    private float Scale => Phase == 2 ? .8f : 1f;

    public void Initialize(PlayerController player, PrototypePlayerHealth hp, Sprite square, float triggerLine,
        float arenaMinX, float arenaMaxX, Vector2 homePosition)
    {
        target = player; vitality = hp; sprite = square; triggerX = triggerLine; minX = arenaMinX; maxX = arenaMaxX;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        hurtbox = GetComponent<BoxCollider2D>();
        hurtbox.size = new Vector2(2.2f, 1.2f);
        hurtbox.isTrigger = true;
        home = homePosition;
        health = new PrototypeHealthState(MaxHealth);
        shape = PrototypeGeometry.Rectangle(transform, square, "Stormwing Body", hurtbox.size, Vector2.zero, new Color(.2f, .55f, .65f));
        PrototypeGeometry.Rectangle(transform, square, "Stormwing Wing L", new Vector2(1.5f, .25f), new Vector2(-1.5f, .35f), new Color(.3f, .75f, .85f), 5);
        PrototypeGeometry.Rectangle(transform, square, "Stormwing Wing R", new Vector2(1.5f, .25f), new Vector2(1.5f, .35f), new Color(.3f, .75f, .85f), 5);
        PrototypeGeometry.Rectangle(transform, square, "Stormwing Eye", new Vector2(.4f, .2f), new Vector2(0f, .15f), Color.yellow, 6);
        telegraph = PrototypeGeometry.Rectangle(null, square, "Stormwing Telegraph", Vector2.one, Vector2.zero, Color.yellow, 4);
        label = PrototypeGeometry.Label(transform, "Stormwing Status", new Vector2(0f, 1.4f), .08f);
        ResetBoss();
    }

    private void OnDestroy()
    {
        if (telegraph != null) Destroy(telegraph.gameObject);
    }

    private void Set(Mode mode, float time = 0f)
    {
        Current = mode;
        timer = time;
        struck = false;
        Refresh();
    }

    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if (health == null || dt <= 0f || Current == Mode.Dead) return;
        if (Current == Mode.Dormant)
        {
            if (PlayerAlive && target.Motor.Position.x > triggerX)
            {
                Set(Mode.Hover, 1.5f);
                Engaged?.Invoke();
            }
            return;
        }
        if (!PlayerAlive) { if (Current != Mode.Hover) Set(Mode.Hover, 1f); return; }
        timer -= dt;
        switch (Current)
        {
            case Mode.Hover:
                if (timer <= 0f) BeginAttack();
                break;
            case Mode.Windup:
                // Swoop: khoa muc tieu o 0.3s cuoi de Player kip ne. Rain: dung tam rai.
                if (CurrentAttack == Attack.Swoop && timer > .3f * Scale) swoopTarget = target.Motor.Position;
                if (timer <= 0f) Strike();
                break;
            case Mode.Attack:
                if (CurrentAttack == Attack.Swoop) SwoopHit();
                if (timer <= 0f) Set(Mode.Recovery, (CurrentAttack == Attack.Swoop ? 1.7f : .9f) * Scale);
                break;
            case Mode.Recovery:
            case Mode.Stagger:
                if (timer <= 0f) Set(Mode.Hover, Phase == 2 ? .7f : 1.2f);
                break;
        }
        Refresh();
    }

    private void BeginAttack()
    {
        var list = Phase == 2 ? cyclePhase2 : cyclePhase1;
        CurrentAttack = list[cycle++ % list.Length];
        swoopTarget = target.Motor.Position;
        rainCenter = Mathf.Clamp(target.Motor.Position.x, minX + 3f, maxX - 3f);
        float windup = CurrentAttack == Attack.Swoop ? 1.0f : 1.1f;
        Set(Mode.Windup, windup * Scale);
    }

    private void Strike()
    {
        if (CurrentAttack == Attack.Swoop)
        {
            Vector2 dir = swoopTarget - Position;
            if (dir.y > -1f) dir.y = -1f;
            swoopDir = dir.normalized;
            Set(Mode.Attack, 1.6f);
            return;
        }
        // Rain: dan roi thang xuong quanh tam da bao hieu.
        int count = Phase == 2 ? 6 : 4;
        float step = Phase == 2 ? 2.2f : 2.8f;
        for (int i = 0; i < count; i++)
        {
            float x = Mathf.Clamp(rainCenter + (i - (count - 1) * .5f) * step, minX, maxX);
            var root = new GameObject("Storm Drop");
            root.transform.position = new Vector2(x, RainSpawnY);
            PrototypeGeometry.Rectangle(root.transform, sprite, "Shot", new Vector2(.3f, .45f), Vector2.zero, new Color(.4f, .9f, 1f), 6);
            root.AddComponent<PrototypeProjectile>().Initialize(Vector2.down, body, null);
        }
        Set(Mode.Attack, .2f);
    }

    private void SwoopHit()
    {
        if (struck || !hurtbox.bounds.Intersects(target.GetComponent<Collider2D>().bounds)) return;
        struck = true;
        vitality.TryHit(1, Position);
    }

    private void FixedUpdate()
    {
        if (health == null || Current == Mode.Dead || Current == Mode.Dormant || !PlayerAlive) return;
        float dt = Time.fixedDeltaTime;
        Vector2 pos = Position;
        switch (Current)
        {
            case Mode.Hover:
                {
                    float speed = Phase == 2 ? 6f : 4.2f;
                    float goalX = Mathf.Clamp(target.Motor.Position.x, minX, maxX);
                    pos.x = Mathf.MoveTowards(pos.x, goalX, speed * dt);
                    pos.y = Mathf.MoveTowards(pos.y, HoverY + Mathf.Sin(Time.time * 2f) * .3f, 7f * dt);
                    break;
                }
            case Mode.Windup:
                // Rain: ve tam roi; Swoop: nhun len nhe de bao hieu.
                if (CurrentAttack == Attack.Rain) pos.x = Mathf.MoveTowards(pos.x, rainCenter, 5f * dt);
                pos.y = Mathf.MoveTowards(pos.y, HoverY + 1f, 3f * dt);
                break;
            case Mode.Attack:
                if (CurrentAttack == Attack.Swoop)
                {
                    pos += swoopDir * (Phase == 2 ? 19f : 16f) * dt;
                    if (pos.y <= GroundY || pos.x <= minX || pos.x >= maxX) timer = Mathf.Min(timer, .02f);
                }
                break;
            case Mode.Stagger:
            case Mode.Recovery:
                pos.y = Mathf.MoveTowards(pos.y, GroundY, 14f * dt);
                break;
        }
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Max(GroundY, pos.y);
        body.MovePosition(pos);
    }

    public void TakeDamage(int amount, Vector2 point) => ReceiveHit(amount, point, point, CombatFaction.Player);

    public bool ReceiveHit(int amount, Vector2 source, Vector2 point, CombatFaction attacker)
    {
        if (attacker == Faction || health == null || !Active || !health.TryDamage(amount, Time.time, 0f)) return false;
        flashUntil = Time.time + .1f;
        if (health.IsDead) { Die(); return true; }
        if (Phase == 1 && health.Current <= MaxHealth / 2)
        {
            Phase = 2;
            cycle = 0;
            ClearShots();
            Set(Mode.Stagger, 1.8f);
        }
        else if (amount >= HeavyHit && Current != Mode.Stagger)
        {
            Set(Mode.Stagger, 1.4f);
        }
        return true;
    }

    private void Die()
    {
        ClearShots();
        hurtbox.enabled = false;
        Set(Mode.Dead);
        Died?.Invoke();
    }

    private static void ClearShots()
    {
        foreach (var shot in FindObjectsByType<PrototypeProjectile>(FindObjectsSortMode.None))
            if (shot != null && shot.Faction == CombatFaction.Enemy) shot.Despawn();
    }

    public void ResetBoss()
    {
        health.Reset(Time.time);
        Phase = 1;
        cycle = 0;
        flashUntil = 0f;
        CurrentAttack = Attack.None;
        body.position = home;
        transform.position = new Vector3(home.x, home.y, transform.position.z);
        hurtbox.enabled = true;
        Set(Mode.Dormant);
    }

    public void SetDefeated()
    {
        health.Reset(Time.time);
        health.TryDamage(MaxHealth, Time.time, 0f);
        hurtbox.enabled = false;
        Current = Mode.Dead;
        Refresh();
    }

    private void Refresh()
    {
        if (shape == null) return;
        bool dead = Current == Mode.Dead;
        shape.enabled = !dead;
        label.gameObject.SetActive(!dead && Current != Mode.Dormant);
        Color tintBody = Phase == 2 ? new Color(.75f, .3f, .5f) : new Color(.2f, .55f, .65f);
        shape.color = Time.time < flashUntil ? Color.white
            : Current == Mode.Windup ? new Color(1f, .85f, .2f)
            : Current == Mode.Stagger ? new Color(.4f, .6f, 1f)
            : Current == Mode.Recovery ? new Color(.3f, .35f, .55f) : tintBody;

        bool showTelegraph = Current == Mode.Windup || (Current == Mode.Attack && CurrentAttack == Attack.Swoop);
        telegraph.enabled = showTelegraph;
        telegraph.color = Current == Mode.Attack ? new Color(1f, .15f, .1f, .5f) : new Color(1f, .85f, .1f, .25f);
        var t = telegraph.transform;
        if (CurrentAttack == Attack.Swoop)
        {
            // Duong lao: tu boss den diem khoa (keo dai qua diem do).
            Vector2 from = Position;
            Vector2 dir = Current == Mode.Attack ? swoopDir : ((swoopTarget - from).sqrMagnitude > .01f ? (swoopTarget - from).normalized : Vector2.down);
            if (dir.y > -.2f) dir.y = -.2f;
            dir.Normalize();
            float length = 9f;
            t.position = from + dir * (length * .5f);
            t.localScale = new Vector3(length, 1.1f, 1f);
            t.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
        else
        {
            // Rain: dai canh bao tren mat dat quanh tam roi.
            float width = (Phase == 2 ? 6 : 4) * (Phase == 2 ? 2.2f : 2.8f) + .6f;
            t.position = new Vector2(rainCenter, GroundY - .3f);
            t.localScale = new Vector3(width, .35f, 1f);
            t.rotation = Quaternion.identity;
        }
        label.text = "STORMWING " + Health + "/" + MaxHealth + (Phase == 2 ? " [ENRAGED]" : "") + " - " + Current;
    }
}
