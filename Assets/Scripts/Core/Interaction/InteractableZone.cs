using UnityEngine;

/// <summary>
/// Lop nen cho moi diem tuong tac trong game (cong dich chuyen, cua, cay, NPC...).
/// Gan len object co Collider2D (isTrigger). Khi Player dung trong vung va bam phim
/// tuong tac thi goi OnInteract.
///
/// Phim duoc doc trong Update (khong doc trong OnTriggerStay2D) de khong bi mat lan bam:
/// OnTriggerStay2D chay trong vong physics, co frame Update co GetKeyDown nhung khong co buoc physics nao.
/// </summary>
public abstract class InteractableZone : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] protected KeyCode interactKey = KeyCode.F;

    private int playerColliderCount;            // dem so collider tag Player dang trong vung
    private Collider2D playerCollider;

    protected bool PlayerInside => playerColliderCount > 0;

    /// <summary>Dieu kien de cho phep tuong tac (vi du: chua kich hoat, chua mo...).</summary>
    protected virtual bool CanInteract => true;

    /// <summary>Player vao vung (goi du CanInteract la gi, class con tu kiem tra).</summary>
    protected virtual void OnPlayerEnter(Collider2D player) { }

    /// <summary>Player roi vung.</summary>
    protected virtual void OnPlayerExit(Collider2D player) { }

    /// <summary>Player bam phim tuong tac khi dang trong vung va CanInteract = true.</summary>
    protected abstract void OnInteract(Collider2D player);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerColliderCount++;
        playerCollider = collision;
        OnPlayerEnter(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerColliderCount = Mathf.Max(0, playerColliderCount - 1);
        if (playerColliderCount == 0) playerCollider = null;
        OnPlayerExit(collision);
    }

    protected virtual void Update()
    {
        if (!PlayerInside || playerCollider == null) return;
        if (!CanInteract) return;
        if (!Input.GetKeyDown(interactKey)) return;

        OnInteract(playerCollider);
    }

    protected virtual void OnDisable()
    {
        // object bi tat khi player dang trong vung => OnTriggerExit2D khong duoc goi
        playerColliderCount = 0;
        playerCollider = null;
    }
}
