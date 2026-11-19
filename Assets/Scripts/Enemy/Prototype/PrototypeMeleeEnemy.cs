using UnityEngine;

public enum PrototypeEnemyState { Idle, Chase, Windup, Attack, Recovery, Hurt, Dead }

/// <summary>Quai can chien graybox: huong danh chot luc gong, khong gay damage tiep xuc.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PrototypeMeleeEnemy : MonoBehaviour, ICombatReceiver, IBashTarget
{
    // ---- Ori: Bash truc tiep vao quai (gay 2 sat thuong, quai bi hat nguoc huong Player phong) ----
    private void OnEnable() => PrototypeBash.Register(this);
    private void OnDisable() => PrototypeBash.Unregister(this);
    public Vector2 BashPosition => body != null ? body.position : (Vector2)transform.position;
    public bool CanBash(Vector2 playerPosition, float range) =>
        health != null && State != PrototypeEnemyState.Dead && Vector2.Distance(playerPosition, BashPosition) <= range;
    public bool Bash(Vector2 launchDirection)
    {
        if (!CanBash(BashPosition, float.PositiveInfinity)) return false;
        ReceiveHit(2, BashPosition + launchDirection.normalized, BashPosition, CombatFaction.Player);
        return true;
    }

    [SerializeField] private float detectRange = 4f;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float moveSpeed = 2.1f;
    [SerializeField] private float windupTime = .5f;
    [SerializeField] private float activeTime = .12f;
    [SerializeField] private float recoveryTime = .7f;
    private PlayerController target;
    private PrototypePlayerHealth playerHealth;
    private Rigidbody2D body;
    private BoxCollider2D hitbox;
    private PrototypeHealthState health;
    private SpriteRenderer shape;
    private SpriteRenderer region;
    private SpriteRenderer marker;
    private TextMesh label;
    private Vector2 home;
    private float elapsed;
    private float flashUntil;
    private float knockVelocity;
    private int direction = 1;
    private bool dealtHit;
    private ContactFilter2D worldFilter;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[12];
    public PrototypeEnemyState State { get; private set; }
    public int Health => health != null ? health.Current : 0;
    public CombatFaction Faction => CombatFaction.Enemy;
    public event System.Action Died;

    // ---- Bien the Charger: bao hieu lan dai, lao thang, dung lai ho (co hoi phan cong kieu Hollow Knight) ----
    private bool charger;
    private float chargeSpeed = 10f;
    public bool IsCharger => charger;

    /// <summary>Doi quai thanh Charger: mau cao hon, gong lau hon, lao thang theo huong da chot roi hut hoi.</summary>
    public void MakeCharger()
    {
        charger = true;
        attackRange = 4.2f;
        detectRange = 6.5f;
        windupTime = .85f;
        activeTime = .5f;
        recoveryTime = 1.1f;
        health = new PrototypeHealthState(6);
        ResetEnemy();
    }

    public void Initialize(PlayerController player, PrototypePlayerHealth vitality, Sprite square)
    {
        target = player;
        playerHealth = vitality;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.size = new Vector2(.8f, 1.4f);
        hitbox.isTrigger = true;
        home = body.position;
        health = new PrototypeHealthState(4);
        worldFilter = new ContactFilter2D();
        worldFilter.SetLayerMask(1);
        worldFilter.useTriggers = false;
        shape = PrototypeGeometry.Rectangle(transform, square, "Enemy Shape", hitbox.size, Vector2.zero, Color.red);
        marker = PrototypeGeometry.Rectangle(transform, square, "Facing", new Vector2(.12f, .12f), new Vector2(.25f,.2f), Color.black, 6);
        region = PrototypeGeometry.Rectangle(transform, square, "Enemy Attack", new Vector2(1.3f,1.15f), new Vector2(.9f,0), Color.red, 4);
        label = PrototypeGeometry.Label(transform, "Enemy Status", new Vector2(0,1.1f), .075f);
        ResetEnemy();
    }

    private void SetState(PrototypeEnemyState state)
    {
        State = state;
        elapsed = 0f;
        dealtHit = false;
        RefreshVisual();
    }

    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if (health == null || State == PrototypeEnemyState.Dead || dt <= 0f) return;
        if (playerHealth.IsDead)
        {
            if (State != PrototypeEnemyState.Idle) SetState(PrototypeEnemyState.Idle);
            return;
        }
        float previous = elapsed;
        elapsed += dt;
        float distance = Vector2.Distance(target.Motor.Position, body.position);
        switch (State)
        {
            case PrototypeEnemyState.Idle:
                if (distance <= detectRange && !WallBetween(target.Motor.Position)) SetState(PrototypeEnemyState.Chase);
                break;
            case PrototypeEnemyState.Chase:
                if (distance > detectRange + 2f) SetState(PrototypeEnemyState.Idle);
                else if (distance <= attackRange && !WallBetween(target.Motor.Position))
                {
                    direction = target.Motor.Position.x >= body.position.x ? 1 : -1;
                    SetState(PrototypeEnemyState.Windup);
                }
                break;
            case PrototypeEnemyState.Windup:
                if (elapsed >= windupTime) SetState(PrototypeEnemyState.Attack);
                break;
            case PrototypeEnemyState.Attack:
                if (charger) TryChargeHit();
                else if (previous <= activeTime) TryStrike();
                if (elapsed >= activeTime) SetState(PrototypeEnemyState.Recovery);
                break;
            case PrototypeEnemyState.Recovery:
                if (elapsed >= recoveryTime) SetState(PrototypeEnemyState.Chase);
                break;
            case PrototypeEnemyState.Hurt:
                if (elapsed >= .25f) SetState(PrototypeEnemyState.Chase);
                break;
        }
        RefreshVisual();
    }

    private void TryChargeHit()
    {
        if (dealtHit || playerHealth.IsDead) return;
        if (!hitbox.bounds.Intersects(target.GetComponent<Collider2D>().bounds)) return;
        dealtHit = true;
        playerHealth.TryHit(1, body.position);
    }

    private void TryStrike()
    {
        if (dealtHit || playerHealth.IsDead || WallBetween(target.Motor.Position)) return;
        Vector2 delta = target.Motor.Position - (body.position + Vector2.right * direction * .9f);
        Bounds bounds = target.GetComponent<Collider2D>().bounds;
        if (Mathf.Abs(delta.x) <= .65f + bounds.extents.x && Mathf.Abs(delta.y) <= .575f + bounds.extents.y)
        {
            dealtHit = true;
            playerHealth.TryHit(1, body.position);
        }
    }

    private void FixedUpdate()
    {
        if (health == null || health.IsDead || playerHealth.IsDead) return;
        float speed = 0f;
        if (State == PrototypeEnemyState.Chase)
        {
            direction = target.Motor.Position.x >= body.position.x ? 1 : -1;
            speed = direction * moveSpeed;
        }
        else if (charger && State == PrototypeEnemyState.Attack)
            speed = direction * chargeSpeed;
        else if (State == PrototypeEnemyState.Hurt)
        {
            speed = knockVelocity;
            knockVelocity = Mathf.MoveTowards(knockVelocity, 0, 18f * Time.fixedDeltaTime);
        }
        if (Mathf.Abs(speed) < .01f) return;
        float sign = Mathf.Sign(speed);
        Vector2 foot = new Vector2(body.position.x + sign * .5f, body.position.y - .58f);
        int count = Physics2D.Raycast(foot, Vector2.down, worldFilter, hits, .3f);
        bool floor = false;
        for (int i = 0; i < count; i++) if (hits[i].normal.y > .65f) floor = true;
        if (!floor) return;
        float step = speed * Time.fixedDeltaTime;
        count = hitbox.Cast(Vector2.right * sign, worldFilter, hits, Mathf.Abs(step) + .02f);
        var playerBody = target.GetComponent<Rigidbody2D>();
        for (int i = 0; i < count; i++)
        {
            // Charger lao xuyen vao Player de gay sat thuong (khong bi chinh Player chan lai).
            if (charger && hits[i].rigidbody == playerBody) continue;
            if (hits[i].normal.x * sign < -.5f) return;
        }
        float x = charger ? Mathf.Clamp(body.position.x + step, home.x - 6f, home.x + 7f)
            : Mathf.Clamp(body.position.x + step, home.x - 2f, home.x + 4f);
        body.MovePosition(new Vector2(x, home.y));
    }

    private bool WallBetween(Vector2 destination)
    {
        Vector2 delta = destination - body.position;
        int count = Physics2D.Raycast(body.position, delta.normalized, worldFilter, hits, delta.magnitude);
        for (int i = 0; i < count; i++)
            if (hits[i].collider != null && hits[i].rigidbody != target.GetComponent<Rigidbody2D>() && hits[i].rigidbody != body)
                return true;
        return false;
    }

    public void TakeDamage(int amount, Vector2 hitPoint)
    {
        ReceiveHit(amount, hitPoint, hitPoint, CombatFaction.Player);
    }

    public bool ReceiveHit(int amount, Vector2 source, Vector2 hitPoint, CombatFaction attacker)
    {
        if (attacker == Faction) return false;
        if (health == null || !health.TryDamage(amount, Time.time, 0f)) return false;
        flashUntil = Time.time + .1f;
        PushAwayFrom(source);
        if (health.IsDead)
        {
            hitbox.enabled = false;
            body.linearVelocity = Vector2.zero;
            SetState(PrototypeEnemyState.Dead);
            Died?.Invoke();
        }
        else SetState(PrototypeEnemyState.Hurt);
        return true;
    }

    public void PushAwayFrom(Vector2 source, float speed = 3.5f) =>
        knockVelocity = (source.x <= body.position.x ? 1f : -1f) * speed;

    public void ResetEnemy()
    {
        health.Reset(Time.time);
        body.position = home;
        transform.position = new Vector3(home.x, home.y, transform.position.z);
        body.linearVelocity = Vector2.zero;
        hitbox.enabled = true;
        flashUntil = 0f;
        knockVelocity = 0f;
        SetState(PrototypeEnemyState.Idle);
    }

    private void RefreshVisual()
    {
        if (shape == null) return;
        Color baseColor = charger ? new Color(.95f, .45f, .1f) : new Color(.9f, .2f, .2f);
        shape.color = State == PrototypeEnemyState.Dead ? Color.gray : Time.time < flashUntil ? Color.white
            : State == PrototypeEnemyState.Windup ? new Color(1f,.8f,.1f)
            : charger && State == PrototypeEnemyState.Recovery ? new Color(.4f, .55f, .9f) : baseColor;
        marker.transform.localPosition = new Vector3(.25f * direction,.2f,0);
        region.enabled = State == PrototypeEnemyState.Windup || State == PrototypeEnemyState.Attack;
        if (charger)
        {
            // Duong lao dai bao truoc: nguoi choi thay ro huong va do dai de ne.
            region.transform.localScale = new Vector3(5f, .9f, 1f);
            region.transform.localPosition = new Vector3(2.9f * direction, -.2f, 0);
        }
        else region.transform.localPosition = new Vector3(.9f * direction, 0, 0);
        region.color = State == PrototypeEnemyState.Windup ? new Color(1f,.8f,.1f,.2f) : new Color(1f,.15f,.1f,.6f);
        label.text = (charger ? "CHARGER " : "MELEE ") + health.Current + "/" + health.Maximum + " - " + State;
    }
}
