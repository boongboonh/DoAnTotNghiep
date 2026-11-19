using UnityEngine;

/// <summary>
/// Boss "Warden" (graybox) theo phong cach Ori: don co bao hieu ro, dan tim Bash duoc va phan lai gay sat thuong lon,
/// Bash thang vao boss de gay sat thuong va lam choang trong luc Player bi hat bay ra xa.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PrototypeBoss : MonoBehaviour, ICombatReceiver, IBashTarget
{
    // Ori: Bash thang vao boss = 3 sat thuong + lam choang, Player bi hat bay ra xa.
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

    public enum Mode { Dormant, Walk, Windup, Attack, Recovery, Stagger, Dead }
    public enum Attack { None, Stomp, Fan, Charge }

    public const int MaxHealth = 24;
    private const int HeavyHit = 3;

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
    private float timer;
    private float flashUntil;
    private bool struck;
    private int cycle;
    private int direction = -1;
    private Vector2 aim = Vector2.left;
    private readonly Attack[] cyclePhase1 = { Attack.Fan, Attack.Charge, Attack.Fan, Attack.Charge };
    private readonly Attack[] cyclePhase2 = { Attack.Fan, Attack.Fan, Attack.Charge };

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

    /// <summary>Trang thai dung chung voi HUD canh bao (Windup/Attack hien dau '!').</summary>
    public PrototypeEnemyState State => Current == Mode.Windup ? PrototypeEnemyState.Windup
        : Current == Mode.Attack ? PrototypeEnemyState.Attack
        : Current == Mode.Dead ? PrototypeEnemyState.Dead
        : Current == Mode.Dormant ? PrototypeEnemyState.Idle
        : Current == Mode.Walk ? PrototypeEnemyState.Chase : PrototypeEnemyState.Recovery;

    public void Initialize(PlayerController player, PrototypePlayerHealth hp, Sprite square, float triggerLine,
        float arenaMinX, float arenaMaxX)
    {
        target = player; vitality = hp; sprite = square; triggerX = triggerLine; minX = arenaMinX; maxX = arenaMaxX;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        hurtbox = GetComponent<BoxCollider2D>();
        hurtbox.size = new Vector2(1.8f, 2f);
        hurtbox.isTrigger = true;
        home = body.position;
        health = new PrototypeHealthState(MaxHealth);
        shape = PrototypeGeometry.Rectangle(transform, square, "Boss Body", hurtbox.size, Vector2.zero, new Color(.45f, .2f, .6f));
        PrototypeGeometry.Rectangle(transform, square, "Boss Eye", new Vector2(.35f, .22f), new Vector2(-.35f, .45f), Color.yellow, 6);
        telegraph = PrototypeGeometry.Rectangle(transform, square, "Boss Telegraph", Vector2.one, Vector2.zero, Color.yellow, 4);
        label = PrototypeGeometry.Label(transform, "Boss Status", new Vector2(0f, 1.7f), .08f);
        ResetBoss();
    }

    private bool PlayerAlive => !target.IsDead && !vitality.IsDead;
    private float Scale => Phase == 2 ? .8f : 1f;

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
                Set(Mode.Walk, 1.2f);
                Engaged?.Invoke();
            }
            return;
        }
        // Nguoi choi chet: ngung tan cong, cho reset.
        if (!PlayerAlive) { if (Current != Mode.Walk) Set(Mode.Walk, 1f); return; }
        timer -= dt;
        switch (Current)
        {
            case Mode.Walk:
                if (timer <= 0f) BeginAttack();
                break;
            case Mode.Windup:
                if (CurrentAttack == Attack.Fan) aim = (target.Motor.Position - Position + Vector2.up * .3f).normalized;
                if (timer <= 0f) Strike();
                break;
            case Mode.Attack:
                if (CurrentAttack == Attack.Charge) ChargeHit();
                if (timer <= 0f) Set(Mode.Recovery, RecoveryTime(CurrentAttack));
                break;
            case Mode.Recovery:
            case Mode.Stagger:
                if (timer <= 0f) Set(Mode.Walk, (Phase == 2 ? .6f : 1f));
                break;
        }
        Refresh();
    }

    private float RecoveryTime(Attack attack) => (attack == Attack.Charge ? 1.4f : attack == Attack.Stomp ? 1f : .8f) * Scale;

    private void BeginAttack()
    {
        float distance = Vector2.Distance(target.Motor.Position, Position);
        var list = Phase == 2 ? cyclePhase2 : cyclePhase1;
        CurrentAttack = distance < 3f ? Attack.Stomp : list[cycle++ % list.Length];
        direction = target.Motor.Position.x >= Position.x ? 1 : -1;
        aim = (target.Motor.Position - Position + Vector2.up * .3f).normalized;
        float windup = CurrentAttack == Attack.Stomp ? .85f : CurrentAttack == Attack.Fan ? .75f : 1.05f;
        Set(Mode.Windup, windup * Scale);
    }

    private void Strike()
    {
        switch (CurrentAttack)
        {
            case Attack.Stomp:
                // Vung dam quanh chan: nhay len de ne.
                Bounds player = target.GetComponent<Collider2D>().bounds;
                float feet = Position.y - hurtbox.size.y * .5f;
                if (Mathf.Abs(player.center.x - Position.x) <= 2.4f + player.extents.x && player.min.y < feet + .9f)
                    vitality.TryHit(1, Position);
                Set(Mode.Attack, .15f);
                break;
            case Attack.Fan:
                Fire(Phase == 2 ? 5 : 3);
                Set(Mode.Attack, .1f);
                break;
            default:
                Set(Mode.Attack, (Phase == 2 ? .9f : .8f));
                break;
        }
    }

    private void Fire(int count)
    {
        float spread = Phase == 2 ? 16f : 14f;
        float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        for (int i = 0; i < count; i++)
        {
            float angle = (baseAngle + (i - (count - 1) * .5f) * spread) * Mathf.Deg2Rad;
            var root = new GameObject("Boss Projectile");
            root.transform.position = Position + Vector2.up * .3f;
            PrototypeGeometry.Rectangle(root.transform, sprite, "Shot", new Vector2(.32f, .32f), Vector2.zero, new Color(.9f, .3f, 1f), 6);
            root.AddComponent<PrototypeProjectile>().Initialize(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), body, null);
        }
    }

    private void ChargeHit()
    {
        if (struck || !hurtbox.bounds.Intersects(target.GetComponent<Collider2D>().bounds)) return;
        struck = true;
        vitality.TryHit(1, Position);
    }

    private void FixedUpdate()
    {
        if (health == null || Current == Mode.Dead || Current == Mode.Dormant || !PlayerAlive) return;
        float speed = 0f;
        if (Current == Mode.Walk)
        {
            float dx = target.Motor.Position.x - Position.x;
            direction = dx >= 0f ? 1 : -1;
            speed = Mathf.Abs(dx) > 1.8f ? direction * (Phase == 2 ? 2.4f : 1.6f) : 0f;
        }
        else if (Current == Mode.Attack && CurrentAttack == Attack.Charge) speed = direction * (Phase == 2 ? 15f : 13f);
        if (Mathf.Abs(speed) < .01f) return;
        float x = Mathf.Clamp(Position.x + speed * Time.fixedDeltaTime, minX, maxX);
        if (Current == Mode.Attack && (x <= minX + .001f || x >= maxX - .001f)) timer = Mathf.Min(timer, .05f);
        body.MovePosition(new Vector2(x, home.y));
    }

    public void TakeDamage(int amount, Vector2 point) => ReceiveHit(amount, point, point, CombatFaction.Player);

    public bool ReceiveHit(int amount, Vector2 source, Vector2 point, CombatFaction attacker)
    {
        if (attacker == Faction || health == null || !Active || !health.TryDamage(amount, Time.time, 0f)) return false;
        flashUntil = Time.time + .1f;
        if (health.IsDead) { Die(); return true; }
        if (Phase == 1 && health.Current <= MaxHealth / 2)
        {
            // Doi pha: choang dai, huy dan cu, vao pha nhanh hon.
            Phase = 2;
            cycle = 0;
            ClearShots();
            Set(Mode.Stagger, 1.6f);
        }
        else if (amount >= HeavyHit && Current != Mode.Stagger)
        {
            // Dan phan lai bang Bash: choang ngan, tao cua so phan cong.
            Set(Mode.Stagger, 1.3f);
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
        foreach (var shot in FindObjectsOfType<PrototypeProjectile>())
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

    /// <summary>Boss da bi ha tu truoc (tien trinh da luu): an han di.</summary>
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
        Color tintBody = Phase == 2 ? new Color(.7f, .15f, .4f) : new Color(.45f, .2f, .6f);
        shape.color = Time.time < flashUntil ? Color.white
            : Current == Mode.Windup ? new Color(1f, .85f, .2f)
            : Current == Mode.Stagger ? new Color(.4f, .6f, 1f)
            : Current == Mode.Recovery ? new Color(.35f, .35f, .6f) : tintBody;

        telegraph.enabled = Current == Mode.Windup || (Current == Mode.Attack && CurrentAttack == Attack.Charge);
        Color tint = Current == Mode.Attack ? new Color(1f, .15f, .1f, .55f) : new Color(1f, .85f, .1f, .25f);
        telegraph.color = tint;
        var t = telegraph.transform;
        if (CurrentAttack == Attack.Stomp)
        {
            t.localScale = new Vector3(4.8f, .5f, 1f);
            t.localPosition = new Vector3(0f, -hurtbox.size.y * .5f + .25f, 0f);
            t.localRotation = Quaternion.identity;
        }
        else if (CurrentAttack == Attack.Fan)
        {
            t.localScale = new Vector3(4f, .12f, 1f);
            t.localPosition = new Vector3(aim.x * 2f, aim.y * 2f + .3f, 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
        }
        else
        {
            t.localScale = new Vector3(14f, 1.6f, 1f);
            t.localPosition = new Vector3(7.2f * direction, -.1f, 0f);
            t.localRotation = Quaternion.identity;
        }
        label.text = "WARDEN " + Health + "/" + MaxHealth + (Phase == 2 ? " [ENRAGED]" : "") + " - " + Current;
    }
}
