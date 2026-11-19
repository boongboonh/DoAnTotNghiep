using System;

/// <summary>Trang thai HP thuan C#, dung chung cho Player va quai prototype.</summary>
public sealed class PrototypeHealthState
{
    public int Maximum { get; }
    public int Current { get; private set; }
    public float InvulnerableUntil { get; private set; }
    public bool IsDead => Current == 0;

    public PrototypeHealthState(int maximum)
    {
        Maximum = Math.Max(1, maximum);
        Current = Maximum;
        InvulnerableUntil = float.NegativeInfinity;
    }

    public bool TryDamage(int damage, float now, float invincibility)
    {
        if (damage <= 0 || IsDead || now < InvulnerableUntil) return false;
        Current = Math.Max(0, Current - damage);
        InvulnerableUntil = now + Math.Max(0f, invincibility);
        return true;
    }

    /// <summary>Hoi mau; khong hoi khi da chet hoac day mau. Tra ve so mau thuc su hoi.</summary>
    public int Heal(int amount)
    {
        if (amount <= 0 || IsDead || Current >= Maximum) return 0;
        int healed = Math.Min(amount, Maximum - Current);
        Current += healed;
        return healed;
    }

    public void Reset(float now, float grace = 0f)
    {
        Current = Maximum;
        InvulnerableUntil = now + Math.Max(0f, grace);
    }
}
