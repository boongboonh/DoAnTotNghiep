using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dat tu the cho cac bone tu du lieu <see cref="PoseSet"/> va tron muot giua cac pose.
///   - Pose nen (<see cref="SetPose"/>): idle, run, jump, fall...
///   - Pose tam (<see cref="Push"/>): cham, bi danh, tiep dat. Het gio tu ve pose nen.
///   - Phan chu ky (swing) tao nhip chay/tho bang sin; <see cref="CycleSpeed"/> nhan nhip theo toc do.
///   - Dat len bone theo kieu cong them len goc nghi; neu Animator/script khac ghi len bone thi lay gia tri do
///     lam goc moi, nen khong bi cong don.
/// Khong biet gi ve game: tinh nang chon pose nam o PlayerPoseDriver.
/// </summary>
[DefaultExecutionOrder(-50)]
public class PosePlayer : MonoBehaviour
{
    [System.Serializable]
    public class BoneBinding
    {
        public string id;
        public Transform target;
    }

    private struct BoneState
    {
        public Vector2 position;
        public float rotation;
        public Vector2 scaleAdd;
        public float swingRotation;
        public Vector2 swingPosition;
        public float swingPhase;
        public float stepLift;
    }

    [SerializeField] private PoseSet poseSet;
    [SerializeField] private BoneBinding[] bones = new BoneBinding[0];
    [Tooltip("Ten pose chay khi bat len. De trong = khong co pose nen.")]
    [SerializeField] private string startPose = "Idle";

    /// <summary>Nhan nhip chu ky (vd toc do chay / toc do chuan).</summary>
    public float CycleSpeed { get; set; } = 1f;
    public string CurrentPose => overrideTimer > 0f ? overrideName : baseName;

    private BoneState[] state;
    private Vector3[] basePos, lastPos, baseScale, lastScale;
    private Quaternion[] baseRot, lastRot;

    private PoseEntry basePose;
    private string baseName;
    private PoseEntry overridePose;
    private string overrideName;
    private float overrideTimer;

    private float phase;
    private float cycleHz;
    private bool ready;

    private void Awake()
    {
        int n = bones.Length;
        state = new BoneState[n];
        basePos = new Vector3[n]; lastPos = new Vector3[n];
        baseScale = new Vector3[n]; lastScale = new Vector3[n];
        baseRot = new Quaternion[n]; lastRot = new Quaternion[n];
        for (int i = 0; i < n; i++)
        {
            Transform t = bones[i].target;
            if (t == null) continue;
            basePos[i] = lastPos[i] = t.localPosition;
            baseRot[i] = lastRot[i] = t.localRotation;
            baseScale[i] = lastScale[i] = t.localScale;
        }
        ready = true;
        if (!string.IsNullOrEmpty(startPose)) SetPose(startPose);
    }

    private void OnDisable()
    {
        if (!ready) return;
        for (int i = 0; i < bones.Length; i++)
        {
            Transform t = bones[i].target;
            if (t == null) continue;
            t.localPosition = basePos[i];
            t.localRotation = baseRot[i];
            t.localScale = baseScale[i];
            lastPos[i] = basePos[i];
            lastRot[i] = baseRot[i];
            lastScale[i] = baseScale[i];
            state[i] = default;
        }
    }

    /// <summary>Doi pose nen. Tra ve false neu khong co pose ten do. Goi lai cung ten khong lam gi.</summary>
    public bool SetPose(string poseName)
    {
        if (baseName == poseName && basePose != null) return true;
        PoseEntry p = poseSet != null ? poseSet.Find(poseName) : null;
        if (p == null) return false;
        basePose = p;
        baseName = poseName;
        return true;
    }

    /// <summary>Phu pose tam trong duration giay, sau do tu ve pose nen. Tra ve false neu khong co pose ten do.</summary>
    public bool Push(string poseName, float duration)
    {
        PoseEntry p = poseSet != null ? poseSet.Find(poseName) : null;
        if (p == null) return false;
        overridePose = p;
        overrideName = poseName;
        overrideTimer = duration;
        return true;
    }

    /// <summary>Huy pose tam dang chay.</summary>
    public void ClearPush()
    {
        overrideTimer = 0f;
    }

    private void LateUpdate()
    {
        if (!ready || bones.Length == 0) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        if (overrideTimer > 0f) overrideTimer -= dt;
        PoseEntry active = overrideTimer > 0f ? overridePose : basePose;

        float blend = 1f;
        float targetHz = 0f;
        if (active != null)
        {
            blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, active.blendSpeed) * dt);
            targetHz = active.cycleHz;
        }
        cycleHz = Mathf.Lerp(cycleHz, targetHz, blend);
        phase += cycleHz * CycleSpeed * dt;
        if (phase > 1000f) phase -= 1000f;

        for (int i = 0; i < bones.Length; i++)
        {
            Transform t = bones[i].target;
            if (t == null) continue;

            BoneState target = default;
            if (active != null)
            {
                BonePose bp = FindBone(active, bones[i].id);
                if (bp != null)
                {
                    target.position = bp.position;
                    target.rotation = bp.rotation;
                    target.scaleAdd = bp.scaleAdd;
                    target.swingRotation = bp.swingRotation;
                    target.swingPosition = bp.swingPosition;
                    target.swingPhase = bp.swingPhase;
                    target.stepLift = bp.stepLift;
                }
            }

            ref BoneState s = ref state[i];
            s.position = Vector2.Lerp(s.position, target.position, blend);
            s.rotation = Mathf.Lerp(s.rotation, target.rotation, blend);
            s.scaleAdd = Vector2.Lerp(s.scaleAdd, target.scaleAdd, blend);
            s.swingRotation = Mathf.Lerp(s.swingRotation, target.swingRotation, blend);
            s.swingPosition = Vector2.Lerp(s.swingPosition, target.swingPosition, blend);
            s.swingPhase = Mathf.Lerp(s.swingPhase, target.swingPhase, blend);
            s.stepLift = Mathf.Lerp(s.stepLift, target.stepLift, blend);

            // Co ai do vua ghi len bone -> lay lam goc moi.
            if ((t.localPosition - lastPos[i]).sqrMagnitude > 1e-8f) basePos[i] = t.localPosition;
            if (Quaternion.Angle(t.localRotation, lastRot[i]) > 0.01f) baseRot[i] = t.localRotation;
            if ((t.localScale - lastScale[i]).sqrMagnitude > 1e-8f) baseScale[i] = t.localScale;

            float angle = (phase + s.swingPhase) * Mathf.PI * 2f;
            float swing = Mathf.Sin(angle);
            float lift = Mathf.Max(0f, Mathf.Cos(angle));

            Vector3 pos = basePos[i] + new Vector3(
                s.position.x + s.swingPosition.x * swing,
                s.position.y + s.swingPosition.y * swing + s.stepLift * lift * lift, 0f);
            Quaternion rot = baseRot[i] * Quaternion.Euler(0f, 0f, s.rotation + s.swingRotation * swing);
            Vector3 scl = new Vector3(
                baseScale[i].x * (1f + s.scaleAdd.x),
                baseScale[i].y * (1f + s.scaleAdd.y),
                baseScale[i].z);

            t.localPosition = pos;
            t.localRotation = rot;
            t.localScale = scl;
            lastPos[i] = pos;
            lastRot[i] = rot;
            lastScale[i] = scl;
        }
    }

    private static BonePose FindBone(PoseEntry pose, string id)
    {
        List<BonePose> list = pose.bones;
        for (int i = 0; i < list.Count; i++)
            if (list[i].boneId == id) return list[i];
        return null;
    }
}
