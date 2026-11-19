using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Khu thu Bash tao luc chay o ben phai san thu (khong sua scene): ho sau noi bang ba diem neo,
/// roi toi bai co quai ban tren cao va mot khoi chi pha duoc bang dan da Bash.
/// </summary>
public sealed class PrototypeBashCourse : MonoBehaviour
{
    private readonly List<PrototypeBashAnchor> anchors = new List<PrototypeBashAnchor>();
    private PrototypeBashBarrier barrier;
    private PrototypeSpecialEnemy shooter;
    private TextMesh status;
    public PrototypeBashBarrier Barrier => barrier;
    public PrototypeSpecialEnemy Shooter => shooter;
    public IReadOnlyList<PrototypeBashAnchor> Anchors => anchors;

    private readonly List<PrototypeProgressPickup> pickups = new List<PrototypeProgressPickup>();
    private PrototypeProgressDoor door;
    private TextMesh pitHint;
    private PrototypeProgress progress;
    public PrototypeProgressDoor Door => door;
    public IReadOnlyList<PrototypeProgressPickup> Pickups => pickups;

    public void Initialize(PlayerController player, PrototypePlayerHealth health, Sprite square, PrototypeCombatRoom room,
        PrototypeProgress state, System.Action onProgress)
    {
        progress = state;
        var ground = new Color(.25f, .28f, .3f);
        AddBlock(square, "Bash Floor Start", new Vector2(8f, 1f), new Vector2(27f, -.65f), ground);
        AddBlock(square, "Bash Floor End", new Vector2(20f, 1f), new Vector2(60f, -.65f), ground);

        foreach (var p in new[] { new Vector2(35f, 2.8f), new Vector2(40.5f, 3.4f), new Vector2(46f, 2.8f) })
        {
            var g = new GameObject("Bash Anchor");
            g.transform.SetParent(transform, false);
            g.transform.position = p;
            var anchor = g.AddComponent<PrototypeBashAnchor>();
            anchor.Initialize(square);
            anchors.Add(anchor);
        }

        var gate = new GameObject("Bash Gate");
        gate.transform.SetParent(transform, false);
        gate.transform.position = new Vector2(58f, 2.6f);
        barrier = gate.AddComponent<PrototypeBashBarrier>();
        barrier.Initialize(square, new Vector2(.9f, 5.5f));

        var shooterObject = new GameObject("Bash Shooter") { tag = "Enemy" };
        shooterObject.transform.SetParent(transform, false);
        shooterObject.transform.position = new Vector2(52.5f, 5.6f);
        shooterObject.AddComponent<Rigidbody2D>();
        shooterObject.AddComponent<BoxCollider2D>();
        shooter = shooterObject.AddComponent<PrototypeSpecialEnemy>();
        shooter.Initialize(player, health, square, PrototypeEnemyKind.Shooter, room);

        pitHint = PrototypeGeometry.Label(transform, "Bash Hint", new Vector2(31.5f, 3.2f), .09f);
        status = PrototypeGeometry.Label(transform, "Bash Course End", new Vector2(65f, 4.8f), .09f);

        // M6: nang luc Bash nhan truoc ho (vi tri an toan), cua tat va vat pham mau sau cong.
        AddPickup(square, player, "Bash Orb", new Vector2(25.5f, .5f), PrototypeProgress.BashOrbId, Color.cyan,
            "BASH ORB\nTouch to learn Bash", "BASH LEARNED\nHold R near an anchor", onProgress);
        AddPickup(square, player, "Max HP Pickup", new Vector2(67f, .55f), PrototypeProgress.MaxHealthItemId,
            Color.magenta, "+1 MAX HP", "MAX HP +1", onProgress);
        var doorObject = new GameObject("Shortcut Door");
        doorObject.transform.SetParent(transform, false);
        doorObject.transform.position = new Vector2(63f, 1.45f);
        door = doorObject.AddComponent<PrototypeProgressDoor>();
        door.Initialize(square, new Vector2(.7f, 3.2f), player, progress, PrototypeProgress.ShortcutDoorId);
        ResetCourse();
        RefreshProgress();
    }

    private void AddPickup(Sprite square, PlayerController player, string name, Vector2 position, string id, Color color,
        string waiting, string acquired, System.Action onProgress)
    {
        var g = new GameObject(name);
        g.transform.SetParent(transform, false);
        g.transform.position = position;
        var pickup = g.AddComponent<PrototypeProgressPickup>();
        pickup.Initialize(square, player, progress, id, color, waiting, acquired, onProgress);
        pickups.Add(pickup);
    }

    /// <summary>Dong bo hien thi vat pham, cua va goi y theo tien trinh hien tai.</summary>
    public void RefreshProgress()
    {
        foreach (var pickup in pickups) if (pickup != null) pickup.Refresh();
        if (door != null) door.Refresh();
        if (pitHint != null)
        {
            pitHint.text = progress != null && progress.BashAvailable
                ? "BASH COURSE >\nHold R near yellow anchor/shot, aim with mouse,\nrelease R to launch. Aim AWAY from a shot to return it."
                : "GAP AHEAD: NEEDS BASH\nTake the cyan orb on the left first";
        }
    }

    private void AddBlock(Sprite square, string name, Vector2 size, Vector2 position, Color color)
    {
        var block = PrototypeGeometry.Rectangle(transform, square, name, size, position, color, 0);
        block.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    private void Update()
    {
        if (status != null) status.text = barrier != null && barrier.Broken ? "GATE BROKEN\nBASH COURSE CLEAR" : "BASH COURSE END";
    }

    /// <summary>Bat/tat quai ban cua khu Bash (END-01: ngung tan cong khi hoan thanh).</summary>
    public void SetShooterActive(bool on)
    {
        if (shooter == null) return;
        if (!on) shooter.ResetEnemy();
        shooter.gameObject.SetActive(on);
    }

    public void ResetCourse()
    {
        foreach (var anchor in anchors) if (anchor != null) anchor.ResetAnchor();
        if (barrier != null) barrier.ResetBarrier();
        if (shooter != null) shooter.ResetEnemy();
    }
}
