using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tao asset mau cho he skill: SkillCatalog + 2 SkillData (Nghe Re, Giu Nhin) + hieu ung mau.
/// Menu: Dang Dang > Skills > Create Sample Assets. Khong ghi de asset da co.
/// Menu "Unlock All (Play Mode)" mo khoa het skill va cong nhua de thu bang bang ky nang.
/// Con so la tam; Giu Nhin chua co hieu ung rieng (dung sat thuong vung tam) cho den khi co SpawnZoneEffect.
/// </summary>
public static class SkillSampleAssets
{
    private const string Root = "Assets/Resources/Skills";
    private const string DataDir = Root + "/Data";

    [MenuItem("Dang Dang/Skills/Create Sample Assets")]
    public static void CreateSampleAssets()
    {
        EnsureFolder("Assets/Resources", "Skills");
        EnsureFolder(Root, "Data");

        AreaDamageEffect ngheReHit = GetOrCreate<AreaDamageEffect>(DataDir + "/NgheRe_Hit.asset", e =>
        {
            e.damage = 2;
            e.offset = new Vector2(0f, 0f);
            e.size = new Vector2(5f, 2.5f);
        });

        AreaDamageEffect ngheReWide = GetOrCreate<AreaDamageEffect>(DataDir + "/NgheRe_Wide.asset", e =>
        {
            e.damage = 1;
            e.offset = new Vector2(0f, 0f);
            e.size = new Vector2(8f, 3f);
        });

        AreaDamageEffect giuNhinHit = GetOrCreate<AreaDamageEffect>(DataDir + "/GiuNhin_Tam.asset", e =>
        {
            e.damage = 1;
            e.offset = new Vector2(2f, 0.3f);
            e.size = new Vector2(4f, 2f);
        });

        SkillData ngheRe = GetOrCreate<SkillData>(DataDir + "/NgheRe.asset", s =>
        {
            s.id = SkillId.NgheRe;
            s.displayName = "Nghe Rễ";
            s.description = "Gõ xuống đất, sóng âm lan theo rễ, gây sát thương quanh người.";
            s.unlockHint = "Nhận ở Dải Rễ, sau khi đánh bại Mẹ Rễ.";
            s.cooldown = 6f;
            s.manaCost = 1;
            s.duration = 0.6f;
            s.poseName = "Cast";
            SetEffects(s, new SkillEffectEntry { time = 0.25f, effect = ngheReHit });

            s.tier2.sapCost = 3;
            s.tier2.cooldownDelta = -1f;
            s.tier2.note = "Hồi chiêu giảm 1 giây.";

            s.tier3Branches[0].sapCost = 6;
            s.tier3Branches[0].branchName = "Sóng Rộng";
            s.tier3Branches[0].note = "Thêm một nhịp sóng lan rộng hơn.";
            s.tier3Branches[0].extraEffects.Add(new SkillEffectEntry { time = 0.45f, effect = ngheReWide });

            s.tier3Branches[1].sapCost = 6;
            s.tier3Branches[1].branchName = "Nhịp Nhanh";
            s.tier3Branches[1].note = "Hồi chiêu giảm thêm 2 giây, niệm nhanh hơn.";
            s.tier3Branches[1].cooldownDelta = -2f;
            s.tier3Branches[1].durationDelta = -0.15f;
        });

        SkillData giuNhin = GetOrCreate<SkillData>(DataDir + "/GiuNhin.asset", s =>
        {
            s.id = SkillId.GiuNhin;
            s.displayName = "Giữ Nhìn";
            s.description = "Tạo vùng ánh nhìn trước mặt. (Bản mẫu: tạm thời chỉ gây sát thương nhẹ.)";
            s.unlockHint = "Nhận ở Đồng Cỏ Nhìn, sau khi đánh bại Gò Mắt.";
            s.cooldown = 8f;
            s.manaCost = 1;
            s.duration = 0.5f;
            s.poseName = "Cast";
            SetEffects(s, new SkillEffectEntry { time = 0.2f, effect = giuNhinHit });

            s.tier2.sapCost = 3;
            s.tier2.cooldownDelta = -1.5f;
            s.tier2.note = "Hồi chiêu giảm 1,5 giây.";

            s.tier3Branches[0].sapCost = 6;
            s.tier3Branches[0].branchName = "Vùng Theo Chân";
            s.tier3Branches[0].note = "Chưa có hiệu ứng (cần SpawnZoneEffect).";
            s.tier3Branches[1].sapCost = 6;
            s.tier3Branches[1].branchName = "Hoa Nở Nhanh";
            s.tier3Branches[1].note = "Chưa có hiệu ứng (cần SpawnZoneEffect).";
        });

        SkillCatalog catalog = GetOrCreate<SkillCatalog>(Root + "/SkillCatalog.asset", c => { });
        var so = new SerializedObject(catalog);
        SerializedProperty list = so.FindProperty("skills");
        AddIfMissing(list, ngheRe);
        AddIfMissing(list, giuNhin);
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = catalog;
        Debug.Log("SkillSampleAssets: da tao catalog va skill mau tai " + Root);
    }

    [MenuItem("Dang Dang/Skills/Unlock All (Play Mode)")]
    public static void UnlockAll()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("SkillSampleAssets: chi dung trong Play Mode.");
            return;
        }

        SkillCatalog catalog = SkillCatalog.Load();
        if (catalog == null)
        {
            Debug.LogWarning("SkillSampleAssets: chua co SkillCatalog, chay Create Sample Assets truoc.");
            return;
        }

        SkillProgressManager pm = SkillProgressManager.Instance;
        foreach (SkillData s in catalog.Skills)
            if (s != null) pm.Unlock(s.id);
        pm.AddSap(20);
        Debug.Log("SkillSampleAssets: da mo khoa skill trong catalog va cong 20 nhua.");
    }

    [MenuItem("Dang Dang/Skills/Delete Skill Save")]
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SkillProgressManager.SaveKey);
        PlayerPrefs.Save();
        Debug.Log("SkillSampleAssets: da xoa save skill.");
    }

    // ---------------- Tien ich ----------------

    private static T GetOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        init(asset);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void SetEffects(SkillData skill, params SkillEffectEntry[] entries)
    {
        var so = new SerializedObject(skill);
        SerializedProperty list = so.FindProperty("effects");
        list.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            SerializedProperty e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("time").floatValue = entries[i].time;
            e.FindPropertyRelative("effect").objectReferenceValue = entries[i].effect;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddIfMissing(SerializedProperty list, Object item)
    {
        for (int i = 0; i < list.arraySize; i++)
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == item) return;
        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }
}
