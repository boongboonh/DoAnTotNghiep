using UnityEngine;

public enum CombatFaction { Player, Enemy }

/// <summary>Optional damage contract with source/faction and accepted-hit feedback.</summary>
public interface ICombatReceiver : IDamageable
{
    CombatFaction Faction { get; }
    bool ReceiveHit(int amount, Vector2 source, Vector2 point, CombatFaction attacker);
}

public static class CombatOcclusion
{
    private static readonly RaycastHit2D[] hits = new RaycastHit2D[64];
    public static bool Blocked(Vector2 from, Vector2 to, Rigidbody2D owner, Rigidbody2D target)
    {
        Vector2 delta = to - from;
        var filter = new ContactFilter2D(); filter.SetLayerMask(1); filter.useTriggers = false;
        int count = Physics2D.Raycast(from, delta.normalized, filter, hits, delta.magnitude);
        for (int i = 0; i < count; i++)
            if (hits[i].collider != null && (owner == null || hits[i].rigidbody != owner)
                && (target == null || hits[i].rigidbody != target)) return true;
        return false;
    }
}
