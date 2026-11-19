using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Doc input mot cho duy nhat cho player. Cac script khac chi hoi PlayerInputReader, khong goi UnityEngine.Input,
/// nen doi phim / them tay cam / khoa input (cutscene, hoi thoai) chi sua o day.
///   - Cac nut bam (nhay, dash, danh, ky nang) duoc nho trong bufferTime giay, den khi duoc Consume.
///   - Locked = true: tra ve khong input va xoa bo dem.
/// </summary>
[DefaultExecutionOrder(-100)]
public class PlayerInputReader : MonoBehaviour
{
    [Header("Phim")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [Tooltip("Phim cho cac slot skill: skillKeys[i] ung voi slot i cua PlayerSkills.")]
    [SerializeField] private KeyCode[] skillKeys = { KeyCode.E, KeyCode.R, KeyCode.F };
    [SerializeField] private KeyCode switchWeaponKey = KeyCode.Q;
    [Tooltip("0 = chuot trai, 1 = chuot phai.")]
    [SerializeField] private int attackMouseButton = 0;
    [SerializeField] private int dashMouseButton = 1;

    [Header("Bo dem lenh")]
    [Tooltip("Lenh bam som duoc giu trong khoang nay (giay). Rieng nhay dung gia tri trong PlayerMovementConfig neu gan.")]
    [SerializeField] private float bufferTime = 0.15f;
    [SerializeField, Min(0f)] private float attackBufferTime = 0.2f;

    private const float None = -100f;
    private readonly PlayerActionBuffer attack = new PlayerActionBuffer();
    private readonly PlayerActionBuffer jump = new PlayerActionBuffer();
    private readonly PlayerActionBuffer dash = new PlayerActionBuffer();
    private float[] skillAt;
    private bool jumpReleaseLatch;
    private bool switchWeaponLatch;
    private float jumpBuffer = -1f;
    private float dashBuffer = -1f;
    private InputActionMap actionMap;
    private InputAction jumpAction;
    private string leftControlName;
    private string rightControlName;
    private bool focused = true;
    /// <summary>Chi dat boi cong cu Editor (AgentPlaySetup) de choi thu khi Unity khong co focus. Build luon false.</summary>
    public static bool IgnoreFocusLoss;
    private bool AcceptInput => isActiveAndEnabled && (focused || IgnoreFocusLoss) && !locked
        && (Time.timeScale > 0f || DangDang.Combat.HitStop.IsActive);

    public bool Locked
    {
        get => locked;
        set
        {
            locked = value;
            if (value) Clear();
        }
    }
    private bool locked;

    /// <summary>Huong ngang, -1..1.</summary>
    public float Move { get; private set; }
    public bool JumpHeld { get; private set; }
    private const float StickDeadzone = .35f;
    public bool DownHeld => AcceptInput && ((Keyboard.current != null
        && (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed))
        || (Gamepad.current != null && (Gamepad.current.dpad.down.isPressed || Gamepad.current.leftStick.y.ReadValue() < -.55f)));
    public bool UpHeld => AcceptInput && ((Keyboard.current != null
        && (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed))
        || (Gamepad.current != null && (Gamepad.current.dpad.up.isPressed || Gamepad.current.leftStick.y.ReadValue() > .55f)));
    /// <summary>Bash (kieu Ori): giu R hoac C / nut Y tay cam. Ngam bang chuot / can / WASD, tha de phong.</summary>
    public bool BashHeld => AcceptInput && ((Keyboard.current != null && (Keyboard.current.rKey.isPressed || Keyboard.current.cKey.isPressed))
        || (Gamepad.current != null && Gamepad.current.buttonNorth.isPressed));
    public bool BashPressed => AcceptInput && ((Keyboard.current != null
            && (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame))
        || (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame));
    /// <summary>Spirit Arc (ban cung): giu Q hoac LT tay cam de ngam, tha de ban.</summary>
    public bool ArcHeld => AcceptInput && ((Keyboard.current != null && Keyboard.current.qKey.isPressed)
        || (Gamepad.current != null && Gamepad.current.leftTrigger.isPressed));
    public bool ArcPressed => AcceptInput && ((Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        || (Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame));
    /// <summary>Bam tuong (Climb, kieu Ori): giu Shift hoac RT tay cam.</summary>
    public bool GrabHeld => AcceptInput && ((Keyboard.current != null
            && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
        || (Gamepad.current != null && Gamepad.current.rightTrigger.isPressed));
    /// <summary>Huong len/xuong tuong tu (analog): ban phim W/S hoac can trai.</summary>
    public float ClimbAxis
    {
        get
        {
            if (!AcceptInput) return 0f;
            float axis = (UpHeld ? 1f : 0f) - (DownHeld ? 1f : 0f);
            return axis;
        }
    }
    /// <summary>True when a gamepad is connected (HUD shows pad legend).</summary>
    public static bool GamepadConnected => Gamepad.current != null;
    /// <summary>Analog aim from the gamepad: right stick, else left stick. Zero inside the deadzone.</summary>
    public Vector2 AimStick
    {
        get
        {
            var pad = Gamepad.current;
            if (!AcceptInput || pad == null) return Vector2.zero;
            Vector2 right = pad.rightStick.ReadValue();
            if (right.sqrMagnitude > StickDeadzone * StickDeadzone) return right.normalized;
            Vector2 left = pad.leftStick.ReadValue();
            return left.sqrMagnitude > StickDeadzone * StickDeadzone ? left.normalized : Vector2.zero;
        }
    }
    public bool PausePressed => Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
    /// <summary>False while locked, unfocused or paused; true during hitstop. Lets Bash tell "released" from "interrupted".</summary>
    public bool InputActive => AcceptInput;
    public bool InteractionEnabled { get; set; }
    public bool InteractPressed => InteractionEnabled && AcceptInput
        && ((Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame));
    /// <summary>Co bat ky phim nao vua duoc bam trong frame nay.</summary>
    public bool AnyKeyDown { get; private set; }

    private bool CanReadBuffer => AcceptInput;
    private float JumpWindow => jumpBuffer >= 0f ? jumpBuffer : bufferTime;
    private float DashWindow => dashBuffer >= 0f ? dashBuffer : bufferTime;
    public bool JumpBuffered => CanReadBuffer && jump.Pending(Time.time, JumpWindow);
    public bool DashBuffered => CanReadBuffer && dash.Pending(Time.time, DashWindow);
    public bool AttackBuffered => CanReadBuffer && attack.Pending(Time.time, attackBufferTime);

    /// <summary>Dat thoi gian nho lenh nhay rieng (tu config). Khong goi = dung bufferTime.</summary>
    public void SetJumpBuffer(float seconds) => jumpBuffer = seconds;
    public void SetDashBuffer(float seconds) => dashBuffer = seconds;

    public bool ConsumeJump() => ConsumeJump(out _);
    public bool ConsumeDash() => ConsumeDash(out _);
    public bool ConsumeJump(out bool released)
    {
        released = jump.Released;
        return JumpBuffered && jump.Consume(Time.time, JumpWindow);
    }
    public bool ConsumeDash(out int direction)
    {
        direction = dash.Direction;
        return DashBuffered && dash.Consume(Time.time, DashWindow);
    }
    public bool ConsumeAttack() => AttackBuffered && attack.Consume(Time.time, attackBufferTime);

    public int SkillSlotCount => skillKeys.Length;

    /// <summary>Ten phim cua slot de hien tren HUD (vd "E", phim so bo tien to Alpha).</summary>
    public string SkillKeyName(int slot)
    {
        if (slot < 0 || slot >= skillKeys.Length) return string.Empty;
        string n = skillKeys[slot].ToString();
        return n.StartsWith("Alpha") ? n.Substring(5) : n;
    }

    public bool SkillBuffered(int slot)
    {
        EnsureSkillBuffers();
        return slot >= 0 && slot < skillAt.Length && Buffered(skillAt[slot], bufferTime);
    }

    public bool ConsumeSkill(int slot)
    {
        EnsureSkillBuffers();
        if (!SkillBuffered(slot)) return false;
        skillAt[slot] = None;
        return true;
    }

    /// <summary>Slot skill vua bam gan nhat con trong bo dem, hoac -1 neu khong co.</summary>
    public int BufferedSkillSlot()
    {
        EnsureSkillBuffers();
        int best = -1;
        float bestTime = None;
        for (int i = 0; i < skillAt.Length; i++)
        {
            if (!Buffered(skillAt[i], bufferTime) || skillAt[i] <= bestTime) continue;
            best = i;
            bestTime = skillAt[i];
        }
        return best;
    }

    /// <summary>True mot lan sau khi nha nut nhay (chot lai de FixedUpdate khong bo sot).</summary>
    public bool ConsumeJumpRelease()
    {
        bool r = jumpReleaseLatch;
        jumpReleaseLatch = false;
        return r;
    }

    /// <summary>True mot lan sau khi bam phim doi vu khi.</summary>
    public bool ConsumeSwitchWeapon()
    {
        bool r = switchWeaponLatch;
        switchWeaponLatch = false;
        return r;
    }

    private void OnEnable()
    {
        if (actionMap == null) BuildActions();
        actionMap.Enable();
    }

    private void OnDisable()
    {
        actionMap?.Disable();
        Clear();
    }

    private void OnDestroy() => actionMap?.Dispose();

    private void BuildActions()
    {
        actionMap = new InputActionMap("Player Gameplay");
        leftControlName = KeyPath(moveLeftKey).Substring("<Keyboard>/".Length);
        rightControlName = KeyPath(moveRightKey).Substring("<Keyboard>/".Length);
        jumpAction = actionMap.AddAction("Jump", InputActionType.Button, KeyPath(jumpKey));
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
        jumpAction.performed += _ =>
        {
            if (!AcceptInput) return;
            jump.Press(Time.time);
            JumpHeld = true;
            jumpReleaseLatch = false;
        };
        jumpAction.canceled += _ =>
        {
            if (!AcceptInput) return;
            jump.Release();
            JumpHeld = false;
            jumpReleaseLatch = true;
        };
        // Dash (kieu Ori): Ctrl hoac chuot phai, RB tay cam. Shift danh cho bam tuong, nen bo qua gia tri LeftShift
        // con luu trong scene; chi them phim dashKey neu nguoi dung da doi sang phim khac.
        var dashAction = actionMap.AddAction("Dash", InputActionType.Button, "<Keyboard>/leftCtrl");
        dashAction.AddBinding("<Keyboard>/rightCtrl");
        if (dashKey != KeyCode.LeftShift && dashKey != KeyCode.LeftControl) dashAction.AddBinding(KeyPath(dashKey));
        dashAction.AddBinding(MousePath(dashMouseButton));
        dashAction.AddBinding("<Gamepad>/rightShoulder");
        dashAction.performed += _ =>
        {
            if (!AcceptInput) return;
            float direction = ReadHorizontal();
            dash.Press(Time.time, direction > .01f ? 1 : direction < -.01f ? -1 : transform.right.x >= 0f ? 1 : -1);
        };
        var attackAction = actionMap.AddAction("Attack", InputActionType.Button, MousePath(attackMouseButton));
        attackAction.AddBinding("<Gamepad>/buttonWest");
        attackAction.performed += _ => { if (AcceptInput) attack.Press(Time.time); };
        for (int i = 0; i < skillKeys.Length; i++)
        {
            int slot = i;
            var skill = actionMap.AddAction("Skill" + i, InputActionType.Button, KeyPath(skillKeys[i]));
            skill.performed += _ =>
            {
                if (!AcceptInput || (InteractionEnabled && skillKeys[slot] == KeyCode.E)) return;
                EnsureSkillBuffers();
                skillAt[slot] = Time.time;
            };
        }
        var switchAction = actionMap.AddAction("SwitchWeapon", InputActionType.Button, KeyPath(switchWeaponKey));
        switchAction.performed += _ => { if (AcceptInput) switchWeaponLatch = true; };
    }

    // Giu KeyCode serialized cu; binding duoc chuyen sang vi tri phim vat ly cua Input System.
    private static string KeyPath(KeyCode code)
    {
        string name = code.ToString();
        if (name.StartsWith("Alpha")) name = name.Substring(5);
        else if (name.StartsWith("Keypad")) name = "numpad" + name.Substring(6);
        else if (code == KeyCode.Return) name = "enter";
        else if (code == KeyCode.LeftControl) name = "leftCtrl";
        else if (code == KeyCode.RightControl) name = "rightCtrl";
        return "<Keyboard>/" + name;
    }

    private static string MousePath(int button)
    {
        switch (button)
        {
            case 0: return "<Mouse>/leftButton";
            case 1: return "<Mouse>/rightButton";
            case 2: return "<Mouse>/middleButton";
            case 3: return "<Mouse>/forwardButton";
            case 4: return "<Mouse>/backButton";
            default: throw new System.ArgumentOutOfRangeException(nameof(button));
        }
    }

    private static bool Held(Keyboard keyboard, string controlName)
    {
        return keyboard != null && keyboard.TryGetChildControl<UnityEngine.InputSystem.Controls.KeyControl>(
            controlName)?.isPressed == true;
    }

    private float ReadHorizontal()
    {
        var keyboard = Keyboard.current;
        bool left = Held(keyboard, leftControlName) || (keyboard != null && keyboard.leftArrowKey.isPressed);
        bool right = Held(keyboard, rightControlName) || (keyboard != null && keyboard.rightArrowKey.isPressed);
        var pad = Gamepad.current;
        if (pad != null)
        {
            float axis = pad.leftStick.x.ReadValue();
            if (pad.dpad.left.isPressed) left = true;
            else if (pad.dpad.right.isPressed) right = true;
            else if (axis < -StickDeadzone) left = true;
            else if (axis > StickDeadzone) right = true;
        }
        return (right ? 1f : 0f) - (left ? 1f : 0f);
    }

    private void Update()
    {
        if (!AcceptInput)
        {
            Clear();
            AnyKeyDown = false;
            return;
        }

        Move = ReadHorizontal();
        JumpHeld = jumpAction.IsPressed();
        AnyKeyDown = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            || (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame));
    }

    private bool Buffered(float pressedAt, float window) => CanReadBuffer && Time.time >= pressedAt && Time.time - pressedAt <= window;

    private void EnsureSkillBuffers()
    {
        if (skillAt != null && skillAt.Length == skillKeys.Length) return;
        skillAt = new float[skillKeys.Length];
        for (int i = 0; i < skillAt.Length; i++) skillAt[i] = None;
    }

    private void Clear()
    {
        jump.Clear();
        dash.Clear();
        attack.Clear();
        EnsureSkillBuffers();
        for (int i = 0; i < skillAt.Length; i++) skillAt[i] = None;
        jumpReleaseLatch = false;
        switchWeaponLatch = false;
        Move = 0f;
        JumpHeld = false;
        AnyKeyDown = false;
    }

    public void ResetInput() => Clear();

    private void OnApplicationFocus(bool focused)
    {
        this.focused = focused;
        if (!focused && !IgnoreFocusLoss) Clear();
    }
}
