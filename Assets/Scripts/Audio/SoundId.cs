/// <summary>Dinh danh cac hieu ung am thanh. Them id moi o CUOI enum de khong lech gia tri da serialize.</summary>
public enum SoundId
{
    None = 0,

    // Player
    PlayerRun, PlayerJump, PlayerDash, PlayerHurt, PlayerDeath, PlayerCheckpoint,
    SwordChop, SwordHit, PlayerShoot, BulletHit,

    // UI
    UIClick, UISelect, UIOpen, UIText,

    // Item / Story
    ItemCollect, ItemUpHPMP, TeleBoxOpen, TeleBoxClose,

    // Door / Trap
    DoorKeyOpen, DoorRockOpen,
}
