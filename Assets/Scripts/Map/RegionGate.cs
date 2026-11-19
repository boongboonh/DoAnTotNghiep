using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class RegionGate : InteractableZone
{
    [SerializeField] private string destination;
    [SerializeField] private GameObject prompt;

    protected override void OnPlayerEnter(Collider2D player) => prompt.SetActive(true);

    protected override void OnPlayerExit(Collider2D player) => prompt.SetActive(PlayerInside);

    protected override void OnInteract(Collider2D player)
    {
        UpdatedWorld world = FindFirstObjectByType<UpdatedWorld>();
        if (world != null) world.Travel(destination);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (prompt != null) prompt.SetActive(false);
    }
}
