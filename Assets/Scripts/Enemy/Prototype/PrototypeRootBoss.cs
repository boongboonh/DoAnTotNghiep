using UnityEngine;

/// <summary>
/// Boss thu 3 "Gloomroot" (graybox, khu rieng): boss ban re, dung yen, co VO. Khi con vo thi don nhe (Edge, Arc, Sentry)
/// bi doi lai; chi sat thuong nang (&gt;= 3: Spirit Smash, Bash vao boss, dan Bash tra lai) moi pha vo va mo "loi" 4.5 giay,
/// luc do moi don deu gay sat thuong. Don: Spikes (cot gai moc len theo bao hieu) va Seeds (dan nhan Bash duoc).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PrototypeRootBoss : MonoBehaviour, ICombatReceiver, IBashTarget
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

    public enum Mode { Dormant, Idle, Windup, Attack, Recovery, Open, Dead }
    public enum Attack { None, Spikes, Seeds }

    public const int MaxHealth = 36;
    public const int HeavyHit = 3;
    private const float OpenTime = 4.5f;
    private const float GroundTop = -.15f;
    private const float ColumnWidth = 1f, ColumnHeight = 3.2f, ColumnStep = 2.6f;
    private const int MaxColumns = 5;

    private PlayerController target;
    private PrototypePlayerHealth vitality;
    private Sprite sprite;
    private Rigidbody2D body;
    private BoxCollider2D hurtbox;
    private PrototypeHealthState health;
    private SpriteRenderer shape;
    private SpriteRenderer shell;
    private TextMesh label;
    private readonly SpriteRenderer[] columns = new SpriteRenderer[MaxColumns];
    private readonly float[] columnX = new float[MaxColumns];
    private int columnCount;
    private Vector2 home;
    private float triggerX, minX, maxX;
    private float timer, flashUntil, deflectUntil;
    private bool struck;
    private int cycle;
    private Vector2 aim = Vector2.left;
    private readonly Attack[] cyclePhase1 = { Attack.Seeds, Attack.Spikes, Attack.Seeds, Attack.Spikes };
    private readonly Attack[] cyclePhase2 = { Attack.Spikes, Attack.Seeds, Attack.Spikes, Attack.Spikes };

    public Mode Current { get; private set; }
    public Attack CurrentAttack { get; private set; }
    public int Phase { get; private set; } = 1;
    public int Health => health != null ? health.Current : 0;
    public bool Active => Current != Mode.Dormant && Current != Mode.Dead;
    public bool Defeated => Current == Mode.Dead;
    public bool Shielded => Active && Current != Mode.Open;
    public float OpenRemaining => Current == Mode.Open ? Mathf.Max(0f, timer) : 0f;
    public CombatFaction Faction => CombatFaction.Enemy;
    public Vector2 Position => body != null ? body.position : (Vector2)transform.position;
    public event System.Action Died;
    public event System.Action Engaged;

    public PrototypeEnemyState State => Current == Mode.Windup ? PrototypeEnemyState.Windup
        : Current == Mode.Attack ? PrototypeEnemyState.Attack
        : Current == Mode.Dead ? PrototypeEnemyState.Dead
        : Current == Mode.Dormant ? PrototypeEnemyState.Idle
        : Current == Mode.Idle ? PrototypeEnemyState.Chase : PrototypeEnemyState.Recovery;

    private bool PlayerAlive => !target.IsDead && !vitality.IsDead;
    private float Scale => Phase == 2 ? .8f : 1f;

    public void Initialize(PlayerController player, PrototypePlayerHealth hp, Sprite square, float triggerLine,
        float arenaMinX, float arenaMaxX, Vector2 homePosition)
    {
        target = player; vitality = hp; sprite = square; triggerX = triggerLine; minX = arenaMinX; maxX = arenaMaxX;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        hurtbox = GetComponent<BoxCollider2D>();
        hurtbox.size = new Vector2(3f, 2.6f);
        hurtbox.isTrigger = true;
        home = homePosition;
        health = new PrototypeHealthState(MaxHealth);
        shape = PrototypeGeometry.Rectangle(transform, square, "Gloomroot Body", hurtbox.size, Vector2.zero, new Color(.25f, .4f, .2f));
        shell = PrototypeGeometry.Rectangle(transform, square, "Gloomroot Shell", hurtbox.size + new Vector2(.4f, .4f), Vector2.zero, new Color(.55f, .6f, .65f, .8f), 7);
        PrototypeGeometry.Rectangle(transform, square, "Gloomroot Eye", new Vector2(.5f, .3f), new Vector2(-.6f, .4f), Color.yellow, 8);
        label = PrototypeGeometry.Label(transform, "Gloomroot Status", new Vector2(0f, 2.1f), .08f);
        for (int i = 0; i < MaxColumns; i++)
        {
            columns[i] = PrototypeGeometry.Rectangle(null, square, "Gloomroot Spike " + i, Vector2.one, Vector2.zero, Color.red, 5);
            columns[i].enabled = false;
        }
        ResetBoss();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < columns.Length; i++)
            if (columns[i] != null) Destroy(columns[i].gameObject);
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
                Set(Mode.Idle, 1.5f);
                Engaged?.Invoke();
            }
            return;
        }
        if (!PlayerAlive) { if (Current != Mode.Idle) { HideColumns(); Set(Mode.Idle, 1f); } return; }
        timer -= dt;
        switch (Current)
        {
            case Mode.Idle:
                if (timer <= 0f) BeginAttack();
                break;
            case Mode.Windup:
                if (CurrentAttack == Attack.Seeds) aim = (target.Motor.Position - Position + Vector2.up * .2f).normalized;
                // Spikes: theo Player den 0.35s cuoi roi khoa vi tri de co thoi gian chay ra.
                else if (timer > .35f * Scale) PlaceColumns(target.Motor.Position.x);
                if (timer <= 0f) Strike();
                break;
            case Mode.Attack:
                if (CurrentAttack == Attack.Spikes) SpikeHit();
                if (timer <= 0f) { HideColumns(); Set(Mode.Recovery, .8f * Scale); }
                break;
            case Mode.Recovery:
                if (timer <= 0f) Set(Mode.Idle, Phase == 2 ? .9f : 1.4f);
                break;
            case Mode.Open:
                if (timer <= 0f) Set(Mode.Idle, 1f);
                break;
        }
        Refresh();
    }

    private void BeginAttack()
    {
        var list = Phase == 2 ? cyclePhase2 : cyclePhase1;
        CurrentAttack = list[cycle++ % list.Length];
        aim = (target.Motor.Position - Position + Vector2.up * .2f).normalized;
        columnCount = 0;
        if (CurrentAttack == Attack.Spikes) PlaceColumns(target.Motor.Position.x);
        Set(Mode.Windup, (CurrentAttack == Attack.Spikes ? 1.1f : .8f) * Scale);
    }

    private void PlaceColumns(float centerX)
    {
        columnCount = Phase == 2 ? 5 : 3;
        for (int i = 0; i < columnCount; i++)
            columnX[i] = Mathf.Clamp(centerX + (i - (columnCount - 1) * .5f) * ColumnStep, minX, maxX);
    }

    private void HideColumns()
    {
        for (int i = 0; i < columns.Length; i++) columns[i].enabled = false;
        columnCount = 0;
    }

    private void Strike()
    {
        if (CurrentAttack == Attack.Spikes)
        {
            Set(Mode.Attack, .5f);
            return;
        }
        int count = Phase == 2 ? 5 : 3;
        float spread = 16f;
        float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        for (int i = 0; i < count; i++)
        {
            float angle = (baseAngle + (i - (count - 1) * .5f) * spread) * Mathf.Deg2Rad;
            var root = new GameObject("Gloomroot Seed");
            root.transform.position = Position + new Vector2(-1f, .4f);
            PrototypeGeometry.Rectangle(root.transform, sprite, "Shot", new Vector2(.34f, .34f), Vector2.zero, new Color(.7f, 1f, .3f), 6);
            root.AddComponent<PrototypeProjectile>().Initialize(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), body, null);
        }
        Set(Mode.Attack, .15f);
    }

    private void SpikeHit()
    {
        if (struck) return;
        Bounds player = target.GetComponent<Collider2D>().bounds;
        for (int i = 0; i < columnCount; i++)
        {
            float cy = GroundTop + ColumnHeight * .5f;
            var box = new Bounds(new Vector3(columnX[i], cy, 0f), new Vector3(ColumnWidth, ColumnHeight, 1f));
            box.center = new Vector3(columnX[i], cy, player.center.z);
            if (!box.Intersects(player)) continue;
            struck = true;
            vitality.TryHit(1, new Vector2(columnX[i], GroundTop));
            return;
        }
    }

    public void TakeDamage(int amount, Vector2 point) => ReceiveHit(amount, point, point, CombatFaction.Player);

    public bool ReceiveHit(int amount, Vector2 source, Vector2 point, CombatFaction attacker)
    {
        if (attacker == Faction || health == null || !Active) return false;
        // Con vo: don nhe bi doi lai.
        if (Current != Mode.Open && amount < HeavyHit)
        {
            deflectUntil = Time.time + .12f;
            return false;
        }
        if (!health.TryDamage(amount, Time.time, 0f)) return false;
        flashUntil = Time.time + .1f;
        if (health.IsDead) { Die(); return true; }
        if (Phase == 1 && health.Current <= MaxHealth / 2)
        {
            Phase = 2;
            cycle = 0;
            ClearShots();
        }
        if (Current != Mode.Open)
        {
            // Don nang pha vo: huy don dang chuan bi va mo loi.
            HideColumns();
            ClearShots();
            Set(Mode.Open, OpenTime);
        }
        return true;
    }

    private void Die()
    {
        ClearShots();
        HideColumns();
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
        flashUntil = 0f; deflectUntil = 0f;
        CurrentAttack = Attack.None;
        HideColumns();
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
        HideColumns();
        Current = Mode.Dead;
        Refresh();
    }

    private void LateUpdate()
    {
        // Nhap nhay khi bi doi don nhe (nhin thay ngay la khong tac dung).
        if (shell != null && shell.enabled)
            shell.color = Time.time < deflectUntil ? new Color(.9f, .95f, 1f, .95f) : new Color(.55f, .6f, .65f, .8f);
    }

    private void Refresh()
    {
        if (shape == null) return;
        bool dead = Current == Mode.Dead;
        shape.enabled = !dead;
        shell.enabled = !dead && Current != Mode.Dormant ? Current != Mode.Open : !dead;
        label.gameObject.SetActive(!dead && Current != Mode.Dormant);
        Color tintBody = Phase == 2 ? new Color(.55f, .3f, .15f) : new Color(.25f, .4f, .2f);
        shape.color = Time.time < flashUntil ? Color.white
            : Current == Mode.Windup ? new Color(.9f, .8f, .2f)
            : Current == Mode.Open ? new Color(.95f, .55f, .2f) : tintBody;

        for (int i = 0; i < columns.Length; i++)
        {
            bool show = i < columnCount && (Current == Mode.Windup || Current == Mode.Attack) && CurrentAttack == Attack.Spikes;
            columns[i].enabled = show;
            if (!show) continue;
            var t = columns[i].transform;
            if (Current == Mode.Attack)
            {
                columns[i].color = new Color(1f, .15f, .1f, .8f);
                t.localScale = new Vector3(ColumnWidth, ColumnHeight, 1f);
                t.position = new Vector3(columnX[i], GroundTop + ColumnHeight * .5f, 0f);
            }
            else
            {
                columns[i].color = new Color(1f, .85f, .1f, .5f);
                t.localScale = new Vector3(ColumnWidth, .3f, 1f);
                t.position = new Vector3(columnX[i], GroundTop + .15f, 0f);
            }
        }
        label.text = "GLOOMROOT " + Health + "/" + MaxHealth + (Phase == 2 ? " [ENRAGED]" : "") + " - "
            + (Current == Mode.Open ? "CORE OPEN" : Current.ToString());
    }
}
