using UnityEngine;

public enum PrototypeEnemyKind { Shooter, Flyer }

/// <summary>Shared prototype health/telegraph lifecycle for ranged and flying enemies.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PrototypeSpecialEnemy : MonoBehaviour, ICombatReceiver, IBashTarget
{
    // Ori: Bash truc tiep vao quai ban/bay (2 sat thuong, bi hat nguoc huong Player phong).
    private void OnEnable() => PrototypeBash.Register(this);
    private void OnDisable() => PrototypeBash.Unregister(this);
    public Vector2 BashPosition => body != null ? body.position : (Vector2)transform.position;
    public bool CanBash(Vector2 playerPosition, float range) =>
        health != null && !health.IsDead && Vector2.Distance(playerPosition, BashPosition) <= range;
    public bool Bash(Vector2 launchDirection)
    {
        if (!CanBash(BashPosition, float.PositiveInfinity)) return false;
        ReceiveHit(2, BashPosition + launchDirection.normalized, BashPosition, CombatFaction.Player);
        return true;
    }

    [SerializeField] private float detectRange = 7f;
    [SerializeField] private float windupTime = .65f;
    [SerializeField] private float recoveryTime = 1f;
    [SerializeField] private float rushSpeed = 8f;
    [SerializeField] private float rushTime = .4f;
    private PlayerController target;
    private PrototypePlayerHealth vitality;
    private PrototypeCombatRoom room;
    private Sprite sprite;
    private Rigidbody2D body;
    private BoxCollider2D hurtbox;
    private PrototypeHealthState health;
    private SpriteRenderer shape;
    private SpriteRenderer telegraph;
    private TextMesh label;
    private Vector2 home;
    private Vector2 aim;
    private Vector2 knockback;
    private float elapsed;
    private float age;
    private float flashUntil;
    private bool struck;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[32];
    public PrototypeEnemyKind Kind { get; private set; }
    public PrototypeEnemyState State { get; private set; }
    public CombatFaction Faction => CombatFaction.Enemy;
    public int Health => health != null ? health.Current : 0;
    public event System.Action Died;

    public void Initialize(PlayerController player, PrototypePlayerHealth hp, Sprite square,
        PrototypeEnemyKind kind, PrototypeCombatRoom owner)
    {
        target = player; vitality = hp; sprite = square; Kind = kind; room = owner;
        body = GetComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        hurtbox = GetComponent<BoxCollider2D>(); hurtbox.size = kind == PrototypeEnemyKind.Flyer ? new Vector2(.8f,.7f) : new Vector2(.8f,1.4f);
        hurtbox.isTrigger = true; home = body.position;
        health = new PrototypeHealthState(3);
        shape = PrototypeGeometry.Rectangle(transform,square,"Body",hurtbox.size,Vector2.zero,Color.magenta);
        telegraph = PrototypeGeometry.Rectangle(transform,square,"Aim",new Vector2(2f,.07f),Vector2.zero,Color.yellow);
        label = PrototypeGeometry.Label(transform,"HP / State",new Vector2(0,1f),.065f);
        ResetEnemy();
    }

    private void Change(PrototypeEnemyState state) { State=state; elapsed=0f; struck=false; Refresh(); }
    private void Update() => Advance(Time.deltaTime);
    public void Advance(float dt)
    {
        if (health == null || health.IsDead || dt <= 0f) return;
        age += dt; elapsed += dt;
        if (target.IsDead || vitality.IsDead)
        {
            if (State != PrototypeEnemyState.Idle) Change(PrototypeEnemyState.Idle);
            return;
        }
        if (State == PrototypeEnemyState.Idle)
        {
            Vector2 delta = target.Motor.Position - body.position;
            if (delta.magnitude <= detectRange && !CombatOcclusion.Blocked(body.position,target.Motor.Position,body,target.GetComponent<Rigidbody2D>()))
            {
                aim = delta.sqrMagnitude > .001f ? delta.normalized : Vector2.left;
                Change(PrototypeEnemyState.Windup);
            }
        }
        else if (State == PrototypeEnemyState.Windup && elapsed >= windupTime)
        {
            // Aim is locked during windup. A new obstacle cancels a shot rather than firing through it.
            if (Kind == PrototypeEnemyKind.Shooter)
            {
                if (!CombatOcclusion.Blocked(body.position,target.Motor.Position,body,target.GetComponent<Rigidbody2D>())) Fire();
                Change(PrototypeEnemyState.Recovery);
            }
            else Change(PrototypeEnemyState.Attack);
        }
        else if (State == PrototypeEnemyState.Attack && elapsed >= rushTime) Change(PrototypeEnemyState.Recovery);
        else if (State == PrototypeEnemyState.Recovery && elapsed >= recoveryTime) Change(PrototypeEnemyState.Idle);
        else if (State == PrototypeEnemyState.Hurt && elapsed >= .25f) Change(PrototypeEnemyState.Recovery);
        Refresh();
    }

    private void Fire()
    {
        var root = new GameObject("Enemy Projectile"); root.transform.position=body.position;
        PrototypeGeometry.Rectangle(root.transform,sprite,"Shot",new Vector2(.24f,.24f),Vector2.zero,new Color(1f,.5f,.1f),6);
        root.AddComponent<PrototypeProjectile>().Initialize(aim,body,room);
    }

    private void FixedUpdate() => TickMotion(Time.fixedDeltaTime);
    public void TickMotion(float dt)
    {
        if (health == null || health.IsDead || target.IsDead || dt <= 0f) return;
        Vector2 delta=Vector2.zero;
        if (State == PrototypeEnemyState.Hurt)
        {
            delta=knockback*dt; knockback=Vector2.MoveTowards(knockback,Vector2.zero,18f*dt);
        }
        else if (Kind == PrototypeEnemyKind.Flyer)
        {
            if (State == PrototypeEnemyState.Attack) delta=aim*rushSpeed*dt;
            else if (State == PrototypeEnemyState.Idle || State == PrototypeEnemyState.Recovery)
                delta=Vector2.MoveTowards(body.position,home+new Vector2(Mathf.Sin(age)*.5f,Mathf.Sin(age*2)*.15f),2.2f*dt)-body.position;
        }
        Vector2 next=body.position+delta;
        next.x=Mathf.Clamp(next.x,home.x-3f,home.x+3f);
        next.y=Kind==PrototypeEnemyKind.Shooter ? home.y : Mathf.Clamp(next.y,home.y-2f,home.y+2f);
        delta=next-body.position;
        if (delta.sqrMagnitude > .000001f)
        {
            var filter=new ContactFilter2D();filter.SetLayerMask(1);filter.useTriggers=false;
            int count=hurtbox.Cast(delta.normalized,filter,hits,delta.magnitude+.01f);
            for(int i=0;i<count;i++)
            {
                if(hits[i].collider==null || hits[i].rigidbody==body || hits[i].rigidbody==target.GetComponent<Rigidbody2D>())continue;
                if(Vector2.Dot(hits[i].normal,delta.normalized)>=-.1f)continue;
                delta=delta.normalized*Mathf.Min(delta.magnitude,Mathf.Max(0,hits[i].distance-.01f));
                if(State==PrototypeEnemyState.Attack)Change(PrototypeEnemyState.Recovery);
            }
        }
        if (Kind==PrototypeEnemyKind.Flyer && State==PrototypeEnemyState.Attack && !struck)
        {
            var filter=new ContactFilter2D();filter.SetLayerMask(1);filter.useTriggers=false;
            int count=Physics2D.BoxCast(body.position,hurtbox.size,0f,delta.normalized,filter,hits,delta.magnitude);
            bool touching=hurtbox.bounds.Intersects(target.GetComponent<Collider2D>().bounds);
            for(int i=0;i<count;i++)
                if(hits[i].rigidbody==target.GetComponent<Rigidbody2D>())touching=true;
            if(touching
                && !CombatOcclusion.Blocked(body.position,target.Motor.Position,body,target.GetComponent<Rigidbody2D>()))
            { struck=true; vitality.ReceiveHit(1,body.position,target.Motor.Position,Faction); }
        }
        body.MovePosition(body.position+delta);
    }

    public void TakeDamage(int amount,Vector2 point) => ReceiveHit(amount,point,point,CombatFaction.Player);
    public bool ReceiveHit(int amount,Vector2 source,Vector2 point,CombatFaction attacker)
    {
        if(attacker==Faction || health==null || !health.TryDamage(amount,Time.time,0))return false;
        flashUntil=Time.time+.1f; PushAwayFrom(source);
        if(health.IsDead){hurtbox.enabled=false;body.linearVelocity=Vector2.zero;Change(PrototypeEnemyState.Dead);Died?.Invoke();}
        else Change(PrototypeEnemyState.Hurt);
        return true;
    }
    public void PushAwayFrom(Vector2 source,float speed=3.5f)
    {
        Vector2 delta=body.position-source;
        knockback=(delta.sqrMagnitude>.001f?delta.normalized:Vector2.right)*speed;
        if(Kind==PrototypeEnemyKind.Shooter)knockback.y=0;
    }
    public void ResetEnemy()
    {
        health.Reset(Time.time);body.position=home;transform.position=home;body.linearVelocity=Vector2.zero;
        hurtbox.enabled=true;knockback=Vector2.zero;flashUntil=0;age=0;Change(PrototypeEnemyState.Idle);
    }
    private void Refresh()
    {
        shape.color=health.IsDead?Color.gray:Time.time<flashUntil?Color.white:State==PrototypeEnemyState.Windup?Color.yellow
            :Kind==PrototypeEnemyKind.Shooter?new Color(1f,.5f,.15f):new Color(.8f,.35f,1f);
        telegraph.enabled=State==PrototypeEnemyState.Windup || State==PrototypeEnemyState.Attack;
        telegraph.transform.localPosition=aim;
        telegraph.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg);
        telegraph.color=State==PrototypeEnemyState.Attack?Color.red:Color.yellow;
        label.text=Kind+" "+Health+"/3 - "+State;
    }
}
