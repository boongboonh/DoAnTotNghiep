using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spirit Smash (kieu Ori 2): bua nang. Nhan F / nut B tay cam: gong bua (dung yen), dap xuong gay sat thuong lon
/// trong vung phia truoc, roi hoi chieu. Khong ton nang luong, doi lai cham va co thoi gian hoi.
/// Bi thuong / dash / Bash trong luc gong thi huy don.
/// </summary>
public sealed class PrototypeSpiritSmash : MonoBehaviour
{
    public const int Damage = 3;
    private const float Windup = .22f, Strike = .08f, Recovery = .26f, Cooldown = .9f;
    private static readonly Vector2 HitSize = new Vector2(2.5f, 1.7f);
    private const float HitReach = 1.35f;

    private enum Phase { None, Windup, Strike, Recovery }

    private PlayerController player;
    private PrototypeSpiritSlots slots;
    private Sprite square;
    private SpriteRenderer handle;
    private SpriteRenderer head;
    private SpriteRenderer wave;
    private Phase phase;
    private float phaseEnd;
    private float cooldownUntil;
    private float waveUntil;
    private int facing = 1;
    private readonly HashSet<ICombatReceiver> hitSet = new HashSet<ICombatReceiver>();

    public bool Active => phase != Phase.None;
    public float CooldownRemaining => Mathf.Max(0f, cooldownUntil - Time.time);
    public int SmashCount { get; private set; }
    public int LastHitCount { get; private set; }

    public void Initialize(PlayerController owner, Sprite shapeSprite)
    {
        player = owner; square = shapeSprite;
        slots = player.GetComponent<PrototypeSpiritSlots>();
        if (slots != null) slots.Changed += Cancel;
        handle = PrototypeGeometry.Rectangle(null, square, "Smash Handle", new Vector2(1.5f, .12f), Vector2.zero, new Color(.7f, .5f, .3f), 18);
        head = PrototypeGeometry.Rectangle(null, square, "Smash Head", new Vector2(.6f, .5f), Vector2.zero, new Color(.85f, .9f, 1f), 19);
        wave = PrototypeGeometry.Rectangle(null, square, "Smash Wave", HitSize, Vector2.zero, new Color(1f, .9f, .6f, .35f), 17);
        SetVisible(false);
        wave.enabled = false;
    }

    private void OnDestroy()
    {
        if (slots != null) slots.Changed -= Cancel;
        if (handle != null) Destroy(handle.gameObject);
        if (head != null) Destroy(head.gameObject);
        if (wave != null) Destroy(wave.gameObject);
    }

    public void Cancel()
    {
        phase = Phase.None;
        SetVisible(false);
    }

    private void SetVisible(bool on)
    {
        if (handle != null) handle.enabled = on;
        if (head != null) head.enabled = on;
    }

    private bool Free()
    {
        if (player == null || player.IsDead || player.ControlLocked) return false;
        switch (player.Locomotion)
        {
            case PlayerLocomotion.Idle:
            case PlayerLocomotion.Run:
            case PlayerLocomotion.Rise:
            case PlayerLocomotion.Fall:
                return true;
            default: return false;
        }
    }

    private void Update()
    {
        if (player == null) return;
        if (wave != null && wave.enabled && Time.time >= waveUntil) wave.enabled = false;

        if (phase == Phase.None)
        {
            if (slots != null && slots.Pressed(SpiritId.Smash) && Free() && Time.time >= cooldownUntil && Time.timeScale > 0f) Begin();
            return;
        }

        // Dash / Bash / bi thuong / khoa dieu khien -> huy don.
        if (!Free() && player.Locomotion != PlayerLocomotion.Attack) { Cancel(); return; }
        if (player.IsDead) { Cancel(); return; }

        // Dung tai cho khi dang dap bua (tren khong thi lo lung nhe).
        player.Motor.SetHorizontalVelocity(0f);
        Vector2 pos = player.Motor.Position;
        float t = phaseEnd - Time.time;

        switch (phase)
        {
            case Phase.Windup:
                {
                    float k = 1f - Mathf.Clamp01(t / Windup);
                    PoseHammer(pos, Mathf.Lerp(40f, 115f, k));
                    if (t <= 0f) { StrikeNow(pos); }
                    break;
                }
            case Phase.Strike:
                PoseHammer(pos, -10f);
                if (t <= 0f) { phase = Phase.Recovery; phaseEnd = Time.time + Recovery; }
                break;
            case Phase.Recovery:
                {
                    float k = 1f - Mathf.Clamp01(t / Recovery);
                    PoseHammer(pos, Mathf.Lerp(-10f, -30f, k));
                    if (t <= 0f) { Cancel(); }
                    break;
                }
        }
    }

    private void Begin()
    {
        facing = player.Motor.FacingRight ? 1 : -1;
        phase = Phase.Windup;
        phaseEnd = Time.time + Windup;
        cooldownUntil = Time.time + Cooldown + Windup + Strike + Recovery;
        hitSet.Clear();
        LastHitCount = 0;
        SetVisible(true);
    }

    /// <summary>Goc 0 = bua nam ngang phia truoc, duong = nang len, am = dap xuong.</summary>
    private void PoseHammer(Vector2 pos, float angleDeg)
    {
        float a = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(a) * facing, Mathf.Sin(a));
        Vector2 origin = pos + Vector2.up * .1f;
        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        handle.transform.SetPositionAndRotation(origin + dir * .75f, Quaternion.Euler(0f, 0f, z));
        head.transform.SetPositionAndRotation(origin + dir * 1.55f, Quaternion.Euler(0f, 0f, z));
    }

    private void StrikeNow(Vector2 pos)
    {
        phase = Phase.Strike;
        phaseEnd = Time.time + Strike;
        SmashCount++;
        Vector2 center = pos + new Vector2(facing * HitReach, .1f);
        var buffer = new Collider2D[24];
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(~0);
        int count = Physics2D.OverlapBox(center, HitSize, 0f, filter, buffer);
        for (int i = 0; i < count; i++)
        {
            var c = buffer[i];
            if (c == null) continue;
            var receiver = c.GetComponentInParent<ICombatReceiver>();
            if (receiver == null || receiver.Faction == CombatFaction.Player || hitSet.Contains(receiver)) continue;
            if (receiver.ReceiveHit(Damage, pos, c.ClosestPoint(center), CombatFaction.Player))
            {
                hitSet.Add(receiver);
                LastHitCount++;
            }
        }
        wave.transform.position = center;
        wave.enabled = true;
        waveUntil = Time.time + .12f;
    }
}
