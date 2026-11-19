using System;
using UnityEngine;

/// <summary>Plays full-body sprite sequences from player animation state.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerSpriteAnimator : MonoBehaviour
{
    [Serializable]
    private sealed class Sequence
    {
        public string pose;
        public Sprite[] frames;
        public Sprite[] leftFrames;
        [Min(0f)] public float framesPerSecond = 10f;
        public bool loop;
        public bool followMovementSpeed;
    }

    [SerializeField] private PosePlayer posePlayer;
    [SerializeField] private Animator legacyAnimator;
    [SerializeField] private Sprite idle;
    [SerializeField] private Sprite leftIdle;
    [SerializeField] private Sequence[] sequences = Array.Empty<Sequence>();
    [Tooltip("Enable only when the left art matches the corresponding right poses and timing.")]
    [SerializeField] private bool useDedicatedLeftFrames;

    private SpriteRenderer spriteRenderer;
    private PlayerMotor motor;
    private PlayerMove legacyMove;
    private Quaternion restRotation;
    private static readonly int LegacyState = Animator.StringToHash("state");
    private string activePose;
    private Sequence activeSequence;
    private float frameTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        motor = GetComponentInParent<PlayerMotor>();
        if (legacyAnimator != null) legacyMove = GetComponentInParent<PlayerMove>();
        restRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        activePose = null;
        activeSequence = null;
        frameTime = 0f;
        ShowFrame();
    }

    private void OnDisable()
    {
        transform.localRotation = restRotation;
    }

    private void LateUpdate()
    {
        string pose = posePlayer != null ? posePlayer.CurrentPose : GetLegacyPose();
        if (pose != activePose)
        {
            activePose = pose;
            activeSequence = Array.Find(sequences, sequence =>
                sequence != null && sequence.pose == pose && sequence.frames is { Length: > 0 });
            frameTime = 0f;
        }

        ShowFrame();
        if (activeSequence == null) return;

        int frameCount = activeSequence.frames.Length;
        float speed = activeSequence.followMovementSpeed && posePlayer != null ? posePlayer.CycleSpeed : 1f;
        frameTime += Time.deltaTime * activeSequence.framesPerSecond * speed;
        frameTime = activeSequence.loop
            ? Mathf.Repeat(frameTime, frameCount)
            : Mathf.Min(frameTime, frameCount - 1);
    }

    private string GetLegacyPose()
    {
        if (legacyAnimator == null) return "Idle";
        // Matches the values set by PlayerMove on the existing Animator controller.
        switch (legacyAnimator.GetInteger(LegacyState))
        {
            case 1: return "Run";
            case 2: return "Jump";
            case 3: return "Fall";
            case 4: return "Dash";
            default: return "Idle";
        }
    }

    private void ShowFrame()
    {
        bool facingLeft = motor != null ? !motor.FacingRight : legacyMove != null && !legacyMove.Direction;
        Sprite leftSprite = activeSequence == null ? leftIdle : null;
        Sprite rightSprite = idle;
        if (activeSequence != null)
        {
            int index = Mathf.FloorToInt(frameTime);
            Sprite[] leftFrames = activeSequence.leftFrames;
            if (leftFrames != null && index < leftFrames.Length && leftFrames[index] != null)
                leftSprite = leftFrames[index];
            Sprite frame = activeSequence.frames[index];
            if (frame != null) rightSprite = frame;
        }

        bool useLeftArt = useDedicatedLeftFrames && facingLeft && leftSprite != null;
        // Cancel the player's turn on this visual when the art already faces left.
        Quaternion rotation = useLeftArt
            ? Quaternion.Euler(0f, 180f, 0f) * restRotation
            : restRotation;
        // The legacy visual parent has its own facing offset for the old rig.
        if (legacyMove != null && transform.parent == legacyAnimator.transform)
            rotation = Quaternion.Inverse(legacyAnimator.transform.localRotation) * rotation;
        transform.localRotation = rotation;
        spriteRenderer.sprite = useLeftArt ? leftSprite : rightSprite;
    }
}
