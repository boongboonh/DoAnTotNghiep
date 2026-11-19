using UnityEngine;

/// <summary>
/// Phat am thanh player theo su kien cua PlayerController / PlayerMotor, de logic di chuyen khong phai biet
/// ve am thanh. Dung PlayerSounds (AudioManager) hien co. Chay khi Run: PlayerSounds tu khong chong len.
/// </summary>
public class PlayerSoundBinder : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [Tooltip("Tuy chon. Co = phat tieng vung kiem va tieng trung don.")]
    [SerializeField] private PlayerCombat combat;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        if (controller == null) return;
        controller.Motor.Jumped += OnJumped;
        controller.DashStarted += OnDashStarted;
        if (combat != null)
        {
            combat.StepStarted += OnAttackStep;
            combat.HitLanded += OnHitLanded;
        }
    }

    private void OnDisable()
    {
        if (controller == null) return;
        controller.Motor.Jumped -= OnJumped;
        controller.DashStarted -= OnDashStarted;
        if (combat != null)
        {
            combat.StepStarted -= OnAttackStep;
            combat.HitLanded -= OnHitLanded;
        }
    }

    private void Update()
    {
        if (controller.Locomotion == PlayerLocomotion.Run && PlayerSounds.instance != null)
            PlayerSounds.instance.PlayRunAudio();
    }

    private void OnJumped()
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.PlayJumpAudio();
    }

    private void OnAttackStep(AttackStep step, int index)
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.SwordChopAudio();
    }

    private void OnHitLanded(AttackStep step, Collider2D target)
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.SwordHitAudio();
    }

    private void OnDashStarted()
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.PlayDashAudio();
    }
}
