using UnityEngine;

/// <summary>
/// San boss sau cua tat (x 70..100): san, tuong phai, cong dong lai khi tran danh bat dau, checkpoint rieng truoc cua vao.
/// Tao runtime, khong sua scene.
/// </summary>
public sealed class PrototypeBossArena : MonoBehaviour
{
    public const string BossCheckpointId = "cp_boss";
    public static readonly Vector2 CheckpointSpawn = new Vector2(73.6f, .6f);
    public const float TriggerX = 77f;
    public const float MinX = 73f;
    public const float MaxX = 99.2f;

    private PlayerController player;
    private PrototypeBoss boss;
    private GameObject gate;
    private SpriteRenderer padShape;
    private TextMesh padLabel;
    private TextMesh status;
    private System.Action<string> onCheckpoint;
    private System.Action onDefeated;
    private bool padActive;

    public PrototypeBoss Boss => boss;

    public void Initialize(PlayerController controller, PrototypePlayerHealth health, Sprite square,
        System.Action<string> checkpointActivated, System.Action defeated)
    {
        player = controller; onCheckpoint = checkpointActivated; onDefeated = defeated;
        var ground = new Color(.25f, .28f, .3f);
        Block(square, "Boss Arena Floor", new Vector2(30f, 1f), new Vector2(85f, -.65f), ground);
        Block(square, "Boss Arena Right Wall", new Vector2(.6f, 14f), new Vector2(100.3f, 5f), ground);
        gate = Block(square, "Boss Arena Gate", new Vector2(.6f, 12f), new Vector2(71.8f, 5f), new Color(.6f, .2f, .3f)).gameObject;

        padShape = PrototypeGeometry.Rectangle(transform, square, "Boss Checkpoint", new Vector2(.6f, 1.6f), new Vector2(73.6f, .65f), Color.yellow, 0);
        padLabel = PrototypeGeometry.Label(transform, "Boss Checkpoint Label", new Vector2(73.6f, 2.5f), .075f);
        status = PrototypeGeometry.Label(transform, "Boss Arena Label", new Vector2(86f, 4.2f), .07f);

        var g = new GameObject("Warden") { tag = "Enemy" };
        g.transform.SetParent(transform, false);
        g.transform.position = new Vector2(94f, .85f);
        g.AddComponent<Rigidbody2D>();
        g.AddComponent<BoxCollider2D>();
        boss = g.AddComponent<PrototypeBoss>();
        boss.Initialize(player, health, square, TriggerX, MinX, MaxX);
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

    /// <summary>Dong bo hien thi: cong dong khi dang danh; nhan thong bao theo trang thai.</summary>
    public void Refresh()
    {
        if (boss == null) return;
        gate.SetActive(boss.Active);
        padShape.color = padActive ? Color.green : Color.yellow;
        padLabel.text = padActive ? "BOSS CHECKPOINT ACTIVE" : "BOSS CHECKPOINT\nE: Activate";
        status.text = boss.Defeated ? "WARDEN DEFEATED\nGo to the flag"
            : boss.Active ? "WARDEN\nDodge the telegraphs, hit when it recovers.\nBash a purple shot back for big damage"
            : "BOSS ARENA >\nCross the line to start the fight";
    }

    /// <summary>Reset khi Player hoi sinh: boss quay ve diem dau (tru khi da bi ha), cong mo.</summary>
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
        onCheckpoint?.Invoke(BossCheckpointId);
        player.Controls.ResetInput();
        Refresh();
    }
}
