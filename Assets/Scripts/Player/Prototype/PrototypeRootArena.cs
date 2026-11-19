using UnityEngine;

/// <summary>
/// Khu boss thu 3 (x 136..172), noi tiep khu Sky: cua tuong mo ra sau khi ha Stormwing. Checkpoint rieng, cong dong khi danh,
/// san phang rong cho cot gai moc len. Tao runtime, khong sua scene.
/// </summary>
public sealed class PrototypeRootArena : MonoBehaviour
{
    public const string CheckpointId = "cp_root";
    public static readonly Vector2 CheckpointSpawn = new Vector2(138.2f, .6f);
    public const float TriggerX = 145f;
    public const float MinX = 140.6f;
    public const float MaxX = 171.2f;

    private PlayerController player;
    private PrototypeRootBoss boss;
    private GameObject gate;
    private SpriteRenderer padShape;
    private TextMesh padLabel;
    private TextMesh status;
    private System.Action<string> onCheckpoint;
    private System.Action onDefeated;
    private bool padActive;

    public PrototypeRootBoss Boss => boss;

    public void Initialize(PlayerController controller, PrototypePlayerHealth health, Sprite square,
        System.Action<string> checkpointActivated, System.Action defeated)
    {
        player = controller; onCheckpoint = checkpointActivated; onDefeated = defeated;
        var ground = new Color(.22f, .3f, .2f);
        Block(square, "Root Arena Floor", new Vector2(36f, 1f), new Vector2(154f, -.65f), ground);
        Block(square, "Root Arena Right Wall", new Vector2(.6f, 16f), new Vector2(172.3f, 6f), ground);
        gate = Block(square, "Root Arena Gate", new Vector2(.6f, 14f), new Vector2(139.8f, 6f), new Color(.45f, .5f, .2f)).gameObject;

        padShape = PrototypeGeometry.Rectangle(transform, square, "Root Checkpoint", new Vector2(.6f, 1.6f), new Vector2(CheckpointSpawn.x, .65f), Color.yellow, 0);
        padLabel = PrototypeGeometry.Label(transform, "Root Checkpoint Label", new Vector2(CheckpointSpawn.x, 2.5f), .075f);
        status = PrototypeGeometry.Label(transform, "Root Arena Label", new Vector2(154f, 3.4f), .07f);

        var g = new GameObject("Gloomroot") { tag = "Enemy" };
        g.transform.SetParent(transform, false);
        Vector2 home = new Vector2(164f, 1.15f);
        g.transform.position = home;
        g.AddComponent<Rigidbody2D>();
        g.AddComponent<BoxCollider2D>();
        boss = g.AddComponent<PrototypeRootBoss>();
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
        padShape.color = padActive ? Color.green : Color.yellow;
        padLabel.text = padActive ? "ROOT CHECKPOINT ACTIVE" : "ROOT CHECKPOINT\nE: Activate";
        status.text = boss.Defeated ? "GLOOMROOT DEFEATED\nGo to the flag"
            : boss.Active ? "GLOOMROOT\nIts shell blocks light hits. Smash it or Bash a seed back to crack it open,\nthen hit the exposed core with anything."
            : "ROOT ARENA >\nCross the line to start the fight";
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
