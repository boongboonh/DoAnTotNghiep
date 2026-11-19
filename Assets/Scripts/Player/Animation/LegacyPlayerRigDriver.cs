using UnityEngine;

/// <summary>Drives the part rig from the current gameplay movement controller.</summary>
[DefaultExecutionOrder(-60)]
[RequireComponent(typeof(PosePlayer))]
public sealed class LegacyPlayerRigDriver : MonoBehaviour
{
    private static readonly int State = Animator.StringToHash("state");

    [SerializeField] private Animator movementAnimator;
    [SerializeField, Min(0.01f)] private float referenceRunSpeed = 7f;

    private PosePlayer posePlayer;
    private Rigidbody2D body;
    private PlayerMove movement;
    private bool wasGrounded;

    private void Awake()
    {
        posePlayer = GetComponent<PosePlayer>();
        body = GetComponentInParent<Rigidbody2D>();
        movement = GetComponentInParent<PlayerMove>();
        BindCrystalWind();
    }

    private void BindCrystalWind()
    {
        SpriteRenderer tail = null;
        Transform hip = null, leftEar = null, rightEar = null;
        foreach (SpriteRenderer part in GetComponentsInChildren<SpriteRenderer>())
        {
            switch (part.name)
            {
                case "Crystal tail": tail = part; break;
                case "Crystal hips": hip = part.transform; break;
                case "Crystal earL": leftEar = part.transform.parent; break;
                case "Crystal earR": rightEar = part.transform.parent; break;
            }
        }
        if (tail != null && tail.enabled && hip != null && leftEar != null && rightEar != null)
            gameObject.AddComponent<CrystalRigWind>().Bind(tail, hip, leftEar, rightEar);
    }

    private void OnEnable()
    {
        wasGrounded = movement != null && movement.IsGround();
    }

    private void Update()
    {
        if (movementAnimator == null) return;

        int state = movementAnimator.GetInteger(State);
        string pose = state switch
        {
            1 => "Run",
            2 => "Jump",
            3 => "Fall",
            4 => "Dash",
            5 => "Bored",
            _ => "Idle"
        };

        if (!posePlayer.SetPose(pose)) posePlayer.SetPose("Idle");
        posePlayer.CycleSpeed = state == 1 && body != null
            ? Mathf.Clamp(Mathf.Abs(body.linearVelocity.x) / referenceRunSpeed, 0.5f, 1.6f)
            : 1f;

        bool grounded = movement != null && movement.IsGround();
        if (grounded && !wasGrounded && pose != "Dash" && pose != "Jump")
            posePlayer.Push("Land", 0.12f);
        wasGrounded = grounded;
    }
}
