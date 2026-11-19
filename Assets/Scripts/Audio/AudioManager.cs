using GF.Save;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quan ly am thanh toan game (singleton, tu tao khi vao game, song qua cac scene).
///  - Phat SFX theo <see cref="SoundId"/> qua pool AudioSource (khong can AudioSource rieng trong prefab).
///  - Doc <see cref="SoundLibrary"/> o Resources/Audio/SoundLibrary.
///  - Quan ly am luong (PlayerPrefs + AudioMixer), ap dung ngay khi khoi dong.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public const string KeyMusic = "soundBackgroundMix";
    public const string KeyEffect = "soundEffectMix";
    private const int PoolSize = 16;

    private static AudioManager instance;
    private static bool quitting;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null && !quitting) Create();
            return instance;
        }
    }

    private SoundLibrary library;
    public SoundLibrary Library => library;
    private readonly List<AudioSource> pool = new List<AudioSource>();
    private readonly Dictionary<SoundId, float> nextAllowed = new Dictionary<SoundId, float>();
    private readonly Dictionary<SoundId, AudioSource> lastSource = new Dictionary<SoundId, AudioSource>();
    private readonly Dictionary<AudioSource, AudioSource> activeBySource = new Dictionary<AudioSource, AudioSource>();
    private readonly Dictionary<SoundId, int> lastClip = new Dictionary<SoundId, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        quitting = false;

        library = Resources.Load<SoundLibrary>("Audio/SoundLibrary");
        if (library == null) Debug.LogWarning("AudioManager: khong tim thay Resources/Audio/SoundLibrary");

        for (int i = 0; i < PoolSize; i++) pool.Add(CreateSource());
        ApplySavedVolumes();
    }

    private void OnApplicationQuit() { quitting = true; }

    private AudioSource CreateSource()
    {
        // A pooled voice needs its own transform to keep concurrent 3D sounds independent.
        var voice = new GameObject("SFX Voice");
        voice.transform.SetParent(transform, false);
        var s = voice.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        if (library != null) s.outputAudioMixerGroup = library.effectGroup;
        return s;
    }

    // ---------------- SFX ----------------

    /// <summary>Phat hieu ung 2D. Tra ve false neu id chua cau hinh / dang bi chan.</summary>
    public static bool Play(SoundId id)
    {
        var m = Instance;
        return m != null && m.PlayInternal(id, null);
    }

    /// <summary>Phat hieu ung tai vi tri trong the gioi (3D nhe, giam theo khoang cach).</summary>
    public static bool PlayAt(SoundId id, Vector3 position)
    {
        var m = Instance;
        return m != null && m.PlayInternal(id, position);
    }

    /// <summary>Phat mot clip bat ky qua bus Effect (cho script chua chuyen sang SoundId).</summary>
    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        var m = Instance;
        if (clip == null || m == null) return;
        var s = m.GetFreeSource();
        m.Configure(s, m.ResolveClip(clip), volume, 1f, SoundBus.Effect, null);
        s.Play();
    }

    /// <summary>
    /// Phat clip cua mot AudioSource gan san (prefab) qua pool + mixer, dung volume/pitch/3D/nhom mixer cua source do.
    /// noOverlap: bo qua neu lan phat truoc cua chinh source nay chua xong (thay cho `if (src.isPlaying) return`).
    /// Source co loop thi giu cach cu (tu Play neu chua phat).
    /// </summary>
    public static void PlaySource(AudioSource src, bool noOverlap = false)
    {
        if (src == null) return;
        var m = Instance;
        if (m == null || src.clip == null)
        {
            if (!(noOverlap && src.isPlaying)) src.Play();
            return;
        }
        AudioClip clip = m.ResolveClip(src.clip);
        if (src.loop)
        {
            if (src.isPlaying) return;
            if (clip != src.clip) src.clip = clip;
            src.Play();
            return;
        }

        if (noOverlap && m.activeBySource.TryGetValue(src, out var prev) && prev != null && prev.isPlaying
            && prev.clip == clip) return;

        var s = m.GetFreeSource();
        s.clip = clip;
        s.volume = src.volume;
        s.pitch = src.pitch;
        s.loop = false;
        s.outputAudioMixerGroup = src.outputAudioMixerGroup != null
            ? src.outputAudioMixerGroup
            : (m.library != null ? m.library.effectGroup : null);
        s.spatialBlend = src.spatialBlend;
        s.minDistance = src.minDistance;
        s.maxDistance = src.maxDistance;
        s.rolloffMode = src.rolloffMode;
        s.transform.position = src.transform.position;
        s.Play();
        if (m.activeBySource.Count > 64) m.PruneDestroyedSources();
        m.activeBySource[src] = s;
    }

    // bo cac source da bi huy (enemy chet / doi scene) de dictionary khong phinh
    private void PruneDestroyedSources()
    {
        var dead = new List<AudioSource>();
        foreach (var kv in activeBySource) if (kv.Key == null) dead.Add(kv.Key);
        foreach (var k in dead) activeBySource.Remove(k);
    }

    /// <summary>Thay AudioSource.PlayClipAtPoint: cung vi tri nhung di qua mixer (slider Effect co tac dung).</summary>
    public static void PlayClipAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        var m = Instance;
        if (clip == null || m == null) return;
        var s = m.GetFreeSource();
        m.Configure(s, m.ResolveClip(clip), volume, 1f, SoundBus.Effect, position);
        s.Play();
    }

    private bool PlayInternal(SoundId id, Vector3? position)
    {
        if (library == null || id == SoundId.None || !library.TryGet(id, out var e)) return false;
        if (e.clips == null || e.clips.Length == 0) return false;

        float now = Time.unscaledTime;
        if (e.cooldown > 0f && nextAllowed.TryGetValue(id, out float t) && now < t) return false;
        if (e.noOverlap && lastSource.TryGetValue(id, out var prev) && prev != null && prev.isPlaying
            && prev.clip != null && IsFromEntry(prev.clip, e)) return false;

        AudioClip clip = PickClip(id, e);
        if (clip == null) return false;

        var s = GetFreeSource();
        Configure(s, clip, e.volume, Random.Range(e.pitchMin, e.pitchMax), e.bus, position);
        s.Play();

        lastSource[id] = s;
        if (e.cooldown > 0f) nextAllowed[id] = now + e.cooldown;
        return true;
    }

    private AudioClip ResolveClip(AudioClip clip) => library != null ? library.ResolveClip(clip) : clip;

    private static bool IsFromEntry(AudioClip clip, SoundEntry e)
    {
        foreach (var c in e.clips) if (c == clip) return true;
        return false;
    }

    private AudioClip PickClip(SoundId id, SoundEntry e)
    {
        int n = e.clips.Length;
        if (n == 1) return e.clips[0];
        int i = Random.Range(0, n);
        if (lastClip.TryGetValue(id, out int last) && i == last) i = (i + 1) % n;     // khong lap lai clip vua phat
        lastClip[id] = i;
        return e.clips[i];
    }

    private void Configure(AudioSource s, AudioClip clip, float volume, float pitch, SoundBus bus, Vector3? position)
    {
        s.clip = clip;
        s.volume = volume;
        s.pitch = pitch;
        s.loop = false;
        s.outputAudioMixerGroup = library == null ? null : (bus == SoundBus.Music ? library.musicGroup : library.effectGroup);
        if (position.HasValue)
        {
            s.transform.position = position.Value;
            s.spatialBlend = 0.6f;
            s.minDistance = 6f;
            s.maxDistance = 30f;
            s.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            s.spatialBlend = 0f;
        }
    }

    // lay source ranh; het thi cuop source phat lau nhat (hieu ung moi khong bi mat)
    private AudioSource GetFreeSource()
    {
        AudioSource oldest = pool[0];
        foreach (var s in pool)
        {
            if (!s.isPlaying) return s;
            if (s.time > oldest.time) oldest = s;
        }
        oldest.Stop();
        return oldest;
    }

    // ---------------- Volume ----------------

    /// <summary>Doi am luong tuyen tinh 0..1 sang dB (chan 0 de khong ra -vo cung).</summary>
    public static float ToDecibel(float linear) => linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;

    /// <summary>Dat am luong (0..1) cho mot bus; luu PlayerPrefs neu save.</summary>
    public static void SetVolume(SoundBus bus, float linear, bool save = true)
    {
        linear = Mathf.Clamp01(linear);
        string key = bus == SoundBus.Music ? KeyMusic : KeyEffect;
        var m = Instance;
        if (m != null && m.library != null && m.library.mixer != null)
            m.library.mixer.SetFloat(key, ToDecibel(linear));
        if (save) PlayerPrefsSaveSystem.SetFloat(key, linear);
    }

    public static float GetVolume(SoundBus bus)
    {
        return PlayerPrefsSaveSystem.LoadFloat(bus == SoundBus.Music ? KeyMusic : KeyEffect, 1f);
    }

    private void ApplySavedVolumes()
    {
        SetVolume(SoundBus.Music, GetVolume(SoundBus.Music), false);
        SetVolume(SoundBus.Effect, GetVolume(SoundBus.Effect), false);
    }
}
