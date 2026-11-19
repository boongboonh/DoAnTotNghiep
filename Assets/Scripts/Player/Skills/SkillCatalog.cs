using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Danh sach moi skill trong game, tra theo <see cref="SkillId"/>. Nguon duy nhat cho bang ky nang va save.
/// Tao 1 asset tai Resources/Skills/SkillCatalog (Create > Dang Dang > Skill Catalog) va keo cac SkillData vao.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Skill Catalog", fileName = "SkillCatalog")]
public class SkillCatalog : ScriptableObject
{
    public const string ResourcePath = "Skills/SkillCatalog";

    [SerializeField] private List<SkillData> skills = new List<SkillData>();

    private Dictionary<SkillId, SkillData> lookup;

    public IReadOnlyList<SkillData> Skills => skills;

    public SkillData Get(SkillId id)
    {
        if (id == SkillId.None) return null;
        if (lookup == null) Rebuild();
        lookup.TryGetValue(id, out SkillData data);
        return data;
    }

    private void Rebuild()
    {
        lookup = new Dictionary<SkillId, SkillData>(skills.Count);
        foreach (SkillData s in skills)
        {
            if (s == null || s.id == SkillId.None) continue;
            if (lookup.ContainsKey(s.id))
            {
                Debug.LogWarning($"SkillCatalog: trung SkillId {s.id} ({s.name}), bo qua.", this);
                continue;
            }
            lookup.Add(s.id, s);
        }
    }

    private void OnValidate() => lookup = null;
    private void OnEnable() => lookup = null;

    private static SkillCatalog cached;

    /// <summary>Catalog trong Resources, hoac null neu chua tao asset.</summary>
    public static SkillCatalog Load()
    {
        if (cached == null) cached = Resources.Load<SkillCatalog>(ResourcePath);
        return cached;
    }
}
