using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Muc tieu Bash duoc: diem neo co dinh hoac dan cua quai.</summary>
public interface IBashTarget
{
    Vector2 BashPosition { get; }
    /// <summary>Con Bash duoc khong. Diem neo dung tham so de tu mo lai khi Player roi pham vi.</summary>
    bool CanBash(Vector2 playerPosition, float range);
    /// <param name="playerLaunchDirection">Huong Player bi phong di.</param>
    bool Bash(Vector2 playerLaunchDirection);
}

/// <summary>
/// Bash (M5): giu R de khoa muc tieu gan nhat, the gioi cham lai, ngam bang chuot, tha R de phong.
/// Player bay theo huong ngam; dan bi doi phe va bay nguoc lai. Thanh cong hoi mot luot air dash.
/// PlayerController chay BashState goi Begin/Tick/FixedTick/Cancel; module nay khong tu doi state.
/// Time scale di qua HitStop.SetBaseScale de khong de len hitstop. Chua co pause (PAU-01).
/// </summary>
public sealed class PrototypeBash : MonoBehaviour
{
    public enum BashPhase { None, Aim, Launch }

    [Tooltip("M6 se bat/tat theo nang luc nhan duoc. Hien mo san de choi thu.")]
    [SerializeField] private bool unlocked = true;
    [SerializeField, Min(1f)] private float range = 7f;
    [SerializeField, Range(.02f, 1f)] private float aimTimeScale = .12f;
    [Tooltip("Thoi gian ngam toi da, tinh theo unscaled time. Het gio tu phong.")]
    [SerializeField, Min(.1f)] private float maxAimSeconds = 1.2f;
    [SerializeField, Min(1f)] private float launchSpeed = 20f;
    [SerializeField, Min(.05f)] private float launchTime = .22f;
    [SerializeField, Min(1f)] private float exitSpeed = 8f;
    [Header("Lo lung sau khi phong (de noi chuoi)")]
    [Tooltip("Thoi gian giam gravity sau khi phong xong. 0 = tat.")]
    [SerializeField, Min(0f)] private float floatTime = .45f;
    [Tooltip("He so gravity trong luc lo lung (1 = binh thuong).")]
    [SerializeField, Range(0f, 1f)] private float floatGravity = .3f;
    [Tooltip("Toc do roi toi da trong luc lo lung.")]
    [SerializeField, Min(0f)] private float floatMaxFall = 4f;

    private static readonly List<IBashTarget> targets = new List<IBashTarget>();

    public static void Register(IBashTarget target)
    {
        if (target != null && !targets.Contains(target)) targets.Add(target);
    }

    public static void Unregister(IBashTarget target) => targets.Remove(target);

    private PlayerController player;
    private PlayerInputReader controls;
    private PlayerMotor motor;
    private Rigidbody2D body;
    private Camera aimCamera;
    private Sprite square;
    private IBashTarget candidate;
    private IBashTarget locked;
    private Vector2 direction = Vector2.right;
    private float aimElapsed;
    private float launchElapsed;
    private bool slowActive;
    private float restoreScale = 1f;
    private float floatRemaining;
    private bool floatActive;
    private bool floatYielded;
    private SpriteRenderer arrow;
    private SpriteRenderer arrowHead;
    private readonly SpriteRenderer[] trail = new SpriteRenderer[6];
    private SpriteRenderer counterShaft;
    private SpriteRenderer counterHead;
    private SpriteRenderer highlight;
    private TextMesh label;

    public BashPhase Phase { get; private set; }
    public bool Busy => Phase != BashPhase.None;
    public bool Unlocked { get => unlocked; set => unlocked = value; }
    public Vector2 AimDirection => direction;
    public IBashTarget Candidate => candidate;
    public int SuccessCount { get; private set; }

    public void Initialize(PlayerController owner, Camera camera, Sprite shapeSprite)
    {
        player = owner;
        controls = owner.Controls;
        motor = owner.Motor;
        body = owner.GetComponent<Rigidbody2D>();
        aimCamera = camera;
        square = shapeSprite;
        // Mui ten huong phong: than day + dau kim cuong + cham quy dao toi diem den; mui ten do o muc tieu chi huong no bay.
        arrow = PrototypeGeometry.Rectangle(null, square, "Bash Aim Arrow", new Vector2(1.5f, .16f), Vector2.zero,
            Color.cyan, 20);
        arrowHead = PrototypeGeometry.Rectangle(null, square, "Bash Aim Head", new Vector2(.5f, .5f), Vector2.zero,
            Color.cyan, 21);
        for (int i = 0; i < trail.Length; i++)
            trail[i] = PrototypeGeometry.Rectangle(null, square, "Bash Trail " + i,
                i == trail.Length - 1 ? new Vector2(.34f, .34f) : new Vector2(.16f, .16f), Vector2.zero,
                new Color(.5f, 1f, 1f, .85f), 19);
        var counter = new Color(1f, .45f, .2f);
        counterShaft = PrototypeGeometry.Rectangle(null, square, "Bash Counter Arrow", new Vector2(1.1f, .12f), Vector2.zero, counter, 20);
        counterHead = PrototypeGeometry.Rectangle(null, square, "Bash Counter Head", new Vector2(.36f, .36f), Vector2.zero, counter, 21);
        SetVisuals(false);
        highlight = PrototypeGeometry.Rectangle(null, square, "Bash Target Highlight", new Vector2(.7f, .7f),
            Vector2.zero, Color.yellow, 19);
        label = PrototypeGeometry.Label(null, "Bash Label", Vector2.zero, .06f);
        arrow.enabled = false;
        highlight.enabled = false;
    }

    // ---------------- API cho PlayerController ----------------

    /// <summary>Vua bam R, da mo khoa va co muc tieu hop le trong tam/duong nhin.</summary>
    public bool CanBegin
    {
        get
        {
            if (!unlocked || Busy || controls == null || !controls.BashPressed) return false;
            candidate = FindNearest();
            return candidate != null;
        }
    }

    public void Begin()
    {
        locked = candidate != null ? candidate : FindNearest();
        if (locked == null) { Phase = BashPhase.None; return; }
        Phase = BashPhase.Aim;
        aimElapsed = 0f;
        direction = motor.FacingRight ? Vector2.right : Vector2.left;
        // Bat dau bang chuot (nhu cu); ban phim chi lay quyen khi nguoi choi bam huong.
        mouseControls = true;
        if (Mouse.current != null) lastMouse = Mouse.current.position.ReadValue();
        restoreScale = Mathf.Max(.01f, DangDang.Combat.HitStop.BaseScale);
        slowActive = true;
        DangDang.Combat.HitStop.SetBaseScale(aimTimeScale);
        motor.BeginBash();
    }

    public void Tick()
    {
        if (Phase != BashPhase.Aim) return;
        // Mat focus, khoa input hoac pause: huy, khong coi la tha nut.
        if (!controls.InputActive) { Cancel(); return; }
        // Muc tieu bien mat / het hop le giua chung: huy, khong tu tim muc tieu moi.
        if (!Alive(locked) || !locked.CanBash(motor.Position, float.PositiveInfinity)) { Cancel(); return; }

        aimElapsed += Time.unscaledDeltaTime;
        direction = ReadAim(direction);
        if (!controls.BashHeld || aimElapsed >= maxAimSeconds) Launch();
    }

    public void FixedTick()
    {
        if (Phase == BashPhase.Aim)
        {
            motor.SetVelocity(Vector2.zero);
        }
        else if (Phase == BashPhase.Launch)
        {
            launchElapsed += Time.fixedDeltaTime;
            motor.HoldBash(direction, launchSpeed);
            if (launchElapsed >= launchTime || motor.BashBlocked) Finish();
        }
    }

    /// <summary>Huy an toan o moi nhanh thoat: tra time scale, gravity, an hien thi. Goi nhieu lan khong sao.</summary>
    public void Cancel()
    {
        RestoreTime();
        if (Phase != BashPhase.None) motor.EndBash(exitSpeed);
        Phase = BashPhase.None;
        locked = null;
        SetVisuals(false);
    }

    // ---------------- Noi bo ----------------

    private void Launch()
    {
        if (!Alive(locked) || !locked.Bash(direction)) { Cancel(); return; }
        RestoreTime();
        Phase = BashPhase.Launch;
        launchElapsed = 0f;
        locked = null;
        SuccessCount++;
        motor.GrantAirDash();
        if (Mathf.Abs(direction.x) > .1f) motor.Face(direction.x > 0f ? 1 : -1);
        arrow.enabled = false;
    }

    private void Finish()
    {
        Phase = BashPhase.None;
        motor.EndBash(exitSpeed);
        SetVisuals(false);
        // Cua so noi chuoi: roi cham lai mot luc, khong cho Player ha xuong ho truoc khi kip Bash tiep.
        if (floatTime > 0f && !motor.Grounded)
        {
            floatRemaining = floatTime;
            floatActive = true;
            floatYielded = false;
            motor.SetGravityOverride(motor.BaseGravity * floatGravity);
        }
    }

    private void EndFloat()
    {
        // Dung co rieng, khong dua vao floatRemaining: het gio van phai tra trong luc (loi truoc day: bi roi cham/nhay cao mai).
        if (!floatActive) return;
        floatActive = false;
        floatRemaining = 0f;
        // Chi tra gravity neu day van la override cua minh (Dash/Dead/Bash/WallClimb tu quan ly cua ho).
        if (motor != null && !floatYielded) motor.ClearGravityOverride();
        floatYielded = false;
    }

    /// <summary>Reset/hoi sinh: motor da tra gravity, chi can bo dem float cu, khong xoa override cua nguoi khac.</summary>
    public void AbortFloat()
    {
        floatActive = false;
        floatRemaining = 0f;
        floatYielded = false;
    }

    private void FixedUpdate()
    {
        if (!floatActive || player == null) return;
        // Moi hanh dong khac nam quyen gravity hoac da het ly do lo lung thi nhuong lai.
        var loco = player.Locomotion;
        bool other = loco == PlayerLocomotion.Dash || loco == PlayerLocomotion.Hurt || loco == PlayerLocomotion.Dead
            || loco == PlayerLocomotion.Locked || loco == PlayerLocomotion.Bash || loco == PlayerLocomotion.WallSlide
            || loco == PlayerLocomotion.WallClimb || loco == PlayerLocomotion.Cast;
        if (other)
        {
            // Dash/Bash/Dead tu dat va tra override; ta chi dung lai bo dem.
            floatYielded = loco == PlayerLocomotion.Dash || loco == PlayerLocomotion.Dead || loco == PlayerLocomotion.Bash
                || loco == PlayerLocomotion.WallClimb;
            EndFloat();
            return;
        }
        floatRemaining -= Time.fixedDeltaTime;
        if (motor.Grounded || floatRemaining <= 0f) { EndFloat(); return; }
        var v = motor.Velocity;
        if (v.y < -floatMaxFall) motor.SetVelocity(new Vector2(v.x, -floatMaxFall));
    }

    private void RestoreTime()
    {
        if (!slowActive) return;
        slowActive = false;
        DangDang.Combat.HitStop.SetBaseScale(restoreScale);
    }

    private Vector2 lastMouse;
    private bool mouseControls = true;

    /// <summary>Thoi gian ngam con lai (giay thuc), de HUD canh bao sap tu phong.</summary>
    public float AimRemaining => Phase == BashPhase.Aim ? Mathf.Max(0f, maxAimSeconds - aimElapsed) : 0f;
    public float AimSecondsMax => maxAimSeconds;

    private Vector2 ReadAim(Vector2 last)
    {
        // Ban phim (WASD/mui ten) ngam duoc khi khong dung chuot; chuot di chuyen thi chuot lay lai quyen.
        Vector2 stick = controls.AimStick;
        if (stick.sqrMagnitude > .01f)
        {
            mouseControls = false;
            if (Mouse.current != null) lastMouse = Mouse.current.position.ReadValue();
            return stick;
        }
        Vector2 keys = new Vector2(controls.Move, (controls.UpHeld ? 1f : 0f) - (controls.DownHeld ? 1f : 0f));
        if (keys.sqrMagnitude > .01f)
        {
            mouseControls = false;
            if (Mouse.current != null) lastMouse = Mouse.current.position.ReadValue();
            return keys.normalized;
        }
        if (Mouse.current != null)
        {
            Vector2 now = Mouse.current.position.ReadValue();
            if ((now - lastMouse).sqrMagnitude > 16f) mouseControls = true;
            lastMouse = now;
        }
        if (!mouseControls) return last;
        if (Mouse.current == null || aimCamera == null) return last;
        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 world = aimCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y,
            Mathf.Abs(aimCamera.transform.position.z)));
        Vector2 delta = (Vector2)world - motor.Position;
        // Con tro trung tam: giu huong hop le cuoi cung (ban dau la huong nhin).
        return delta.sqrMagnitude > .09f ? delta.normalized : last;
    }

    private static bool Alive(IBashTarget target)
    {
        if (target == null) return false;
        return !(target is Object unityObject) || unityObject != null;
    }

    private IBashTarget FindNearest()
    {
        IBashTarget best = null;
        float bestDistance = float.PositiveInfinity;
        Vector2 from = motor.Position;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            var target = targets[i];
            if (!Alive(target)) { targets.RemoveAt(i); continue; }
            if (!target.CanBash(from, range)) continue;
            float distance = Vector2.Distance(from, target.BashPosition);
            if (distance > range || distance >= bestDistance) continue;
            if (distance > .05f && CombatOcclusion.Blocked(from, target.BashPosition, body, null)) continue;
            best = target;
            bestDistance = distance;
        }
        return best;
    }

    private void Update()
    {
        if (player == null) return;
        if (Phase == BashPhase.None) candidate = unlocked && !player.IsDead ? FindNearest() : null;
    }

    private void LateUpdate()
    {
        if (player == null) return;
        Vector2 pos = motor.Position;
        IBashTarget shown = Phase == BashPhase.Aim ? locked : Phase == BashPhase.None ? candidate : null;
        bool showTarget = Alive(shown);
        highlight.enabled = showTarget;
        if (showTarget)
        {
            highlight.transform.position = shown.BashPosition;
            highlight.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            highlight.color = Phase == BashPhase.Aim ? Color.white : new Color(1f, .9f, .2f, .75f);
        }
        bool aiming = Phase == BashPhase.Aim;
        arrow.enabled = aiming;
        arrowHead.enabled = aiming;
        foreach (var dot in trail) dot.enabled = aiming;
        bool counterOn = aiming && showTarget && !(shown is PrototypeBashAnchor);
        counterShaft.enabled = counterOn;
        counterHead.enabled = counterOn;
        if (aiming)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Sat nhan vat, dam va chon mau theo thoi gian ngam con lai (do dan khi sap tu phong).
            float urgency = 1f - Mathf.Clamp01((maxAimSeconds - aimElapsed) / (maxAimSeconds * .33f));
            Color tint = Color.Lerp(Color.cyan, new Color(1f, .35f, .25f), urgency);
            arrow.color = tint;
            arrowHead.color = tint;
            arrow.transform.SetPositionAndRotation(pos + direction * 1.35f, Quaternion.Euler(0f, 0f, angle));
            arrowHead.transform.SetPositionAndRotation(pos + direction * 2.25f, Quaternion.Euler(0f, 0f, angle + 45f));
            float travel = launchSpeed * launchTime;
            for (int i = 0; i < trail.Length; i++)
            {
                float d = i == trail.Length - 1 ? travel : Mathf.Lerp(2.85f, Mathf.Max(3.2f, travel - .5f), i / (float)(trail.Length - 2));
                trail[i].transform.position = pos + direction * d;
                trail[i].transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            }
            if (counterOn)
            {
                // Muc tieu (dan, quai) bi hat nguoc huong Player phong.
                Vector2 from = shown.BashPosition;
                Vector2 back = -direction;
                float backAngle = Mathf.Atan2(back.y, back.x) * Mathf.Rad2Deg;
                counterShaft.transform.SetPositionAndRotation(from + back * .95f, Quaternion.Euler(0f, 0f, backAngle));
                counterHead.transform.SetPositionAndRotation(from + back * 1.6f, Quaternion.Euler(0f, 0f, backAngle + 45f));
            }
        }
        label.transform.position = pos + Vector2.up * 1.5f;
        label.text = aiming ? "BASH  " + Mathf.Max(0f, maxAimSeconds - aimElapsed).ToString("0.0") + "s"
            : showTarget ? "HOLD R / C: BASH" : string.Empty;
    }

    private void SetVisuals(bool on)
    {
        if (arrow != null) arrow.enabled = on;
        if (arrowHead != null) arrowHead.enabled = on;
        foreach (var dot in trail) if (dot != null) dot.enabled = on;
        if (counterShaft != null) counterShaft.enabled = on;
        if (counterHead != null) counterHead.enabled = on;
        if (highlight != null) highlight.enabled = on;
        if (label != null && !on) label.text = string.Empty;
    }

    private void OnDisable() => Cancel();

    private void OnDestroy()
    {
        if (arrow != null) Destroy(arrow.gameObject);
        if (arrowHead != null) Destroy(arrowHead.gameObject);
        foreach (var dot in trail) if (dot != null) Destroy(dot.gameObject);
        if (counterShaft != null) Destroy(counterShaft.gameObject);
        if (counterHead != null) Destroy(counterHead.gameObject);
        if (highlight != null) Destroy(highlight.gameObject);
        if (label != null) Destroy(label.gameObject);
    }
}
