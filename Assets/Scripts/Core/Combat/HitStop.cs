using UnityEngine;

/// <summary>Hitstop dung unscaled time, khong thay fixedDeltaTime. Input van duoc buffer trong luc dung.</summary>
namespace DangDang.Combat
{
public sealed class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private float until;
    private float previousScale;
    private bool ownsFreeze;
    public static bool IsActive => instance != null && instance.ownsFreeze;

    private void OnEnable() => instance = this;

    /// <summary>Time scale that will be in effect once any running hitstop ends.</summary>
    public static float BaseScale => instance != null && instance.ownsFreeze ? instance.previousScale : Time.timeScale;

    /// <summary>Changes the gameplay time scale without fighting a running hitstop: the freeze keeps
    /// timeScale at 0 and restores this value when it ends.</summary>
    public static void SetBaseScale(float scale)
    {
        scale = Mathf.Max(0.01f, scale);
        if (instance != null && instance.ownsFreeze) instance.previousScale = scale;
        else Time.timeScale = scale;
    }

    public static void Request(float seconds)
    {
        if (instance == null || seconds <= 0f) return;
        instance.Freeze(seconds);
    }

    private void Freeze(float seconds)
    {
        if (!ownsFreeze)
        {
            if (Time.timeScale <= 0f) return;
            previousScale = Time.timeScale;
            ownsFreeze = true;
            Time.timeScale = 0f;
        }
        until = Mathf.Max(until, Time.unscaledTime + seconds);
    }

    private void Update()
    {
        if (ownsFreeze && Time.unscaledTime >= until) Release();
    }

    public void Release()
    {
        if (ownsFreeze && Time.timeScale == 0f) Time.timeScale = previousScale;
        ownsFreeze = false;
        until = 0f;
    }

    private void OnDisable()
    {
        Release();
        if (instance == this) instance = null;
    }
}
}
