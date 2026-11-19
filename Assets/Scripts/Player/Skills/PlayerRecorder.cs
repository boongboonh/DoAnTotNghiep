using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ghi lai vai giay gan nhat cua player (vi tri, hinh anh, huong, don danh) de skill "Lap Vet" phat lai thanh ban sao.
/// Tu gan len PlayerController khi scene tai (khong can gan tay), nen lich su da co san truoc khi niem skill.
/// Chi doc du lieu cua player; khong thay doi hanh vi cua player.
/// </summary>
public class PlayerRecorder : MonoBehaviour
{
    public struct Sample
    {
        public float time;
        public Vector3 position;
        public Sprite sprite;
        public Quaternion rotation;
        public Vector3 scale;
        public bool flipX, flipY;
        public int facing;
    }

    public struct AttackRecord
    {
        public float time;        // luc bat dau don (cung truc voi Sample.time)
        public float hitStart, hitEnd;
        public int damage;
        public Vector2 offset, size;   // local cua player, x huong ve phia truoc
        public GameObject hitEffect;
    }

    public const float SampleInterval = 0.04f;
    public const float MaxHistory = 6f;

    private readonly List<Sample> samples = new List<Sample>();
    private readonly List<AttackRecord> attacks = new List<AttackRecord>();

    private PlayerController player;
    private PlayerCombat combat;
    private SpriteRenderer source;
    private float nextSampleTime;

    public SpriteRenderer Source => source;

    // ---------------- Tu gan ----------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var p = FindFirstObjectByType<PlayerController>();
        if (p != null) EnsureOn(p);
    }

    public static PlayerRecorder EnsureOn(PlayerController p)
    {
        if (p == null) return null;
        var r = p.GetComponent<PlayerRecorder>();
        return r != null ? r : p.gameObject.AddComponent<PlayerRecorder>();
    }

    // ---------------- Ghi ----------------

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        combat = player != null && player.Combat != null ? player.Combat : GetComponent<PlayerCombat>();
        if (combat != null) combat.StepStarted += OnStepStarted;
    }

    private void OnDestroy()
    {
        if (combat != null) combat.StepStarted -= OnStepStarted;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || Time.time < nextSampleTime) return;
        nextSampleTime = Time.time + SampleInterval;
        if (source == null) source = ResolveRenderer();

        var s = new Sample
        {
            time = Time.time,
            position = transform.position,
            facing = player != null && player.Motor != null && !player.Motor.FacingRight ? -1 : 1,
        };
        if (source != null)
        {
            var t = source.transform;
            s.sprite = source.sprite;
            s.rotation = t.rotation;
            s.scale = t.lossyScale;
            s.flipX = source.flipX;
            s.flipY = source.flipY;
        }
        else
        {
            s.rotation = transform.rotation;
            s.scale = Vector3.one;
        }
        samples.Add(s);
        Trim();
    }

    private void OnStepStarted(AttackStep step, int index)
    {
        attacks.Add(new AttackRecord
        {
            time = Time.time,
            hitStart = step.hitStart,
            hitEnd = step.hitEnd,
            damage = step.damage,
            offset = step.hitOffset,
            size = step.hitSize,
            hitEffect = step.hitEffect,
        });
    }

    private void Trim()
    {
        float oldest = Time.time - MaxHistory;
        int drop = 0;
        while (drop < samples.Count && samples[drop].time < oldest) drop++;
        if (drop > 0) samples.RemoveRange(0, drop);
        drop = 0;
        while (drop < attacks.Count && attacks[drop].time < oldest) drop++;
        if (drop > 0) attacks.RemoveRange(0, drop);
    }

    private SpriteRenderer ResolveRenderer()
    {
        var animator = GetComponentInChildren<PlayerSpriteAnimator>(true);
        if (animator != null) return animator.GetComponent<SpriteRenderer>();
        foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            if (r.enabled && r.sprite != null) return r;
        return null;
    }

    // ---------------- Doc ----------------

    /// <summary>Lay ban sao (khong chia se bo nho) cua <paramref name="lookback"/> giay gan nhat. False neu chua co gi.</summary>
    public bool TryGetWindow(float lookback, out List<Sample> window, out List<AttackRecord> windowAttacks)
    {
        float from = Time.time - Mathf.Clamp(lookback, SampleInterval, MaxHistory);
        window = new List<Sample>();
        windowAttacks = new List<AttackRecord>();
        for (int i = 0; i < samples.Count; i++)
            if (samples[i].time >= from) window.Add(samples[i]);
        for (int i = 0; i < attacks.Count; i++)
            if (attacks[i].time >= from) windowAttacks.Add(attacks[i]);
        return window.Count >= 2;
    }

    /// <summary>Mau moi nhat (dung cho ban sao di thang).</summary>
    public bool TryGetLatest(out Sample sample)
    {
        if (samples.Count == 0) { sample = default; return false; }
        sample = samples[samples.Count - 1];
        return true;
    }
}
