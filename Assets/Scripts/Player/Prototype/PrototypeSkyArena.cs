using UnityEngine;

/// <summary>
/// Khu boss thu 2 (x 100..136), noi tiep san Warden: cua tuong mo ra sau khi ha Warden. Co checkpoint rieng, cong dong khi
/// danh, san rong 36 don vi de boss bay co cho lao xuong. Tao runtime, khong sua scene.
/// </summary>
public sealed class PrototypeSkyArena : MonoBehaviour
{
    public const string CheckpointId = "cp_sky";
    public static readonly Vector2 CheckpointSpawn = new Vector2(102.6f, .6f);
    public const float TriggerX = 109f;
    public const float MinX = 104.2f;
    public const float MaxX = 135.2f;

    private PlayerController player;
    private PrototypeStormBoss boss;
    private GameObject gate;
    private GameObject exitWall;
    private SpriteRenderer padShape;
    private TextMesh padLabel;
    private TextMesh status;
    private System.Action<string> onCheckpoint;
    private System.Action onDefeated;
    private bool padActive;

    public PrototypeStormBoss Boss => boss;

    public void Initialize(PlayerController controller, PrototypePlayerHealth health, Sprite square,
        System.Action<string> checkpointActivated, System.Action defeated)
    {
        player = controller; onCheckpoint = checkpointActivated; onDefeated = defeated;
        var ground = new Color(.2f, .3f, .34f);
        Block(square, "Sky Arena Floor", new Vector2(36f, 1f), new Vector2(118f, -.65f), ground);
        // Tuong phai la cua ra: mo sau khi ha Stormwing de di tiep sang khu Root.
        exitWall = Block(square, "Sky Arena Right Wall", new Vector2(.6f, 16f), new Vector2(136.3f, 6f), ground).gameObject;
        gate = Block(square, "Sky Arena Gate", new Vector2(.6f, 14f), new Vector2(103.6f, 6f), new Color(.2f, .5f, .6f)).gameObject;

        padShape = PrototypeGeometry.Rectangle(transform, square, "Sky Checkpoint", new Vector2(.6f, 1.6f), new Vector2(CheckpointSpawn.x, .65f), Color.yellow, 0);
        padLabel = PrototypeGeometry.Label(transform, "Sky Checkpoint Label", new Vector2(CheckpointSpawn.x, 2.5f), .075f);
        status = PrototypeGeometry.Label(transform, "Sky Arena Label", new Vector2(118f, 3.2f), .07f);

        var g = new GameObject("Stormwing") { tag = "Enemy" };
        g.transform.SetParent(transform, false);
        Vector2 home = new Vector2(124f, 5f);
        g.transform.position = home;
        g.AddComponent<Rigidbody2D>();
        g.AddComponent<BoxCollider2D>();
        boss = g.AddComponent<PrototypeStormBoss>();
        boss.Initialize(player, health, square, TriggerX, MinX, MaxX, home);
        boss.Engaged += () => Refresh();
        boss.Died += () => { Refresh(); onDefeated?.Invoke(); };
        Refresh();
    }

    private BoxCollider2D Block(Sprite square, string name, Vector2 size, Vector2 position, Color color)
    {
        var visual = PrototypeGeometry.Rectangle(transform, square, name, size, position, color, 0);
        var collider = visual.gameObject.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        return collider;
    }

    public void SetCheckpointActive(bool on)
    {
        padActive = on;
        Refresh();
    }

    public void Refresh()
    {
        if (boss == null) return;
        gate.SetActive(boss.Active);
        exitWall.SetActive(!boss.Defeated);
        padShape.color = padActive ? Color.green : Color.yellow;
        padLabel.text = padActive ? "SKY CHECKPOINT ACTIVE" : "SKY CHECKPOINT\nE: Activate";
        status.text = boss.Defeated ? "STORMWING DEFEATED\nThe east wall is open >"
            : boss.Active ? "STORMWING\nDodge the dive line. It lands after a swoop - strike then.\nArc / Sentry reach it in the air"
            : "SKY ARENA >\nCross the line to start the fight";
    }

    public void ResetArena(bool defeated)
    {
        if (boss == null) return;
        if (defeated) boss.SetDefeated();
        else boss.ResetBoss();
        Refresh();
    }

    private void Update()
    {
        if (player == null || player.IsDead || !player.Controls.InteractPressed) return;
        Vector2 d = player.Motor.Position - (Vector2)padShape.transform.position;
        if (Mathf.Abs(d.x) > .9f || Mathf.Abs(d.y) > 1.6f || !player.Motor.Grounded || boss.Active) return;
        padActive = true;
        onCheckpoint?.Invoke(CheckpointId);
        player.Controls.ResetInput();
        Refresh();
    }
}
