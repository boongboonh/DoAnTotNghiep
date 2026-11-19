using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundBus { Effect, Music }

[Serializable]
public class MusicTrack
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

[Serializable]
public class MusicPlaylist
{
    public string[] scenes;
    public MusicTrack[] tracks;
    public MusicTrack combat;
    [Min(0f)] public float fadeSeconds = 1f;
}

[Serializable]
public class SoundReplacement
{
    public AudioClip original;
    public AudioClip clip;
}

[Serializable]
public class SoundEntry
{
    public SoundId id;
    public AudioClip[] clips;                    // nhieu clip => chon ngau nhien khong lap lien tiep
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitchMin = 1f;
    [Range(0.5f, 2f)] public float pitchMax = 1f;
    public SoundBus bus = SoundBus.Effect;
    [Tooltip("Khong phat chong len chinh no cho den khi clip truoc phat xong (vd tieng chay)")]
    public bool noOverlap;
    [Tooltip("Khoang cach toi thieu giua 2 lan phat (giay)")]
    public float cooldown;
}

/// <summary>Bang anh xa SoundId -> clip + thong so, va tham chieu AudioMixer. Dat o Resources/Audio/SoundLibrary.</summary>
[CreateAssetMenu(menuName = "Audio/Sound Library", fileName = "SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    public AudioMixer mixer;
    public AudioMixerGroup effectGroup;
    public AudioMixerGroup musicGroup;
    public List<SoundEntry> entries = new List<SoundEntry>();
    [SerializeField] private MusicPlaylist[] musicPlaylists;
    [SerializeField] private SoundReplacement[] soundReplacements;

    private Dictionary<SoundId, SoundEntry> map;
    private Dictionary<AudioClip, AudioClip> replacements;

    public MusicPlaylist GetMusicPlaylist(string sceneName)
    {
        if (musicPlaylists == null) return null;
        foreach (var playlist in musicPlaylists)
            if (playlist?.scenes != null && Array.IndexOf(playlist.scenes, sceneName) >= 0)
                return playlist;
        return null;
    }

    public AudioClip ResolveClip(AudioClip original)
    {
        if (original == null || soundReplacements == null) return original;
        if (replacements == null)
        {
            replacements = new Dictionary<AudioClip, AudioClip>();
            foreach (var entry in soundReplacements)
                if (entry?.original != null && entry.clip != null)
                    replacements[entry.original] = entry.clip;
        }
        return replacements.TryGetValue(original, out var clip) ? clip : original;
    }

    public bool TryGet(SoundId id, out SoundEntry entry)
    {
        if (map == null)
        {
            map = new Dictionary<SoundId, SoundEntry>();
            foreach (var e in entries) if (e != null) map[e.id] = e;
        }
        return map.TryGetValue(id, out entry);
    }

    private void OnValidate()
    {
        map = null;
        replacements = null;
    }
}
