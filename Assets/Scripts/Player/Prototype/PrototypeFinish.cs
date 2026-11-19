using UnityEngine;

/// <summary>Co dich cua tuyen thu. Chi hoan thanh khi Player song va phong thu cuoi (combat trial) da xong.</summary>
public sealed class PrototypeFinish : MonoBehaviour
{
    private PrototypePlayground arena;
    private PlayerController player;
    private SpriteRenderer flag;
    private TextMesh label;

    public void Initialize(Sprite square, PrototypePlayground owner, PlayerController controller)
    {
        arena = owner; player = controller;
        PrototypeGeometry.Rectangle(transform, square, "Pole", new Vector2(.12f, 2.2f), new Vector2(0f, .95f),
            new Color(.8f, .8f, .8f), 4);
        flag = PrototypeGeometry.Rectangle(transform, square, "Flag", new Vector2(.9f, .55f), new Vector2(.5f, 1.7f),
            Color.gray, 5);
        label = PrototypeGeometry.Label(transform, "Finish Label", new Vector2(0f, 3.1f), .08f);
    }

    private void Update()
    {
        if (arena == null || player == null) return;
        bool ready = arena.CanFinish;
        flag.color = arena.Completed ? Color.green : ready ? Color.yellow : Color.gray;
        label.text = arena.Completed ? "ROUTE COMPLETE"
            : ready ? "FINISH\nTouch the flag" : "FINISH\nDefeat the Warden first";
        if (!ready) return;
        Vector2 delta = player.Motor.Position - (Vector2)transform.position;
        if (Mathf.Abs(delta.x) < .9f && Mathf.Abs(delta.y) < 1.8f) arena.Complete();
    }
}
