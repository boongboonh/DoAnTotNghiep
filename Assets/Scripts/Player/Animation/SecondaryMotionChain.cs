using UnityEngine;

/// <summary>
/// Chuyen dong phu (tai, duoi, khan) bang spring-damper, khong can keyframe.
///   - Moi joint la 1 Transform (xuong 2D Animation hoac sprite long nhau). Chi xoay quanh truc Z.
///   - Dau vao: van toc cua anchor (do bang vi tri giua cac frame) doi sang khong gian local cua anchor,
///     nen khi nhan vat quay mat (xoay root 180 do) chuoi van tu dung chieu, khong can xu ly rieng.
///   - Moi joint co stiffness/damping/response rieng; <see cref="followThrough"/> keo joint sau theo joint truoc.
///   - Tu nhan biet Animator/script khac ghi len bone: neu rotation hien tai khac gia tri da ghi lan truoc
///     thi lay no lam goc moi, nen dung chung duoc voi clip animation ma khong bi cong don.
/// Cach dung: gan len 1 GameObject bat ky (vd Tail), keo cac bone vao Joints tu goc den ngon.
/// </summary>
[DefaultExecutionOrder(50)]
public class SecondaryMotionChain : MonoBehaviour
{
    [System.Serializable]
    public class Joint
    {
        public Transform bone;
        [Tooltip("Do cung lo xo. Cang cao cang nhanh ve vi tri nghi.")]
        public float stiffness = 90f;
        [Tooltip("Do tat dao dong. Cang cao cang it lac.")]
        public float damping = 9f;
        [Tooltip("He so nhan dau vao cua joint nay (joint cuoi thuong lon hon).")]
        public float response = 1f;
        [Tooltip("Goc lech toi da (do) so voi goc nghi.")]
        public float maxAngle = 40f;
    }

    [Header("Chuoi")]
    [Tooltip("Dung de do van toc. De trong = parent cua bone dau tien.")]
    [SerializeField] private Transform anchor;
    [SerializeField] private Joint[] joints = new Joint[0];
    [Tooltip("0..1: joint sau chiu them anh huong tu goc cua joint truoc (tao do tre, uon luon).")]
    [SerializeField, Range(0f, 1f)] private float followThrough = 0.4f;

    [Header("Phan ung voi van toc")]
    [Tooltip("Do (goc) tren moi don vi/giay. x = van toc huong truoc/sau, y = len/xuong (local cua anchor). " +
             "Doi dau neu chuoi lech sai chieu.")]
    [SerializeField] private Vector2 velocityResponse = new Vector2(-3.5f, 2f);
    [Tooltip("Lam muot van toc do duoc (lon = it tre).")]
    [SerializeField] private float velocitySmoothing = 20f;
    [Tooltip("Van toc lon hon gia tri nay coi la dich chuyen tuc thoi (teleport), reset chuoi.")]
    [SerializeField] private float teleportSpeed = 60f;

    [Header("Lac nhe khi dung yen")]
    [SerializeField] private float swayAngle = 3f;
    [SerializeField] private float swayHz = 0.5f;
    [Tooltip("Lech pha giua cac joint (don vi chu ky).")]
    [SerializeField] private float swayPhase = 0.15f;
    [Tooltip("Goc xe xuong co dinh (do) do trong luc, nhan voi response.")]
    [SerializeField] private float droop = 0f;

    private const float MaxStep = 1f / 120f;

    private float[] angle;
    private float[] omega;
    private Quaternion[] baseRot;
    private Quaternion[] lastApplied;

    private Vector3 lastAnchorPos;
    private bool hasLast;
    private Vector2 smoothedVelocity;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        int n = joints.Length;
        angle = new float[n];
        omega = new float[n];
        baseRot = new Quaternion[n];
        lastApplied = new Quaternion[n];
        for (int i = 0; i < n; i++)
        {
            if (joints[i] == null || joints[i].bone == null) continue;
            baseRot[i] = joints[i].bone.localRotation;
            lastApplied[i] = baseRot[i];
        }
        if (anchor == null && joints.Length > 0 && joints[0].bone != null)
            anchor = joints[0].bone.parent;
        if (anchor == null) anchor = transform;
        hasLast = false;
        smoothedVelocity = Vector2.zero;
    }

    private void OnEnable()
    {
        hasLast = false;
        smoothedVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        if (baseRot == null) return;
        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null || joints[i].bone == null) continue;
            joints[i].bone.localRotation = baseRot[i];
            lastApplied[i] = baseRot[i];
            angle[i] = 0f;
            omega[i] = 0f;
        }
    }

    /// <summary>Day chuoi 1 luc (goc/giay). Dung cho tiep dat, bi danh, dash. Joint sau nhan manh hon theo response.</summary>
    public void Kick(float angularVelocity)
    {
        if (omega == null) return;
        for (int i = 0; i < omega.Length; i++)
            omega[i] += angularVelocity * joints[i].response;
    }

    /// <summary>Dua chuoi ve trang thai nghi ngay lap tuc (sau teleport, hoi sinh).</summary>
    public void ResetState()
    {
        if (angle == null) return;
        for (int i = 0; i < angle.Length; i++)
        {
            angle[i] = 0f;
            omega[i] = 0f;
        }
        hasLast = false;
        smoothedVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.033f);
        if (dt <= 0f || joints.Length == 0) return;

        Vector2 localVelocity = MeasureLocalVelocity(dt);

        int steps = Mathf.Max(1, Mathf.CeilToInt(dt / MaxStep));
        float h = dt / steps;
        float time = Time.time;

        for (int s = 0; s < steps; s++)
        {
            for (int i = 0; i < joints.Length; i++)
            {
                Joint j = joints[i];
                if (j == null || j.bone == null) continue;

                float input = (localVelocity.x * velocityResponse.x + localVelocity.y * velocityResponse.y) * j.response;
                float sway = Mathf.Sin((time * swayHz + i * swayPhase) * Mathf.PI * 2f) * swayAngle * j.response;
                float target = input + sway + droop * j.response;
                if (i > 0) target += followThrough * angle[i - 1];
                target = Mathf.Clamp(target, -j.maxAngle, j.maxAngle);

                omega[i] += (j.stiffness * (target - angle[i]) - j.damping * omega[i]) * h;
                angle[i] = Mathf.Clamp(angle[i] + omega[i] * h, -j.maxAngle, j.maxAngle);
            }
        }

        for (int i = 0; i < joints.Length; i++)
        {
            Joint j = joints[i];
            if (j == null || j.bone == null) continue;
            Transform t = j.bone;

            // Co ai do (Animator, script khac) vua ghi len bone -> lay lam goc moi.
            if (Quaternion.Angle(t.localRotation, lastApplied[i]) > 0.01f)
                baseRot[i] = t.localRotation;

            Quaternion rot = baseRot[i] * Quaternion.Euler(0f, 0f, angle[i]);
            t.localRotation = rot;
            lastApplied[i] = rot;
        }
    }

    private Vector2 MeasureLocalVelocity(float dt)
    {
        Vector3 pos = anchor.position;
        if (!hasLast)
        {
            lastAnchorPos = pos;
            hasLast = true;
            return Vector2.zero;
        }

        Vector3 worldVelocity = (pos - lastAnchorPos) / dt;
        lastAnchorPos = pos;

        if (worldVelocity.magnitude > teleportSpeed)
        {
            ResetState();
            lastAnchorPos = pos;
            hasLast = true;
            return Vector2.zero;
        }

        // Chi dung rotation (khong dung scale) de khong bi dao dau khi scale am.
        Vector3 local = Quaternion.Inverse(anchor.rotation) * worldVelocity;
        Vector2 raw = new Vector2(local.x, local.y);
        smoothedVelocity = Vector2.Lerp(smoothedVelocity, raw, 1f - Mathf.Exp(-velocitySmoothing * dt));
        return smoothedVelocity;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        velocitySmoothing = Mathf.Max(0.1f, velocitySmoothing);
        teleportSpeed = Mathf.Max(1f, teleportSpeed);
    }
#endif
}
