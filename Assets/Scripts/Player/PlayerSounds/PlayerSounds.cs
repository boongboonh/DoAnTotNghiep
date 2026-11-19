using UnityEngine;

/// <summary>
/// Lop mong cho am thanh player: goi AudioManager (SoundLibrary). Neu id chua cau hinh trong library thi
/// dung AudioSource cu gan trong scene (de khong mat tieng khi chua chuyen xong). API giu nguyen.
/// </summary>
public class PlayerSounds : MonoBehaviour
{
    public static PlayerSounds instance;

    [Header("Fallback (AudioSource cu, chi dung khi SoundLibrary thieu id)")]
    [SerializeField] private AudioSource runAudio;
    [SerializeField] private AudioSource jumpAudio;
    [SerializeField] private AudioSource dashAudio;
    [SerializeField] private AudioSource swordChop;
    [SerializeField] private AudioSource swordHit;
    [SerializeField] private AudioSource shoot;
    [SerializeField] private AudioSource bulletHit;
    [SerializeField] private AudioSource checkpointSound;
    [SerializeField] private AudioSource hurtPlayer;
    [SerializeField] private AudioSource deathPlayer;

    private void Awake()
    {
        instance = this;
    }

    private static void Play(SoundId id, AudioSource fallback, bool onlyIfIdle = false)
    {
        if (AudioManager.Play(id)) return;
        if (fallback == null) return;
        if (onlyIfIdle && fallback.isPlaying) return;
        fallback.Play();
    }

    public void PlayRunAudio() => Play(SoundId.PlayerRun, runAudio, true);        // khong chong len khi dang phat
    public void PlayJumpAudio() => Play(SoundId.PlayerJump, jumpAudio);
    public void PlayDashAudio() => Play(SoundId.PlayerDash, dashAudio);
    public void SwordChopAudio() => Play(SoundId.SwordChop, swordChop);
    public void SwordHitAudio() => Play(SoundId.SwordHit, swordHit);
    public void ShootAudio() => Play(SoundId.PlayerShoot, shoot);
    public void BulletHitAudio() => Play(SoundId.BulletHit, bulletHit);
    public void CheckpointSoundAudio() => Play(SoundId.PlayerCheckpoint, checkpointSound);
    public void HurtPlayerAudio() => Play(SoundId.PlayerHurt, hurtPlayer);
    public void DeathPlayerAudio() => Play(SoundId.PlayerDeath, deathPlayer);
}
