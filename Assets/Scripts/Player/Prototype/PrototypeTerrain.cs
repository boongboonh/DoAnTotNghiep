using UnityEngine;

/// <summary>Dia hinh thu M3, checkpoint va nguy hiem; tao runtime, khong ghi vao scene.</summary>
[DefaultExecutionOrder(-95)]
public sealed class PrototypeTerrain : MonoBehaviour
{
    private PrototypePlayground arena;
    private PlayerController player;
    private PrototypePlayerHealth health;
    private Rigidbody2D moving;
    private PhysicsMaterial2D movingMaterial;
    private Vector2 movingHome = new Vector2(10f, 4.5f);
    private float startedAt;
    private Collider2D spikes;
    private Collider2D checkpoint;
    private SpriteRenderer checkpointVisual;
    private TextMesh checkpointLabel;
    private Collider2D playerBody;
    public const string MainCheckpointId = "cp_main";
    public bool CheckpointActive { get; private set; }
    public Vector2 CheckpointSpawn => new Vector2(9f, .6f);

    public void Initialize(PrototypePlayground owner, PlayerController controller, PrototypePlayerHealth vitality, Sprite sprite)
    {
        arena = owner; player = controller; health = vitality;
        player.Controls.InteractionEnabled = true;
        playerBody = player.GetComponent<Collider2D>();
        var platform = Block(sprite, "One Way Platform", new Vector2(3f, 2.2f), new Vector2(2.5f, .2f), Color.cyan);
        var effector = platform.gameObject.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true; effector.useOneWayGrouping = true;
        platform.usedByEffector = true;
        Label("Platform Hint", new Vector2(3f, 3f), "ONE WAY\nS / Down + Space: Drop");
        var movingCollider = Block(sprite, "Moving Platform", movingHome, new Vector2(2.5f, .25f), new Color(.65f,.7f,1f));
        // The motor carries its rider explicitly; friction must not add a second horizontal push.
        movingMaterial = new PhysicsMaterial2D("Prototype Moving Platform") { friction = 0f, bounciness = 0f };
        movingCollider.sharedMaterial = movingMaterial;
        moving = movingCollider.gameObject.AddComponent<Rigidbody2D>();
        moving.bodyType = RigidbodyType2D.Kinematic;
        moving.interpolation = RigidbodyInterpolation2D.Interpolate;
        Label("Moving Hint", new Vector2(12f, 5.4f), "MOVING PLATFORM");
        spikes = Block(sprite, "Spikes", new Vector2(5f, .02f), new Vector2(1f, .3f), Color.red);
        spikes.isTrigger = true;
        Label("Spikes Hint", new Vector2(5f, 1f), "SPIKES: DAMAGE");
        checkpoint = Block(sprite, "Checkpoint", new Vector2(9f, .65f), new Vector2(.6f, 1.6f), Color.yellow);
        checkpoint.isTrigger = true;
        checkpointVisual = checkpoint.GetComponent<SpriteRenderer>();
        checkpointLabel = Label("Checkpoint Hint", new Vector2(9f, 2.5f), "CHECKPOINT\nE: Activate");
        Label("Pit Hint", new Vector2(14.5f, -.7f), "PIT: RESPAWN");
        ResetTerrain();
    }

    private BoxCollider2D Block(Sprite sprite, string name, Vector2 position, Vector2 size, Color color)
    {
        var visual = PrototypeGeometry.Rectangle(transform, sprite, name, size, position, color, 0);
        var collider = visual.gameObject.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        return collider;
    }

    private TextMesh Label(string name, Vector2 position, string text)
    {
        var label = PrototypeGeometry.Label(transform, name, position, .075f);
        label.text = text; return label;
    }

    private void FixedUpdate()
    {
        if (moving == null) return;
        float offset = Mathf.PingPong((Time.time - startedAt) * 1.5f, 4f);
        moving.MovePosition(movingHome + Vector2.right * offset);
    }

    private void Update()
    {
        if (player == null || player.IsDead) return;
        if (player.Motor.Position.y < -4f) { arena.ResetRun(); return; }
        if (spikes.bounds.Intersects(playerBody.bounds)) health.TryHit(1, spikes.bounds.center);
        if (player.Controls.InteractPressed) TryActivateCheckpoint();
    }

    public bool TryActivateCheckpoint()
    {
        if (player.IsDead || player.ControlLocked || !checkpoint.bounds.Intersects(playerBody.bounds)) return false;
        if (!player.Motor.Grounded || player.Locomotion == PlayerLocomotion.Hurt) return false;
        RestoreCheckpoint();
        arena.OnCheckpointActivated(MainCheckpointId);
        // E is reserved for interaction in the prototype, not the legacy skill slot.
        player.Controls.ResetInput();
        return true;
    }

    /// <summary>Dat checkpoint o trang thai kich hoat (khi nhan E hoac khi nap save).</summary>
    public void RestoreCheckpoint()
    {
        CheckpointActive = true;
        checkpointVisual.color = Color.green;
        checkpointLabel.text = "CHECKPOINT ACTIVE\nF5: Respawn | Ctrl+F5: Restart";
    }

    public void ResetTerrain()
    {
        startedAt = Time.time;
        if (moving != null)
        {
            moving.position = movingHome;
            moving.transform.position = movingHome;
            moving.linearVelocity = Vector2.zero;
        }
    }

    public void ClearCheckpoint()
    {
        CheckpointActive = false;
        checkpointVisual.color = Color.yellow;
        checkpointLabel.text = "CHECKPOINT\nE: Activate";
    }

    private void OnDestroy()
    {
        if (player != null && player.Controls != null) player.Controls.InteractionEnabled = false;
        if (movingMaterial != null) Destroy(movingMaterial);
    }
}
