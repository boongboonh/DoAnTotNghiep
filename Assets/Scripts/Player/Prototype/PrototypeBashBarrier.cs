using UnityEngine;

/// <summary>Khoi chi pha duoc bang dan da Bash (melee thuong khong pha duoc: khong co tag muc tieu).</summary>
public sealed class PrototypeBashBarrier : MonoBehaviour
{
    private BoxCollider2D solid;
    private SpriteRenderer shape;
    private TextMesh label;
    public bool Broken { get; private set; }

    public void Initialize(Sprite square, Vector2 size)
    {
        solid = gameObject.AddComponent<BoxCollider2D>();
        solid.size = size;
        shape = PrototypeGeometry.Rectangle(transform, square, "Barrier", size, Vector2.zero,
            new Color(.2f, .75f, .85f), 4);
        label = PrototypeGeometry.Label(transform, "Barrier Label", Vector2.zero, .07f);
        label.text = "BASH\nSHOT";
        label.color = Color.black;
        ResetBarrier();
    }

    public void Break()
    {
        if (Broken) return;
        Broken = true;
        Apply();
    }

    public void ResetBarrier()
    {
        Broken = false;
        Apply();
    }

    private void Apply()
    {
        if (solid != null) solid.enabled = !Broken;
        if (shape != null) shape.enabled = !Broken;
        if (label != null) label.gameObject.SetActive(!Broken);
    }
}
