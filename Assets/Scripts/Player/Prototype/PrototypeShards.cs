using System;
using UnityEngine;

public enum PrototypeShardId { None = 0, Overcharge = 1, Magnet = 2, Splinter = 3, Vitality = 4, Energy = 5 }

/// <summary>
/// He Spirit Shard (nang cap bi dong, kieu Ori 2): 2 o, gan o menu Pause, luu bang PlayerPrefs. Moi shard chi nam 1 o.
/// Ten lay tu Ori nhung hieu ung la uoc luong cho prototype:
///  Overcharge: Spirit tieu nang luong (Arc, Sentry, Blaze, Spike, Flash) gay them 1 sat thuong.
///  Magnet: vat pham hoi mau bay ve phia Player trong 6 don vi.
///  Splinter: Spirit Arc ban them 2 mui phu (1 sat thuong) toa ra hai ben.
///  Vitality: +1 mau toi da (ap dung khi hoi sinh / ve checkpoint).
///  Energy: +1 o nang luong toi da.
/// </summary>
public sealed class PrototypeShards : MonoBehaviour
{
    public const int SlotCount = 2;
    private const string SaveKey = "DangDangPrototype.Shards";
    private static readonly PrototypeShardId[] Cycle =
    {
        PrototypeShardId.Overcharge, PrototypeShardId.Magnet, PrototypeShardId.Splinter,
        PrototypeShardId.Vitality, PrototypeShardId.Energy, PrototypeShardId.None
    };

    public static PrototypeShards Current { get; private set; }
    private readonly PrototypeShardId[] slots = { PrototypeShardId.None, PrototypeShardId.None };

    public event Action Changed;

    public static bool Has(PrototypeShardId id) => Current != null && Current.IsEquipped(id);
    public static int EnergyDamageBonus => Has(PrototypeShardId.Overcharge) ? 1 : 0;
    public static int VitalityBonus => Has(PrototypeShardId.Vitality) ? 1 : 0;

    public void Initialize()
    {
        Current = this;
        Load();
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    public PrototypeShardId Get(int slot) => slots[slot];
    public bool IsEquipped(PrototypeShardId id) => id != PrototypeShardId.None && (slots[0] == id || slots[1] == id);

    public void CycleSlot(int slot)
    {
        int index = Array.IndexOf(Cycle, slots[slot]);
        PrototypeShardId next = Cycle[(index + 1) % Cycle.Length];
        // Mot shard khong the nam hai o: doi cho voi o kia.
        int other = 1 - slot;
        if (next != PrototypeShardId.None && slots[other] == next) slots[other] = slots[slot];
        slots[slot] = next;
        Save();
        Changed?.Invoke();
    }

    public void ResetDefaults()
    {
        slots[0] = slots[1] = PrototypeShardId.None;
        Save();
        Changed?.Invoke();
    }

    public static string DisplayName(PrototypeShardId id) => id == PrototypeShardId.None ? "(empty)" : id.ToString();

    public static string Description(PrototypeShardId id)
    {
        switch (id)
        {
            case PrototypeShardId.Overcharge: return "+1 damage on energy Spirits";
            case PrototypeShardId.Magnet: return "Heal pickups fly to you";
            case PrototypeShardId.Splinter: return "Arc fires 2 extra arrows";
            case PrototypeShardId.Vitality: return "+1 max HP (on respawn)";
            case PrototypeShardId.Energy: return "+1 energy cell";
            default: return "";
        }
    }

    private void Save()
    {
        try
        {
            PlayerPrefs.SetString(SaveKey, (int)slots[0] + "," + (int)slots[1]);
            PlayerPrefs.Save();
        }
        catch (Exception) { }
    }

    private void Load()
    {
        try
        {
            string[] parts = PlayerPrefs.GetString(SaveKey, "").Split(',');
            int a, b;
            if (parts.Length != SlotCount || !int.TryParse(parts[0], out a) || !int.TryParse(parts[1], out b)) return;
            if (a < 0 || a > 5 || b < 0 || b > 5 || (a != 0 && a == b)) return;
            slots[0] = (PrototypeShardId)a; slots[1] = (PrototypeShardId)b;
        }
        catch (Exception) { }
    }
}
