using UnityEngine;

public class SoundInGameController : SoundMenuController
{
    [SerializeField, HideInInspector] private AudioSource soundFighting;
    protected override AudioSource LegacyCombatSource => soundFighting;

    private EventManager events;
    private bool started;

    protected override void Start()
    {
        base.Start();
        started = true;
        Subscribe();
    }

    private void OnEnable()
    {
        if (started) Subscribe();
    }

    protected override void OnDisable()
    {
        if (events != null)
        {
            events.onMusicFighting -= StartCombatMusic;
            events.offMusicFighting -= StopCombatMusic;
        }
        events = null;
        base.OnDisable();
    }

    private void Subscribe()
    {
        events = EventManager.Instance;
        if (events == null) return;
        events.onMusicFighting += StartCombatMusic;
        events.offMusicFighting += StopCombatMusic;
    }

    private void StartCombatMusic() => SetFighting(true);
    private void StopCombatMusic() => SetFighting(false);
}
