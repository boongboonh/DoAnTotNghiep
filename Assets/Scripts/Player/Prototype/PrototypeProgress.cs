using System;
using System.Collections.Generic;
using GF.Save;
using UnityEngine;

/// <summary>Du lieu save rieng cua prototype (khong dung chung khoa voi save game cu).</summary>
[Serializable]
public sealed class PrototypeSaveData
{
    public int version = PrototypeProgress.Version;
    public string checkpointId = "";
    public bool bashUnlocked;
    public List<string> items = new List<string>();
    public List<string> doors = new List<string>();
}

/// <summary>
/// Tien trinh prototype (M6): nang luc Bash, vat pham da lay, cua da mo, checkpoint.
/// Luu bang PlayerPrefs qua khoa rieng, co version; chet/reset phong giu tien trinh, RestartRun xoa.
/// Dinh danh on dinh (chuoi), khong dung ten object hay thu tu.
/// </summary>
public sealed class PrototypeProgress : MonoBehaviour
{
    public const int Version = 1;
    public const string SaveKey = "DangDangPrototype.Save";
    public const string BashOrbId = "bash_orb";
    public const string MaxHealthItemId = "hp_up_1";
    public const string ShortcutDoorId = "shortcut_door";
    public const string TrialClearId = "combat_trial_clear";
    public const string BossDefeatedId = "boss_warden";

    [Tooltip("Chi de debug: mo Bash trong bo nho, KHONG ghi vao save thuong.")]
    [SerializeField] private bool debugUnlockBash;

    private PrototypeSaveData data = new PrototypeSaveData();
    public event Action Changed;

    public bool BashAvailable => debugUnlockBash || data.bashUnlocked;
    public string CheckpointId => data.checkpointId ?? "";
    public int MaxHealthBonus => HasItem(MaxHealthItemId) ? 1 : 0;
    public bool HasItem(string id) => data.items.Contains(id);
    public bool IsDoorOpen(string id) => data.doors.Contains(id);

    /// <summary>Doc save. Sai version, JSON hong hoac khong co save thi dung du lieu rong.</summary>
    public void Load()
    {
        data = new PrototypeSaveData();
        string json = PlayerPrefsSaveSystem.LoadString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var loaded = JsonUtility.FromJson<PrototypeSaveData>(json);
            if (loaded == null || loaded.version != Version) return;
            if (loaded.items == null) loaded.items = new List<string>();
            if (loaded.doors == null) loaded.doors = new List<string>();
            if (loaded.checkpointId == null) loaded.checkpointId = "";
            data = loaded;
        }
        catch (ArgumentException)
        {
            data = new PrototypeSaveData();
        }
    }

    /// <summary>Nhan vat pham mot lan. Tra false neu da nhan roi.</summary>
    public bool CollectItem(string id)
    {
        if (string.IsNullOrEmpty(id) || HasItem(id)) return false;
        data.items.Add(id);
        if (id == BashOrbId) data.bashUnlocked = true;
        Commit();
        return true;
    }

    public bool OpenDoor(string id)
    {
        if (string.IsNullOrEmpty(id) || IsDoorOpen(id)) return false;
        data.doors.Add(id);
        Commit();
        return true;
    }

    public void SetCheckpoint(string id)
    {
        if (data.checkpointId == id) return;
        data.checkpointId = id ?? "";
        Commit();
    }

    public void ClearCheckpoint() => SetCheckpoint("");

    /// <summary>Choi lai toan tuyen: xoa save cua prototype (chi khoa rieng nay).</summary>
    public void ResetAll()
    {
        data = new PrototypeSaveData();
        PlayerPrefsSaveSystem.DeleteKey(SaveKey);
        PlayerPrefsSaveSystem.Flush();
        Changed?.Invoke();
    }

    private void Commit()
    {
        PlayerPrefsSaveSystem.SaveString(SaveKey, JsonUtility.ToJson(data));
        Changed?.Invoke();
    }
}
