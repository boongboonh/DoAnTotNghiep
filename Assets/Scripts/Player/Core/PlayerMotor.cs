using System;
using UnityEngine;

/// <summary>
/// Vat ly di chuyen cua player tren Rigidbody2D: chay co gia toc, nhay (coyote, nha nut de nhay thap),
/// trong luc, dash, quay mat, do mat dat. KHONG doc input va khong quyet dinh trang thai;
/// PlayerController goi cac ham o day.
/// Motor so huu gravityScale cua Rigidbody2D: script khac muon doi gravity thi goi
/// <see cref="SetGravityOverride"/>, khong gan truc tiep rb.gravityScale.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(-90)]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private PlayerMovementConfig config;
    [SerializeField] private LayerMask groundMask;
    [Tooltip("Collider dung de do mat dat. De trong = Collider2D tren cung object.")]
    [SerializeField] private Collider2D body;

    private Rigidbody2D rb;
    private float baseGravity;
    private bool gravityOverridden;
    private float coyoteTimer;
    private float airFallSpeed;
    private bool airJumpSpent;
    private bool airDashSpent;
    private bool dashing;
    private Collider2D support;
    private Vector2 supportPosition;
    private Collider2D droppedPlatform;
    private float dropStarted;
    public event Action Crushed;
    public bool CanDropThrough => Grounded && IsOneWay(support);
    private float wallJumpControlRemaining;
    public int WallDirection { get; private set; }
    private int lastWallDirection;
    private float wallCoyoteTimer;
    private float wallRegrabRemaining;
    private int wallJumpAway;
    /// <summary>Co the nhay tuong: dang cham tuong hoac vua roi tuong (coyote), tren khong va khong bi khoa sau bat tuong.</summary>
    public bool CanWallJump => !Grounded && !WallJumpProtected && (WallDirection != 0 || wallCoyoteTimer > 0f);
    public bool WallJumpProtected => wallJumpControlRemaining > 0f;
    public bool AirJumpAvailable => !airJumpSpent;
    public bool AirDashAvailable => !airDashSpent;
    public bool DashBlocked { get; private set; }
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[16];

    public PlayerMovementConfig Config => config;
    /// <summary>gravityScale goc cua Rigidbody2D (chua nhan he so roi).</summary>
    public float BaseGravity => baseGravity;
    public bool GravityOverridden => gravityOverridden;
    public bool Grounded { get; private set; }
    public bool FacingRight { get; private set; } = true;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
    public Vector2 Position => rb != null ? rb.position : (Vector2)transform.position;
    /// <summary>Dang tren dat hoac con trong thoi gian coyote.</summary>
    public bool CanJump => Grounded || coyoteTimer > 0f;

    /// <summary>Vua nhay (da dat van toc).</summary>
    public event Action Jumped;
    /// <summary>Vua cham dat; tham so la toc do roi truoc luc cham (duong).</summary>
    public event Action<float> Landed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseGravity = rb.gravityScale;
        if (body == null) body = GetComponent<Collider2D>();
        FacingRight = transform.right.x >= 0f;
    }

    private void OnEnable()
    {
        Grounded = config != null && CheckGround();
        coyoteTimer = 0f;
        airFallSpeed = 0f;
        gravityOverridden = false;
        ResetAirActions();
        dashing = false;
        DashBlocked = false;
        ClearWallMotion();
        if (rb != null) rb.gravityScale = baseGravity;
    }

    private void OnDisable()
    {
        RestoreDropCollision();
        support = null;
        if (rb != null) rb.gravityScale = baseGravity;
    }

    private void FixedUpdate()
    {
        UpdateDropCollision();
        CarryWithSupport();
        bool was = Grounded;
        Grounded = CheckGround();
        float dt = Time.fixedDeltaTime;
        wallJumpControlRemaining = Mathf.Max(0f, wallJumpControlRemaining - dt);
        wallRegrabRemaining = Mathf.Max(0f, wallRegrabRemaining - dt);
        WallDirection = Grounded || WallJumpProtected ? 0 : DetectWall();
        // Nho tuong vua cham de nhay tuong tre mot chut van duoc.
        if (WallDirection != 0) { lastWallDirection = WallDirection; wallCoyoteTimer = config.wallCoyoteTime; }
        else wallCoyoteTimer = Grounded ? 0f : Mathf.Max(0f, wallCoyoteTimer - dt);

        if (Grounded)
        {
            coyoteTimer = config.coyoteTime;
            if (!was)
            {
                ResetAirActions();
                float fall = airFallSpeed;
                airFallSpeed = 0f;
                Landed?.Invoke(fall);
            }
        }
        else
        {
            // A dash that carries us off a ledge uses the airborne charge too.
            if (dashing) airDashSpent = true;
            coyoteTimer -= dt;
            if (rb.linearVelocity.y < 0f) airFallSpeed = Mathf.Max(airFallSpeed, -rb.linearVelocity.y);
        }

        if (!gravityOverridden)
        {
            bool falling = rb.linearVelocity.y < 0f;
            rb.gravityScale = baseGravity * (falling ? config.fallGravityMultiplier : 1f);
            if (rb.linearVelocity.y < -config.maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -config.maxFallSpeed);
        }
    }

    // ---------------- Di chuyen ----------------

    /// <summary>Tien toi van toc ngang muc tieu theo input (-1..1), co gia toc.</summary>
    public void MoveHorizontal(float input)
    {
        if (WallJumpProtected) return;
        float target = input * config.runSpeed;
        float accel;
        if (Mathf.Abs(input) > 0.01f) accel = Grounded ? config.groundAcceleration : config.airAcceleration;
        else accel = Grounded ? config.groundDeceleration : config.airDeceleration;
        // Vua bat tuong: giu huong ve phia tuong thi quay lai nhanh de bam tiep (leo mot buc tuong).
        if (wallRegrabRemaining > 0f && input * wallJumpAway < -.01f) accel = Mathf.Max(accel, config.wallRegrabAcceleration);

        Vector2 v = rb.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, target, accel * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }

    public void StopHorizontal()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    /// <summary>Gan thang van toc ngang (lao khi danh), giu nguyen van toc doc.</summary>
    public void SetHorizontalVelocity(float vx)
    {
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    /// <summary>Giam dan van toc ngang ve 0 theo luc can (don vi/giay^2), dung luc bi choang.</summary>
    public void DampHorizontal(float drag)
    {
        Vector2 v = rb.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, 0f, drag * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }

    public void Jump()
    {
        LaunchJump(config.jumpVelocity);
    }

    public bool TryAirJump()
    {
        if (Grounded || CanJump || airJumpSpent) return false;
        airJumpSpent = true;
        LaunchJump(config.airJumpVelocity);
        return true;
    }

    private void LaunchJump(float speed)
    {
        support = null;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed);
        Grounded = false;
        coyoteTimer = 0f;
        airFallSpeed = 0f;
        Jumped?.Invoke();
    }

    /// <summary>Nha nut nhay khi dang bay len: cat van toc doc de nhay thap hon.</summary>
    public void CutJump()
    {
        Vector2 v = rb.linearVelocity;
        if (v.y > 0f) rb.linearVelocity = new Vector2(v.x, v.y * config.jumpCutMultiplier);
    }

    // ---------------- Dash ----------------

    public bool CanWallSlide(float input) => !Grounded && !WallJumpProtected
        && Velocity.y <= 0f && WallDirection != 0 && input * WallDirection > .01f;

    public bool TryWallJump()
    {
        if (!CanWallJump) return false;
        int side = WallDirection != 0 ? WallDirection : lastWallDirection;
        int away = -side;
        LaunchJump(Mathf.Abs(config.wallJumpVelocity.y));
        rb.linearVelocity = new Vector2(away * Mathf.Abs(config.wallJumpVelocity.x), rb.linearVelocity.y);
        wallJumpControlRemaining = config.wallJumpControlTime;
        wallRegrabRemaining = config.wallRegrabWindow;
        wallJumpAway = away;
        wallCoyoteTimer = 0f;
        WallDirection = 0;
        Face(away);
        return true;
    }

    // ---------------- Bam / leo tuong (Climb, kieu Ori) ----------------

    private int climbSide;
    /// <summary>Huong tuong dang bam (+1 phai, -1 trai), 0 neu khong bam.</summary>
    public int ClimbSide => climbSide;

    public bool CanWallClimb(bool grabHeld) => grabHeld && !Grounded && !WallJumpProtected && WallDirection != 0;

    public void BeginWallClimb()
    {
        climbSide = WallDirection;
        SetGravityOverride(0f);
        rb.linearVelocity = Vector2.zero;
        wallRegrabRemaining = 0f;
        if (climbSide != 0) Face(climbSide);
    }

    /// <summary>Leo len/xuong theo truc (-1..1), dinh sat tuong, khong roi.</summary>
    public void ClimbWall(float axis)
    {
        float speed = axis > 0f ? config.wallClimbUpSpeed : config.wallClimbDownSpeed;
        rb.linearVelocity = new Vector2(climbSide * .5f, axis * speed);
    }

    /// <summary>Het tuong o dinh: nhay nhe qua mep ve phia tuong (khong tu roi xuong).</summary>
    public void ClimbLedgeHop()
    {
        if (climbSide == 0) return;
        rb.linearVelocity = new Vector2(climbSide * config.wallLedgeHop.x, config.wallLedgeHop.y);
    }

    public void EndWallClimb()
    {
        if (climbSide == 0 && !gravityOverridden) return;
        ClearGravityOverride();
        climbSide = 0;
    }

    public void SlideOnWall()
    {
        var v = rb.linearVelocity;
        v.y = Mathf.Max(v.y, -config.wallSlideSpeed);
        rb.linearVelocity = v;
    }

    public void ClearWallMotion()
    {
        WallDirection = 0;
        wallJumpControlRemaining = 0f;
        wallRegrabRemaining = 0f;
        wallCoyoteTimer = 0f;
    }

    private int DetectWall()
    {
        if (body == null) return 0;
        // Probe the middle of the body so floor corners and ceilings do not count as walls.
        var bounds = body.bounds;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(groundMask);
        filter.useTriggers = false;
        for (int side = -1; side <= 1; side += 2)
        {
            int count = Physics2D.Raycast(bounds.center, Vector2.right * side, filter, groundHits,
                bounds.extents.x + config.wallCheckDistance);
            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider == null || hit.rigidbody == rb || hit.normal.x * side > -.9f) continue;
                var platform = hit.collider.GetComponent<PlatformEffector2D>();
                if (hit.collider.usedByEffector && platform != null && platform.isActiveAndEnabled && platform.useOneWay) continue;
                return side;
            }
        }
        return 0;
    }

    /// <param name="direction">+1 sang phai, -1 sang trai.</param>
    public void BeginDash(int direction)
    {
        ClearWallMotion();
        airDashSpent = true;
        dashing = true;
        DashBlocked = false;
        coyoteTimer = 0f;
        SetGravityOverride(0f);
        HoldDash(direction);
    }

    /// <summary>Giu van toc dash on dinh moi tick (chong va cham lam mat toc do).</summary>
    public void HoldDash(int direction)
    {
        if (DashBlocked) return;
        float speed = config.dashSpeed;
        if (body != null)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(groundMask);
            filter.useTriggers = false;
            Vector2 heading = direction > 0 ? Vector2.right : Vector2.left;
            float travel = speed * Time.fixedDeltaTime;
            int count = body.Cast(heading, filter, groundHits, travel + .01f);
            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider == null || hit.rigidbody == rb || Vector2.Dot(hit.normal, heading) > -.65f) continue;
                speed = Mathf.Min(speed, Mathf.Max(0f, hit.distance - .01f) / Time.fixedDeltaTime);
                DashBlocked = true;
            }
        }
        rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    public void EndDash()
    {
        dashing = false;
        ClearGravityOverride();
        Vector2 v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -config.dashExitSpeed, config.dashExitSpeed);
        rb.linearVelocity = v;
    }

    // ---------------- Bash ----------------

    public bool BashBlocked { get; private set; }

    /// <summary>Hang tai cho khi ngam Bash: dung yen, khong gravity.</summary>
    public void BeginBash()
    {
        ClearWallMotion();
        coyoteTimer = 0f;
        BashBlocked = false;
        dashing = false;
        SetGravityOverride(0f);
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>Giu van toc phong theo huong bat ky; dung o vat can dac, giong HoldDash.</summary>
    public void HoldBash(Vector2 heading, float speed)
    {
        if (BashBlocked) { rb.linearVelocity = Vector2.zero; return; }
        heading = heading.sqrMagnitude > .0001f ? heading.normalized : Vector2.right;
        if (body != null)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(groundMask);
            filter.useTriggers = false;
            float travel = speed * Time.fixedDeltaTime;
            int count = body.Cast(heading, filter, groundHits, travel + .01f);
            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider == null || hit.rigidbody == rb || hit.collider == droppedPlatform
                    || Vector2.Dot(hit.normal, heading) > -.65f) continue;
                speed = Mathf.Min(speed, Mathf.Max(0f, hit.distance - .01f) / Time.fixedDeltaTime);
                BashBlocked = true;
            }
        }
        rb.linearVelocity = heading * speed;
    }

    /// <summary>Ket thuc/huy Bash: tra gravity, gioi han van toc thoat.</summary>
    public void EndBash(float exitSpeed)
    {
        BashBlocked = false;
        ClearGravityOverride();
        Vector2 v = rb.linearVelocity;
        if (v.magnitude > exitSpeed) rb.linearVelocity = v.normalized * exitSpeed;
    }

    /// <summary>Bash thanh cong hoi mot luot air dash (khong hoi nhay doi).</summary>
    public void GrantAirDash() { airDashSpent = false; pogoSpent = false; }

    // ---------------- Luc ngoai (knockback, bi day) ----------------

    public void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    public void ResetMotion(Vector2 position)
    {
        RestoreDropCollision();
        support = null;
        ClearWallMotion();
        dashing = false;
        DashBlocked = false;
        BashBlocked = false;
        ResetAirActions();
        ClearGravityOverride();
        rb.linearVelocity = Vector2.zero;
        rb.position = position;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        Grounded = false;
        coyoteTimer = 0f;
        airFallSpeed = 0f;
    }

    private bool pogoSpent;
    public bool PogoAvailable => !pogoSpent;

    /// <summary>Chem xuong trung muc tieu: bat len mot lan moi lan roi (hoi lai khi cham dat hoac Bash), kieu Spirit Edge cua Ori.</summary>
    public bool TryPogo(float velocity)
    {
        if (pogoSpent || Grounded) return false;
        pogoSpent = true;
        support = null;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocity);
        coyoteTimer = 0f;
        airFallSpeed = 0f;
        return true;
    }

    private void ResetAirActions()
    {
        pogoSpent = false;
        airJumpSpent = false;
        airDashSpent = false;
    }

    public void AddImpulse(Vector2 impulse)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    /// <summary>Gan gravityScale co dinh (vd 0 khi dash, leo thang). Goi ClearGravityOverride de tra lai.</summary>
    public void SetGravityOverride(float scale)
    {
        gravityOverridden = true;
        rb.gravityScale = scale;
    }

    public void ClearGravityOverride()
    {
        gravityOverridden = false;
        rb.gravityScale = baseGravity;
    }

    // ---------------- Quay mat ----------------

    /// <param name="direction">+1 nhin phai, -1 nhin trai. Xoay root quanh Y (cac bone con tu quay theo).</param>
    public void Face(int direction)
    {
        if (direction == 0) return;
        bool right = direction > 0;
        FacingRight = right;
        transform.rotation = Quaternion.Euler(0f, right ? 0f : 180f, 0f);
    }

    // ---------------- Mat dat ----------------

    private static bool IsOneWay(Collider2D collider)
    {
        if (collider == null || !collider.usedByEffector) return false;
        var effector = collider.GetComponent<PlatformEffector2D>();
        return effector != null && effector.isActiveAndEnabled && effector.useOneWay;
    }

    public void DropThrough()
    {
        if (!CanDropThrough) return;
        droppedPlatform = support;
        dropStarted = Time.time;
        Physics2D.IgnoreCollision(body, droppedPlatform, true);
        support = null;
        Grounded = false;
        coyoteTimer = 0f;
        ClearWallMotion();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
    }

    public void RestoreDropCollision()
    {
        if (body != null && droppedPlatform != null) Physics2D.IgnoreCollision(body, droppedPlatform, false);
        droppedPlatform = null;
    }

    private void UpdateDropCollision()
    {
        if (droppedPlatform == null) return;
        if (Time.time - dropStarted < .25f) return;
        if (body.bounds.max.y < droppedPlatform.bounds.min.y || !droppedPlatform.enabled)
            RestoreDropCollision();
        else if (Time.time - dropStarted > 1f)
        {
            RestoreDropCollision();
            Crushed?.Invoke();
        }
    }

    private void CarryWithSupport()
    {
        if (!Grounded || support == null || support.attachedRigidbody == null || rb.linearVelocity.y > .1f) return;
        var platformBody = support.attachedRigidbody;
        Vector2 delta = platformBody.position - supportPosition;
        supportPosition = platformBody.position;
        if (delta.sqrMagnitude < .000001f) return;
        var filter = new ContactFilter2D(); filter.SetLayerMask(groundMask); filter.useTriggers = false;
        int count = body.Cast(delta.normalized, filter, groundHits, delta.magnitude + .01f);
        for (int i = 0; i < count; i++)
        {
            var hit = groundHits[i];
            if (hit.collider == null || hit.rigidbody == rb || hit.collider == support || hit.collider == droppedPlatform) continue;
            if (Vector2.Dot(hit.normal, delta.normalized) < -.1f)
            {
                support = null;
                Crushed?.Invoke();
                return;
            }
        }
        rb.position += delta;
    }

    private bool CheckGround()
    {
        support = null;
        if (body == null || config == null || rb.linearVelocity.y > 0.1f) return false;
        var filter = new ContactFilter2D();
        filter.SetLayerMask(groundMask);
        filter.useTriggers = false;
        int count = body.Cast(Vector2.down, filter, groundHits, config.groundCheckExtra);
        for (int i = 0; i < count; i++)
        {
            var hit = groundHits[i];
            if (hit.collider == null || hit.rigidbody == rb || hit.collider == droppedPlatform || hit.normal.y < .65f) continue;
            if (IsOneWay(hit.collider) && body.bounds.min.y < hit.collider.bounds.max.y - .08f) continue;
            support = hit.collider;
            if (support.attachedRigidbody != null) supportPosition = support.attachedRigidbody.position;
            return true;
        }
        return false;
    }
}
