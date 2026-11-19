using System;
using UnityEngine;

/// <summary>Vat pham nhan mot lan (Bash hoac +1 mau toi da). Trang thai doc tu PrototypeProgress, nen reset phong khong nhan lai.</summary>
public sealed class PrototypeProgressPickup : MonoBehaviour
{
    private PlayerController player;
    private PrototypeProgress progress;
    private string itemId;
    private string waitingText;
    private string collectedText;
    private SpriteRenderer shape;
    private TextMesh label;
    private Action collected;
    private float messageUntil;
    public string ItemId => itemId;

    public void Initialize(Sprite square, PlayerController owner, PrototypeProgress state, string id, Color color,
        string waiting, string acquired, Action onCollected)
    {
        player = owner; progress = state; itemId = id; waitingText = waiting; collectedText = acquired; collected = onCollected;
        shape = PrototypeGeometry.Rectangle(transform, square, "Pickup", new Vector2(.5f, .5f), Vector2.zero, color, 6);
        shape.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        label = PrototypeGeometry.Label(transform, "Pickup Label", new Vector2(0f, 1.1f), .07f);
        Refresh();
    }

    public void Refresh()
    {
        bool has = progress != null && progress.HasItem(itemId);
        if (shape != null) shape.enabled = !has;
        if (label != null)
        {
            bool showMessage = Time.time < messageUntil;
            label.gameObject.SetActive(!has || showMessage);
            label.text = has ? collectedText : waitingText;
        }
    }

    private void Update()
    {
        if (messageUntil > 0f && Time.time >= messageUntil) { messageUntil = 0f; Refresh(); }
        if (player == null || player.IsDead || progress.HasItem(itemId)) return;
        Vector2 delta = player.Motor.Position - (Vector2)transform.position;
        if (Mathf.Abs(delta.x) > .7f || Mathf.Abs(delta.y) > 1.1f) return;
        if (!progress.CollectItem(itemId)) return;
        messageUntil = Time.time + 4f;
        collected?.Invoke();
        Refresh();
    }
}
