using UnityEngine;

/// <summary>Swept straight projectile; explicit faction and reset ownership.</summary>
public sealed class PrototypeProjectile : MonoBehaviour, IBashTarget
{
    [SerializeField, Min(.1f)] private float speed = 7f;
    [SerializeField, Min(.1f)] private float lifetime = 4f;
    [SerializeField, Min(1)] private int damage = 1;
    [Header("Sau khi bi Bash")]
    [SerializeField, Min(.1f)] private float bashedSpeed = 10f;
    [SerializeField, Min(1)] private int bashedDamage = 3;
    private Vector2 direction;
    private float age;
    private Rigidbody2D owner;
    private PrototypeCombatRoom room;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
    public CombatFaction Faction { get; private set; } = CombatFaction.Enemy;
    public bool Spent { get; private set; }
    /// <summary>Moi vien dan chi bi Bash mot lan.</summary>
    public bool Bashed { get; private set; }
    public Vector2 BashPosition => transform.position;
    public Vector2 Direction => direction;

    public void Initialize(Vector2 heading, Rigidbody2D source, PrototypeCombatRoom arena)
    {
        direction = heading.sqrMagnitude > .001f ? heading.normalized : Vector2.left;
        owner = source; room = arena;
        var collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = .12f; collider.isTrigger = true;
        room?.Register(this);
        PrototypeBash.Register(this);
    }

    public bool CanBash(Vector2 playerPosition, float range) => !Spent && !Bashed;

    /// <summary>Doi phe sang Player va bay nguoc huong phong cua Player.</summary>
    public bool Bash(Vector2 playerLaunchDirection)
    {
        if (Spent || Bashed) return false;
        Bashed = true;
        Faction = CombatFaction.Player;
        owner = null;
        direction = playerLaunchDirection.sqrMagnitude > .001f ? -playerLaunchDirection.normalized : -direction;
        speed = bashedSpeed;
        damage = bashedDamage;
        age = 0f;
        var shape = GetComponentInChildren<SpriteRenderer>();
        if (shape != null) shape.color = Color.cyan;
        PrototypeBash.Unregister(this);
        return true;
    }

    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if (Spent || dt <= 0f) return;
        age += dt;
        if (age >= lifetime) { Despawn(); return; }
        Vector2 from = transform.position;
        float distance = speed * dt;
        var filter = new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers = true;
        int count = Physics2D.CircleCast(from, .12f, direction, filter, hits, distance);
        Collider2D nearest = null;
        float nearestDistance = float.PositiveInfinity;
        ICombatReceiver receiver = null;
        for (int i = 0; i < count; i++)
        {
            var c = hits[i].collider;
            if (c == null || c.gameObject == gameObject || (owner != null && c.attachedRigidbody == owner)) continue;
            var candidate = c.GetComponentInParent<ICombatReceiver>();
            if (candidate != null && candidate.Faction == Faction) continue;
            if (candidate == null && (c.isTrigger || c.gameObject.layer != 0)) continue;
            if (hits[i].distance >= nearestDistance) continue;
            nearest = c; nearestDistance = hits[i].distance; receiver = candidate;
        }
        if (nearest != null)
        {
            transform.position = from + direction * nearestDistance;
            // Chi dan da Bash moi pha duoc vat can Bash; dan quai thuong chi bi chan.
            if (Bashed) nearest.GetComponentInParent<PrototypeBashBarrier>()?.Break();
            receiver?.ReceiveHit(damage, from - direction * .2f, nearest.ClosestPoint(transform.position), Faction);
            Despawn();
        }
        else transform.position = from + direction * distance;
    }

    public void Despawn()
    {
        if (Spent) return;
        Spent = true;
        PrototypeBash.Unregister(this);
        gameObject.SetActive(false);
        room?.Unregister(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        room?.Unregister(this);
        PrototypeBash.Unregister(this);
    }
}
