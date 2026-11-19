using UnityEngine;

/// <summary>
/// Hieu ung skill: tao mot vung ton tai <see cref="duration"/> giay (vung anh nhin cua Giu Nhin, song am cua Nghe Re...).
/// Vung tu tao, KHONG can prefab: chi can tuy chon <see cref="visualPrefab"/> de hien thi.
/// Moi nhip tick vung gay sat thuong (neu tickDamage &gt; 0) va bao cho <see cref="IWorldReactive"/> trong vung.
/// Asset dung chung, khong luu trang thai tren asset; trang thai o <see cref="SkillZone"/>.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Skill Effects/Spawn Zone", fileName = "SpawnZoneEffect")]
public class SpawnZoneEffect : SkillEffect
{
    [Tooltip("Tam vung, local cua player (x huong ve phia truoc).")]
    public Vector2 offset = new Vector2(2f, 0.5f);
    public Vector2 size = new Vector2(4f, 3f);
    [Min(0.05f)] public float duration = 4f;
    [Tooltip("Vung di theo player thay vi dung yen tai noi niem.")]
    public bool followPlayer = false;

    [Header("Tick")]
    [Min(0.05f)] public float tickInterval = 0.5f;
    [Tooltip("Sat thuong moi nhip tick. 0 = khong gay sat thuong (chi bao cho vat the phan ung).")]
    public int tickDamage = 0;
    public string[] targetTags = { "Enemy", "Boss" };
    public LayerMask targetMask = ~0;

    [Header("Hien thi")]
    [Tooltip("Prefab hinh anh/hieu ung (con cua vung, tu huy cung vung). Co the de trong.")]
    public GameObject visualPrefab;

    public override void Execute(in SkillContext context)
    {
        Transform player = context.Transform;
        Vector3 pos = player.TransformPoint(offset);

        var go = new GameObject("SkillZone_" + (context.Skill != null ? context.Skill.name : "Zone"));
        go.transform.position = pos;
        if (followPlayer) go.transform.SetParent(player, true);

        if (visualPrefab != null)
        {
            var visual = Instantiate(visualPrefab, pos, Quaternion.identity, go.transform);
            // Prefab hinh anh quay theo huong nhin cua player.
            var scale = visual.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * context.Direction;
            visual.transform.localScale = scale;
        }

        var zone = go.AddComponent<SkillZone>();
        zone.Init(context.Skill, size, duration, tickInterval, tickDamage, targetTags, targetMask);
    }
}
