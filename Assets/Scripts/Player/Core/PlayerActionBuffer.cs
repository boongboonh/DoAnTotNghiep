using UnityEngine;

/// <summary>Mot lenh cho moi hanh dong: bam lai thay lenh cu, consume mot lan, het han tu mat hieu luc.</summary>
public sealed class PlayerActionBuffer
{
    private float pressedAt = float.NegativeInfinity;
    public int Direction { get; private set; }
    public bool Released { get; private set; }

    public void Press(float now, int direction = 0)
    {
        pressedAt = now;
        Direction = direction;
        Released = false;
    }

    public void Release() => Released = true;

    public bool Pending(float now, float duration)
    {
        float age = now - pressedAt;
        return duration >= 0f && age >= 0f && age <= duration;
    }

    public bool Consume(float now, float duration)
    {
        if (!Pending(now, duration)) return false;
        Clear();
        return true;
    }

    public void Clear()
    {
        pressedAt = float.NegativeInfinity;
        Direction = 0;
        Released = false;
    }
}
