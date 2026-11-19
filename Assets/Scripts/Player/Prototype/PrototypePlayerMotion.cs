using DG.Tweening;
using UnityEngine;

/// <summary>Animation chi thay doi visual; khong bao gio scale collider hay Rigidbody.</summary>
[RequireComponent(typeof(PlayerController))]
public sealed class PrototypePlayerMotion : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private SpriteRenderer bodyVisual;
    [Header("Squash / Stretch (visual only)")]
    [SerializeField] private Vector2 jumpSquash = new Vector2(1.22f, .76f);
    [SerializeField] private Vector2 landingSquash = new Vector2(1.28f, .72f);
    [SerializeField] private Vector2 airStretch = new Vector2(.85f, 1.2f);
    [SerializeField, Min(.01f)] private float jumpSquashHold = .045f;
    [SerializeField, Min(.01f)] private float jumpSquashRelease = .11f;
    [SerializeField, Min(.01f)] private float landingRecovery = .24f;
    private PlayerController controller;
    private PlayerMotor motor;
    private PlayerCombat combat;
    private PrototypePlayerHealth vitality;
    private Vector3 restScale;
    private Vector3 impulse;
    private Tween pulse;
    private Tween compressionTween;
    private float jumpCompression;
    private float phase;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        motor = GetComponent<PlayerMotor>();
        combat = GetComponent<PlayerCombat>();
        restScale = visual.localScale;
    }

    private void OnEnable()
    {
        motor.Jumped += OnJump;
        motor.Landed += OnLand;
        controller.DashStarted += OnDash;
    }

    private void OnDisable()
    {
        motor.Jumped -= OnJump;
        motor.Landed -= OnLand;
        controller.DashStarted -= OnDash;
        pulse?.Kill();
        ClearJumpCompression();
        impulse = Vector3.zero;
        visual.localScale = restScale;
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
    }

    private void OnJump()
    {
        pulse?.Kill();
        impulse = Vector3.zero;
        ClearJumpCompression();
        jumpCompression = 1f;
        // Nen ngan roi bung ra; khong tri hoan van toc nhay cua motor.
        compressionTween = DOTween.Sequence().AppendInterval(jumpSquashHold)
            .Append(DOVirtual.Float(1f, 0f, jumpSquashRelease, x => jumpCompression = x).SetEase(Ease.InOutSine));
    }

    private void ClearJumpCompression()
    {
        compressionTween?.Kill();
        compressionTween = null;
        jumpCompression = 0f;
    }

    private void OnDash()
    {
        ClearJumpCompression();
        Pulse(new Vector3(0.16f, -0.12f, 0f), 0.2f);
    }
    private void OnLand(float speed)
    {
        ClearJumpCompression();
        if (speed > 1f)
        {
            float impact = Mathf.Lerp(.35f, 1f, Mathf.Clamp01(speed / 14f));
            Pulse(new Vector3(landingSquash.x - 1f, landingSquash.y - 1f, 0f) * impact, landingRecovery);
        }
    }

    private void Pulse(Vector3 value, float duration)
    {
        pulse?.Kill();
        impulse = value;
        pulse = DOVirtual.Vector3(value, Vector3.zero, duration, x => impulse = x).SetEase(Ease.OutCubic);
    }

    private void LateUpdate()
    {
        if (Time.deltaTime <= 0f) return;
        var state = controller.Locomotion;
        float speed = Mathf.Clamp01(Mathf.Abs(motor.Velocity.x) / motor.Config.runSpeed);
        phase += Time.deltaTime * (state == PlayerLocomotion.Run ? 16f * speed : 2.5f);
        Vector3 scale = Vector3.one;
        float tilt = 0f;
        float bob = 0f;
        Color color = new Color(0.18f, 0.75f, 1f);
        if (state == PlayerLocomotion.Idle)
            scale += new Vector3(-0.008f, 0.012f, 0f) * Mathf.Sin(phase);
        else if (state == PlayerLocomotion.Run)
        {
            scale += new Vector3(0.025f, -0.025f, 0f) * Mathf.Cos(phase * 2f) * speed;
            bob = Mathf.Abs(Mathf.Sin(phase)) * 0.035f * speed;
            tilt = -4f * speed;
        }
        else if (state == PlayerLocomotion.Rise)
        {
            float stretch = Mathf.Lerp(.35f, 1f, Mathf.Clamp01(Mathf.Abs(motor.Velocity.y) / motor.Config.jumpVelocity));
            scale += new Vector3(airStretch.x - 1f, airStretch.y - 1f, 0f) * stretch;
            tilt = -3f * speed;
        }
        else if (state == PlayerLocomotion.Fall)
        {
            float stretch = Mathf.Lerp(.35f, 1f, Mathf.Clamp01(Mathf.Abs(motor.Velocity.y) / motor.Config.jumpVelocity));
            scale += new Vector3(airStretch.x - 1f, airStretch.y - 1f, 0f) * stretch;
            color = new Color(0.35f, 0.65f, 1f);
        }
        else if (state == PlayerLocomotion.WallSlide)
        {
            tilt = 10f * motor.WallDirection * (motor.FacingRight ? 1f : -1f);
            scale += new Vector3(.08f, -.06f, 0f);
            color = new Color(.4f, 1f, .75f);
        }
        else if (state == PlayerLocomotion.WallClimb)
        {
            // Bam chat vao tuong: nghieng ve phia tuong, hoi dep; dung mau rieng de nhan ra dang grab.
            tilt = 6f * motor.ClimbSide * (motor.FacingRight ? 1f : -1f);
            scale += new Vector3(.1f, -.08f, 0f);
            color = new Color(1f, .85f, .35f);
        }
        else if (state == PlayerLocomotion.Attack && combat != null && combat.Active)
        {
            bool finisher = combat.StepIndex == combat.ComboStepCount - 1 && combat.ComboStepCount > 1;
            bool backhand = !finisher && combat.StepIndex % 2 == 1;
            float direction = backhand ? -1f : 1f;
            if (combat.Phase == MeleeAttackPhase.Windup)
            {
                tilt = (finisher ? 16f : 9f) * direction;
                scale += finisher ? new Vector3(.1f, -.12f, 0f) : new Vector3(.05f, -.06f, 0f);
                color = new Color(.9f, .8f, .3f);
            }
            else if (combat.Phase == MeleeAttackPhase.Active)
            {
                tilt = (finisher ? -21f : -13f) * direction;
                scale += finisher ? new Vector3(.18f, -.12f, 0f) : new Vector3(.12f, -.08f, 0f);
                color = finisher ? new Color(1f, .45f, 1f) : backhand ? new Color(.25f, .9f, 1f) : new Color(1f, .65f, .25f);
            }
            else
            {
                float recovery = Mathf.InverseLerp(combat.CurrentStep.hitEnd, combat.CurrentStep.duration, combat.Elapsed);
                tilt = Mathf.Lerp((finisher ? -16f : -9f) * direction, 0f, recovery);
                scale += new Vector3(.06f, -.04f, 0f) * (1f - recovery);
            }
            if (combat.Upward) { tilt += 35f; color = Color.Lerp(color, Color.cyan, .3f); }
        }
        else if (state == PlayerLocomotion.Dash)
        {
            scale += new Vector3(0.08f, -0.08f, 0f);
            tilt = -8f;
            color = new Color(0.5f, 1f, 1f);
        }
        else if (state == PlayerLocomotion.Hurt)
        {
            tilt = 12f;
            scale += new Vector3(.1f,-.1f,0f);
            color = new Color(1f,.3f,.3f);
        }
        else if (state == PlayerLocomotion.Dead)
        {
            scale = new Vector3(1.15f,.45f,1f);
        }
        bool locomotion = state == PlayerLocomotion.Idle || state == PlayerLocomotion.Run
            || state == PlayerLocomotion.Rise || state == PlayerLocomotion.Fall;
        if (locomotion)
            scale = Vector3.Lerp(scale, new Vector3(jumpSquash.x, jumpSquash.y, 1f), jumpCompression);
        else ClearJumpCompression();
        scale += impulse;
        visual.localScale = Vector3.Scale(restScale, scale);
        // Giu chan gan day hitbox khi nhun, khong dich collider theo animation.
        // Tren khong giu tam hinh; tren dat giu chan khi ap luc nen xuong.
        float anchor = motor.Grounded ? (scale.y - 1f) * restScale.y * 0.5f : 0f;
        visual.localPosition = new Vector3(0f, anchor + bob, 0f);
        visual.localRotation = Quaternion.Euler(0f, 0f, tilt);
        if (vitality == null) vitality = GetComponent<PrototypePlayerHealth>();
        bodyVisual.color = vitality != null ? vitality.DisplayColor(color) : color;
    }
}

