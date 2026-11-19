using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ban sao mo cua player, tao boi <see cref="CloneEffect"/>. Hai che do:
///  - Replay: phat lai vi tri/hinh anh va don danh vai giay truoc (Lap Vet), sat thuong nhan voi he so.
///  - Walk: di thang theo huong cho truoc (Sai Bong), co the hut su chu y cua quai qua <see cref="Taunters"/>.
/// Trang thai nam o object nay, khong nam tren asset effect.
/// </summary>
public class ShadowClone : MonoBehaviour
{
    /// <summary>Cac ban sao dang hut su chu y. Quai goi <see cref="NearestTaunter"/> de chon muc tieu (chua noi vao quai).</summary>
    public static readonly List<ShadowClone> Taunters = new List<ShadowClone>();

    public static ShadowClone NearestTaunter(Vector2 from)
    {
        ShadowClone best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < Taunters.Count; i++)
        {
            if (Taunters[i] == null) continue;
            float d = ((Vector2)Taunters[i].transform.position - from).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = Taunters[i]; }
        }
        return best;
    }

    private enum Mode { Replay, Walk }

    private const float FadeTime = 0.3f;

    private Mode mode;
    private SpriteRenderer sr;
    private Color baseColor;
    private float time;
    private float length;
    private bool taunt;

    // Replay
    private List<PlayerRecorder.Sample> samples;
    private List<PlayerRecorder.AttackRecord> attacks;
    private HitScanner scanner;
    private float damageScale;
    private int index;
    private int scanningAttack = -1;

    // Walk
    private Vector2 direction;
    private float speed;
    private LayerMask obstacleMask;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void StartReplay(List<PlayerRecorder.Sample> window, List<PlayerRecorder.AttackRecord> windowAttacks,
        SpriteRenderer source, Color tint, float damageMultiplier, string[] targetTags, LayerMask targetMask)
    {
        mode = Mode.Replay;
        samples = window;
        attacks = windowAttacks;
        damageScale = damageMultiplier;
        scanner = new HitScanner(targetTags, targetMask);
        length = window[window.Count - 1].time - window[0].time + 0.2f;
        Setup(tint, source);
        Apply(window[0]);
    }

    public void StartWalk(PlayerRecorder.Sample from, SpriteRenderer source, Color tint, Vector2 dir, float walkSpeed,
        float duration, bool tauntEnemies, LayerMask obstacles)
    {
        mode = Mode.Walk;
        direction = dir.normalized;
        speed = walkSpeed;
        length = duration;
        obstacleMask = obstacles;
        taunt = tauntEnemies;
        Setup(tint, source);
        Apply(from);
        if (taunt) Taunters.Add(this);
    }

    private void Setup(Color tint, SpriteRenderer source)
    {
        baseColor = tint;
        sr.color = tint;
        if (source != null)
        {
            sr.sortingLayerID = source.sortingLayerID;
            sr.sortingOrder = source.sortingOrder - 1;
        }
    }

    private void OnDestroy()
    {
        Taunters.Remove(this);
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= length) { Destroy(gameObject); return; }

        if (mode == Mode.Replay) TickReplay();
        else TickWalk();

        float left = length - time;
        var c = baseColor;
        c.a = baseColor.a * (left < FadeTime ? left / FadeTime : 1f);
        sr.color = c;
    }

    // ---------------- Replay ----------------

    private void TickReplay()
    {
        float t0 = samples[0].time;
        while (index + 1 < samples.Count && samples[index + 1].time - t0 <= time) index++;

        var a = samples[index];
        Vector3 pos = a.position;
        if (index + 1 < samples.Count)
        {
            var b = samples[index + 1];
            float span = b.time - a.time;
            if (span > 0.0001f) pos = Vector3.Lerp(a.position, b.position, Mathf.Clamp01((time - (a.time - t0)) / span));
        }
        Apply(a);
        transform.position = pos;

        // Phat lai don danh: quet hitbox trong cua so hit cua tung don, nhan he so sat thuong.
        for (int i = 0; i < attacks.Count; i++)
        {
            var atk = attacks[i];
            float start = atk.time - t0 + atk.hitStart;
            float end = atk.time - t0 + atk.hitEnd;
            if (time < start || time > end) continue;

            if (scanningAttack != i) { scanningAttack = i; scanner.Reset(); }
            Vector2 center = (Vector2)pos + new Vector2(a.facing * atk.offset.x, atk.offset.y);
            int dmg = Mathf.Max(1, Mathf.RoundToInt(atk.damage * damageScale));
            scanner.Scan(center, atk.size, dmg, atk.hitEffect, null);
            break;
        }
    }

    // ---------------- Walk ----------------

    private void TickWalk()
    {
        Vector2 step = direction * (speed * Time.deltaTime);
        if (obstacleMask.value != 0)
        {
            var hit = Physics2D.Raycast(transform.position, direction, step.magnitude + 0.3f, obstacleMask);
            if (hit.collider != null) return; // gap tuong thi dung lai, het gio moi tan
        }
        transform.position += (Vector3)step;
    }

    private void Apply(PlayerRecorder.Sample s)
    {
        sr.sprite = s.sprite;
        sr.flipX = s.flipX;
        sr.flipY = s.flipY;
        transform.rotation = s.rotation;
        transform.localScale = s.scale == Vector3.zero ? Vector3.one : s.scale;
        transform.position = s.position;
    }
}
