using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundMenuController : MonoBehaviour
{
    [SerializeField, HideInInspector] private List<AudioSource> listSound;
    [SerializeField, HideInInspector] private float fadeTime = 1f;

    private readonly List<MusicTrack> tracks = new List<MusicTrack>();
    private MusicTrack combatTrack;
    private AudioSource background;
    private AudioSource combat;
    private int lastTrack = -1;
    private bool fighting;
    private float fadeSeconds;

    protected virtual AudioSource LegacyCombatSource => null;

    protected virtual void Start()
    {
        var library = AudioManager.Instance?.Library;
        var playlist = library != null ? library.GetMusicPlaylist(gameObject.scene.name) : null;
        fadeSeconds = playlist != null ? playlist.fadeSeconds : fadeTime;

        if (playlist != null)
        {
            if (playlist.tracks != null)
                foreach (var track in playlist.tracks) AddTrack(track);
            combatTrack = playlist.combat;
        }
        else
        {
            if (listSound != null)
                foreach (var source in listSound) AddTrack(ReadTrack(source));
            combatTrack = ReadTrack(LegacyCombatSource);
        }

        var musicGroup = library != null ? library.musicGroup : null;
        if (tracks.Count > 0)
        {
            background = CreateSource();
            background.outputAudioMixerGroup = musicGroup;
            PlayNext(0f);
        }
        if (combatTrack?.clip != null)
        {
            combat = CreateSource();
            combat.outputAudioMixerGroup = musicGroup;
            combat.loop = true;
        }
    }

    protected virtual void Update()
    {
        if (!fighting && background != null && !background.isPlaying) PlayNext(fadeSeconds);
    }

    protected void SetFighting(bool active)
    {
        if (combat == null || fighting == active) return;
        fighting = active;
        if (active)
        {
            FadeOut(background);
            Play(combat, combatTrack, fadeSeconds);
        }
        else
        {
            FadeOut(combat);
            if (background != null) PlayNext(fadeSeconds);
        }
    }

    public void StopSoundCurrent() => FadeOut(background);

    protected virtual void OnDisable()
    {
        Stop(background);
        Stop(combat);
        fighting = false;
    }

    private AudioSource CreateSource()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    private static MusicTrack ReadTrack(AudioSource source)
    {
        return source == null ? null : new MusicTrack { clip = source.clip, volume = source.volume };
    }

    private void AddTrack(MusicTrack track)
    {
        if (track?.clip != null) tracks.Add(track);
    }

    private void PlayNext(float duration)
    {
        int next = Random.Range(0, tracks.Count - (lastTrack >= 0 && tracks.Count > 1 ? 1 : 0));
        if (tracks.Count > 1 && lastTrack >= 0 && next >= lastTrack) next++;
        lastTrack = next;
        Play(background, tracks[next], duration);
    }

    private static void Play(AudioSource source, MusicTrack track, float duration)
    {
        DOTween.Kill(source);
        if (!source.isPlaying || source.clip != track.clip)
        {
            source.Stop();
            source.clip = track.clip;
            source.volume = duration > 0f ? 0f : track.volume;
            source.Play();
        }

        if (duration > 0f) source.DOFade(track.volume, duration).SetUpdate(true);
        else source.volume = track.volume;
    }

    private void FadeOut(AudioSource source)
    {
        if (source == null || !source.isPlaying) return;
        DOTween.Kill(source);
        if (fadeSeconds > 0f)
            source.DOFade(0f, fadeSeconds).SetUpdate(true).OnComplete(source.Stop);
        else
            source.Stop();
    }

    private static void Stop(AudioSource source)
    {
        if (source == null) return;
        DOTween.Kill(source);
        source.Stop();
    }
}
