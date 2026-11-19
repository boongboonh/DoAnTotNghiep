using GF.Save;
using System;
using UnityEngine;

public enum SkillUpgradeResult
{
    Ok,
    NotUnlocked,
    MaxTier,
    NeedBranch,
    InvalidBranch,
    NotEnoughSap
}

/// <summary>
/// Giu tien trinh skill cua nguoi choi: mo khoa, bac, nhanh bac 3, nhua thoi gian, bo trang bi (loadout).
/// Tu tao khi can (khong can dat vao scene), song qua cac scene, luu vao PlayerPrefs moi lan thay doi.
/// UI va PlayerSkills chi doc qua day; khong luu trang thai tien trinh o noi khac.
/// </summary>
public class SkillProgressManager : MonoBehaviour
{
    public const string SaveKey = "SkillProgress";
    public const int SlotCount = 3;
    /// <summary>Nhua can de dat lai nhanh bac 3 cua mot skill (ve bac 2).</summary>
    public const int ResetBranchCost = 3;

    private static SkillProgressManager instance;

    public static SkillProgressManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SkillProgressManager>();
                if (instance == null)
                {
                    var go = new GameObject("SkillProgressManager");
                    instance = go.AddComponent<SkillProgressManager>();
                }
            }
            return instance;
        }
    }

    private SkillProgress data;

    public event Action<SkillId> Unlocked;
    public event Action<SkillId> TierChanged;
    public event Action<int> SapChanged;
    /// <summary>Slot vua doi (da luu). Dung de PlayerSkills / UI cap nhat.</summary>
    public event Action<int> LoadoutChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ---------------- Luu / doc ----------------

    private void Load()
    {
        data = null;
        try
        {
            string json = PlayerPrefsSaveSystem.LoadString(SaveKey, "");
            if (!string.IsNullOrEmpty(json)) data = JsonUtility.FromJson<SkillProgress>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("SkillProgressManager: save loi, tao moi. " + e.Message, this);
        }

        if (data == null) data = new SkillProgress();
        if (data.entries == null) data.entries = new System.Collections.Generic.List<SkillEntryProgress>();
        if (data.loadout == null || data.loadout.Length != SlotCount)
        {
            var fixedLoadout = new int[SlotCount];
            if (data.loadout != null) Array.Copy(data.loadout, fixedLoadout, Mathf.Min(data.loadout.Length, SlotCount));
            data.loadout = fixedLoadout;
        }
    }

    private SkillProgress Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public void Save()
    {
        PlayerPrefsSaveSystem.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefsSaveSystem.Flush();
    }

    /// <summary>Xoa toan bo tien trinh skill (dung khi choi moi).</summary>
    public void DeleteSave()
    {
        PlayerPrefsSaveSystem.DeleteKey(SaveKey);
        data = new SkillProgress();
        data.loadout = new int[SlotCount];
        SapChanged?.Invoke(0);
        for (int i = 0; i < SlotCount; i++) LoadoutChanged?.Invoke(i);
    }

    // ---------------- Nhua thoi gian ----------------

    public int Sap => Data.sap;

    public void AddSap(int amount)
    {
        if (amount == 0) return;
        Data.sap = Mathf.Max(0, Data.sap + amount);
        Save();
        SapChanged?.Invoke(Data.sap);
    }

    // ---------------- Mo khoa va bac ----------------

    public bool IsUnlocked(SkillId id)
    {
        SkillEntryProgress e = Data.Find(id);
        return e != null && e.unlocked;
    }

    /// <summary>0 = chua co, 1..3 = bac hien tai.</summary>
    public int GetTier(SkillId id)
    {
        SkillEntryProgress e = Data.Find(id);
        return e != null && e.unlocked ? Mathf.Clamp(e.tier, 1, SkillData.MaxTier) : 0;
    }

    /// <summary>-1 = chua chon nhanh.</summary>
    public int GetBranch(SkillId id)
    {
        SkillEntryProgress e = Data.Find(id);
        return e != null && e.unlocked ? e.branch : -1;
    }

    /// <summary>Mo khoa skill (nhan manh loi). Neu da co thi khong lam gi. Tra ve true khi vua mo khoa.</summary>
    public bool Unlock(SkillId id)
    {
        if (id == SkillId.None) return false;
        SkillEntryProgress e = Data.GetOrCreate(id);
        if (e.unlocked) return false;

        e.unlocked = true;
        e.tier = 1;
        e.branch = -1;
        Save();
        Unlocked?.Invoke(id);
        return true;
    }

    /// <summary>Thong so thuc te cua skill theo tien trinh. Skill khong co id hoac chua theo doi: coi nhu bac 1.</summary>
    public SkillStats StatsOf(SkillData skill)
    {
        if (skill.id == SkillId.None) return skill.Resolve(1, -1);
        SkillEntryProgress e = Data.Find(skill.id);
        if (e == null || !e.unlocked) return skill.Resolve(1, -1);
        return skill.Resolve(e.tier, e.branch);
    }

    /// <summary>Kiem tra nang cap len bac ke tiep. Len bac 3 can chon nhanh (0 hoac 1).</summary>
    public SkillUpgradeResult CheckUpgrade(SkillData skill, int branch = -1)
    {
        SkillEntryProgress e = skill.id != SkillId.None ? Data.Find(skill.id) : null;
        if (e == null || !e.unlocked) return SkillUpgradeResult.NotUnlocked;
        if (e.tier >= SkillData.MaxTier) return SkillUpgradeResult.MaxTier;

        SkillTier next;
        if (e.tier + 1 == SkillData.MaxTier)
        {
            if (branch < 0) return SkillUpgradeResult.NeedBranch;
            next = skill.GetBranch(branch);
            if (next == null) return SkillUpgradeResult.InvalidBranch;
        }
        else
        {
            next = skill.tier2;
        }

        int cost = next != null ? next.sapCost : 0;
        return Data.sap >= cost ? SkillUpgradeResult.Ok : SkillUpgradeResult.NotEnoughSap;
    }

    /// <summary>Nhua can de nang cap len bac ke tiep, hoac -1 khi khong nang cap duoc.</summary>
    public int UpgradeCost(SkillData skill, int branch = -1)
    {
        SkillEntryProgress e = skill.id != SkillId.None ? Data.Find(skill.id) : null;
        if (e == null || !e.unlocked || e.tier >= SkillData.MaxTier) return -1;
        SkillTier next = e.tier + 1 == SkillData.MaxTier ? skill.GetBranch(branch) : skill.tier2;
        return next != null ? next.sapCost : -1;
    }

    public SkillUpgradeResult TryUpgrade(SkillData skill, int branch = -1)
    {
        SkillUpgradeResult r = CheckUpgrade(skill, branch);
        if (r != SkillUpgradeResult.Ok) return r;

        SkillEntryProgress e = Data.Find(skill.id);
        bool toBranch = e.tier + 1 == SkillData.MaxTier;
        SkillTier next = toBranch ? skill.GetBranch(branch) : skill.tier2;

        Data.sap -= next != null ? next.sapCost : 0;
        e.tier++;
        if (toBranch) e.branch = branch;
        Save();
        SapChanged?.Invoke(Data.sap);
        TierChanged?.Invoke(skill.id);
        return SkillUpgradeResult.Ok;
    }

    /// <summary>Dat lai nhanh bac 3 (ve bac 2, bo chon nhanh), ton <see cref="ResetBranchCost"/> nhua. Khong hoan nhua da tra.</summary>
    public bool TryResetBranch(SkillData skill)
    {
        SkillEntryProgress e = skill.id != SkillId.None ? Data.Find(skill.id) : null;
        if (e == null || !e.unlocked || e.tier < SkillData.MaxTier) return false;
        if (Data.sap < ResetBranchCost) return false;

        Data.sap -= ResetBranchCost;
        e.tier = SkillData.MaxTier - 1;
        e.branch = -1;
        Save();
        SapChanged?.Invoke(Data.sap);
        TierChanged?.Invoke(skill.id);
        return true;
    }

    // ---------------- Bo trang bi ----------------

    public SkillId GetLoadout(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return SkillId.None;
        return (SkillId)Data.loadout[slot];
    }

    /// <summary>Gan skill vao slot. Skill phai da mo khoa; neu dang o slot khac thi go khoi slot do.</summary>
    public bool Equip(int slot, SkillId id)
    {
        if (slot < 0 || slot >= SlotCount) return false;
        if (id == SkillId.None) return Unequip(slot);
        if (!IsUnlocked(id)) return false;

        for (int i = 0; i < SlotCount; i++)
        {
            if (i != slot && Data.loadout[i] == (int)id)
            {
                Data.loadout[i] = 0;
                LoadoutChanged?.Invoke(i);
            }
        }
        Data.loadout[slot] = (int)id;
        Save();
        LoadoutChanged?.Invoke(slot);
        return true;
    }

    public bool Unequip(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return false;
        if (Data.loadout[slot] == 0) return true;
        Data.loadout[slot] = 0;
        Save();
        LoadoutChanged?.Invoke(slot);
        return true;
    }
}
