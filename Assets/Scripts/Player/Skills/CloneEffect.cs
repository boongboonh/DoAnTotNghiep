using UnityEngine;

/// <summary>
/// Hieu ung skill: tao ban sao mo cua player.
///  - ReplayRecent (Lap Vet): phat lai <see cref="lookback"/> giay gan nhat (di chuyen + don danh) thanh ban sao,
///    sat thuong don danh nhan <see cref="damageScale"/> (mac dinh 60%).
///  - WalkForward (Sai Bong): ban sao di thang <see cref="duration"/> giay theo huong nhin, cham duoc cong tac
///    (bao <see cref="IWorldReactive"/> qua mot <see cref="SkillZone"/> di theo ban sao) va hut su chu y quai
///    (<see cref="ShadowClone.Taunters"/>; quai chua doc danh sach nay).
/// Asset dung chung, khong luu trang thai tren asset.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Skill Effects/Clone", fileName = "CloneEffect")]
public class CloneEffect : SkillEffect
{
    public enum CloneMode { ReplayRecent, WalkForward }

    public CloneMode mode = CloneMode.ReplayRecent;
    public Color tint = new Color(0.6f, 0.8f, 1f, 0.5f);
    public string[] targetTags = { "Enemy", "Boss" };
    public LayerMask targetMask = ~0;

    [Header("ReplayRecent")]
    [Range(0.5f, PlayerRecorder.MaxHistory)] public float lookback = 3f;
    [Range(0f, 2f)] public float damageScale = 0.6f;

    [Header("WalkForward")]
    public float duration = 6f;
    public float walkSpeed = 4f;
    public bool tauntEnemies = true;
    [Tooltip("Ban sao dung lai khi gap layer nay (vd Ground). De trong = di xuyen.")]
    public LayerMask obstacleMask = 0;
    [Tooltip("Vung cham cong tac di theo ban sao.")]
    public Vector2 touchSize = new Vector2(1f, 1.5f);

    public override void Execute(in SkillContext context)
    {
        var recorder = PlayerRecorder.EnsureOn(context.Player);
        if (recorder == null) return;

        if (mode == CloneMode.ReplayRecent) SpawnReplay(recorder);
        else SpawnWalker(recorder, context);
    }

    private void SpawnReplay(PlayerRecorder recorder)
    {
        if (!recorder.TryGetWindow(lookback, out var window, out var attacks)) return;
        var clone = NewClone("ReplayClone");
        clone.StartReplay(window, attacks, recorder.Source, tint, damageScale, targetTags, targetMask);
    }

    private void SpawnWalker(PlayerRecorder recorder, in SkillContext context)
    {
        if (!recorder.TryGetLatest(out var from)) return;
        var clone = NewClone("ShadowWalker");
        clone.StartWalk(from, recorder.Source, tint, new Vector2(context.Direction, 0f), walkSpeed, duration, tauntEnemies, obstacleMask);

        // Vung cham di theo ban sao: bao IWorldReactive (cong tac, hoa...) nhu skill dang tac dong.
        var zoneGo = new GameObject("ShadowTouch");
        zoneGo.transform.SetParent(clone.transform, false);
        var zone = zoneGo.AddComponent<SkillZone>();
        zone.Init(context.Skill, touchSize, duration, 0.2f, 0, targetTags, targetMask);
    }

    private static ShadowClone NewClone(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<SpriteRenderer>();
        return go.AddComponent<ShadowClone>();
    }
}
