using UnityEngine;

/// <summary>Diem neo co dinh cho Bash. Dung xong phai roi khoi pham vi moi dung lai duoc.</summary>
public sealed class PrototypeBashAnchor : MonoBehaviour, IBashTarget
{
    private SpriteRenderer shape;
    private bool used;
    public Vector2 BashPosition => transform.position;
    public bool Used => used;

    public void Initialize(Sprite square)
    {
        shape = PrototypeGeometry.Rectangle(transform, square, "Anchor", new Vector2(.55f, .55f), Vector2.zero,
            Color.yellow, 6);
        shape.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Refresh();
    }

    private void OnEnable() => PrototypeBash.Register(this);
    private void OnDisable() => PrototypeBash.Unregister(this);

    public bool CanBash(Vector2 playerPosition, float range)
    {
        if (used && Vector2.Distance(BashPosition, playerPosition) > range)
        {
            used = false;
            Refresh();
        }
        return !used;
    }

    public bool Bash(Vector2 playerLaunchDirection)
    {
        if (used) return false;
        used = true;
        Refresh();
        return true;
    }

    public void ResetAnchor()
    {
        used = false;
        Refresh();
    }

    private void Refresh()
    {
        if (shape != null) shape.color = used ? new Color(.4f, .4f, .3f) : Color.yellow;
    }
}
