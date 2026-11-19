using UnityEngine;

/// <summary>Cua tat: chi mo duoc tu phia ben phai (E). Trang thai mo luu trong PrototypeProgress.</summary>
public sealed class PrototypeProgressDoor : MonoBehaviour
{
    private PlayerController player;
    private PrototypeProgress progress;
    private string doorId;
    private BoxCollider2D solid;
    private SpriteRenderer shape;
    private TextMesh label;
    private Vector2 size;
    public bool IsOpen => progress != null && progress.IsDoorOpen(doorId);

    public void Initialize(Sprite square, Vector2 doorSize, PlayerController owner, PrototypeProgress state, string id)
    {
        player = owner; progress = state; doorId = id; size = doorSize;
        solid = gameObject.AddComponent<BoxCollider2D>();
        solid.size = size;
        shape = PrototypeGeometry.Rectangle(transform, square, "Door", size, Vector2.zero, new Color(.55f, .35f, .2f), 4);
        label = PrototypeGeometry.Label(transform, "Door Label", new Vector2(0f, size.y * .5f + .6f), .07f);
        Refresh();
    }

    public void Refresh()
    {
        bool open = IsOpen;
        if (solid != null) solid.enabled = !open;
        if (shape != null) shape.color = open ? new Color(.3f, .6f, .3f, .25f) : new Color(.55f, .35f, .2f);
        if (label != null) label.text = open ? "SHORTCUT OPEN" : "DOOR";
    }

    public bool TryOpen()
    {
        if (IsOpen || player == null || player.IsDead || player.ControlLocked) return false;
        if (player.Motor.Position.x <= transform.position.x) return false;
        return progress.OpenDoor(doorId);
    }

    private void Update()
    {
        if (IsOpen || player == null || player.IsDead) return;
        Vector2 delta = player.Motor.Position - (Vector2)transform.position;
        bool near = Mathf.Abs(delta.x) < 2.2f && Mathf.Abs(delta.y) < size.y;
        label.text = !near ? "DOOR" : delta.x > 0f ? "E: OPEN SHORTCUT" : "LOCKED\nOpens from the other side";
        if (near && delta.x > 0f && player.Controls.InteractPressed && TryOpen())
        {
            player.Controls.ResetInput();
            Refresh();
        }
    }
}
