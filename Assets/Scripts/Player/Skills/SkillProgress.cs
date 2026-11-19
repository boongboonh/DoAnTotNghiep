using System;
using System.Collections.Generic;

/// <summary>Tien trinh cua mot skill: da mo khoa chua, bac hien tai, nhanh bac 3 da chon.</summary>
[Serializable]
public class SkillEntryProgress
{
    public int id;              // gia tri cua SkillId
    public bool unlocked;
    public int tier = 1;        // 1..SkillData.MaxTier
    public int branch = -1;     // -1 = chua chon, 0/1 = nhanh bac 3
}

/// <summary>
/// Du lieu save cua he skill (class thuan, serialize bang JsonUtility): tien trinh tung skill,
/// nhua thoi gian, va bo trang bi (SkillId cua tung slot, 0 = trong).
/// </summary>
[Serializable]
public class SkillProgress
{
    public int sap;
    public List<SkillEntryProgress> entries = new List<SkillEntryProgress>();
    public int[] loadout = new int[3];

    public SkillEntryProgress Find(SkillId id)
    {
        int key = (int)id;
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].id == key) return entries[i];
        return null;
    }

    public SkillEntryProgress GetOrCreate(SkillId id)
    {
        SkillEntryProgress e = Find(id);
        if (e == null)
        {
            e = new SkillEntryProgress { id = (int)id };
            entries.Add(e);
        }
        return e;
    }
}
