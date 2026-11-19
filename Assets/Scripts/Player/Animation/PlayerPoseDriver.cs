using UnityEngine;

/// <summary>
/// Cau noi giua PlayerController va PosePlayer: doi trang thai di chuyen thanh ten pose.
///   Idle -> "Idle" (hoac "Bored" khi dung yen lau), Run -> "Run", Rise -> "Jump", Fall -> "Fall",
///   Dash -> "Dash", Locked -> "Idle". Cham dat manh phu pose "Land" ngan va day cac chuoi chuyen dong phu.
/// Pose nao khong co trong PoseSet thi bo qua (khong bao loi). Ten pose doi duoc o muc Pose names.
/// </summary>
[DefaultExecutionOrder(-60)]
public class PlayerPoseDriver : MonoBehaviour
{
    [SerializeField] private PosePlayer posePlayer;
    [SerializeField] private PlayerController controller;
    [Tooltip("Tuy chon. Co = phu pose cua don danh dang chay.")]
    [SerializeField] private PlayerCombat combat;
    [Tooltip("Tuy chon. Co = phu pose cua skill dang niem.")]
    [SerializeField] private PlayerSkills skills;
    [Tooltip("Cac chuoi tai, duoi, khan de day khi tiep dat / dash.")]
    [SerializeField] private SecondaryMotionChain[] chains = new SecondaryMotionChain[0];

    [Header("Ten pose")]
    [SerializeField] private string idlePose = "Idle";
    [SerializeField] private string boredPose = "Bored";
    [SerializeField] private string runPose = "Run";
    [SerializeField] private string jumpPose = "Jump";
    [SerializeField] private string fallPose = "Fall";
    [SerializeField] private string dashPose = "Dash";
    [SerializeField] private string landPose = "Land";
    [SerializeField] private string hurtPose = "Hurt";

    [Header("Nhip chay")]
    [Tooltip("Toc do ngang tuong ung nhip chay chuan (CycleSpeed = 1).")]
    [SerializeField] private float referenceRunSpeed = 7f;

    [Header("Tiep dat / dash")]
    [SerializeField] private float landPoseTime = 0.12f;
    [Tooltip("Toc do roi toi thieu de tinh la tiep dat manh.")]
    [SerializeField] private float landMinFallSpeed = 4f;
    [Tooltip("Luc day chuoi (goc/giay) khi tiep dat, nhan theo toc do roi / 10.")]
    [SerializeField] private float landKick = 220f;
    [SerializeField] private float dashKick = -260f;

    private void Awake()
    {
        if (controller == null) controller = GetComponentInParent<PlayerController>();
        if (posePlayer == null) posePlayer = GetComponentInChildren<PosePlayer>();
        if (combat == null && controller != null) combat = controller.GetComponent<PlayerCombat>();
        if (skills == null && controller != null) skills = controller.GetComponent<PlayerSkills>();
    }

    private void OnEnable()
    {
        if (controller == null) return;
        controller.Motor.Landed += OnLanded;
        controller.DashStarted += OnDashStarted;
        controller.LocomotionChanged += OnLocomotionChanged;
        if (combat != null) combat.StepStarted += OnAttackStep;
        if (skills != null) skills.SkillStarted += OnSkillStarted;
    }

    private void OnDisable()
    {
        if (controller == null) return;
        controller.Motor.Landed -= OnLanded;
        controller.DashStarted -= OnDashStarted;
        controller.LocomotionChanged -= OnLocomotionChanged;
        if (combat != null) combat.StepStarted -= OnAttackStep;
        if (skills != null) skills.SkillStarted -= OnSkillStarted;
    }

    private void Update()
    {
        if (posePlayer == null || controller == null) return;

        string want;
        switch (controller.Locomotion)
        {
            case PlayerLocomotion.Run: want = runPose; break;
            case PlayerLocomotion.Rise: want = jumpPose; break;
            case PlayerLocomotion.Fall: want = fallPose; break;
            case PlayerLocomotion.Dash: want = dashPose; break;
            default: want = controller.IsBored ? boredPose : idlePose; break;
        }

        // Pose thieu (vd Bored) -> lui ve Idle de nhan vat khong bi giu pose cu.
        if (!posePlayer.SetPose(want)) posePlayer.SetPose(idlePose);

        float speedX = Mathf.Abs(controller.Motor.Velocity.x);
        posePlayer.CycleSpeed = controller.Locomotion == PlayerLocomotion.Run
            ? Mathf.Clamp(speedX / Mathf.Max(0.01f, referenceRunSpeed), 0.5f, 1.6f)
            : 1f;
    }

    private void OnLanded(float fallSpeed)
    {
        if (fallSpeed < landMinFallSpeed) return;
        if (posePlayer != null) posePlayer.Push(landPose, landPoseTime);
        Kick(landKick * Mathf.Clamp01(fallSpeed / 10f));
    }

    private void OnDashStarted() => Kick(dashKick);

    private void OnAttackStep(AttackStep step, int index)
    {
        if (posePlayer != null) posePlayer.Push(step.poseName, step.duration);
    }

    private void OnSkillStarted(SkillData skill, int slot)
    {
        if (posePlayer != null) posePlayer.Push(skill.poseName, skill.duration);
    }

    private void OnLocomotionChanged(PlayerLocomotion from, PlayerLocomotion to)
    {
        if (to != PlayerLocomotion.Hurt || posePlayer == null) return;
        posePlayer.Push(hurtPose, controller.Config.hurtStunTime);
        Kick(landKick);
    }

    private void Kick(float angularVelocity)
    {
        for (int i = 0; i < chains.Length; i++)
            if (chains[i] != null) chains[i].Kick(angularVelocity);
    }
}
