using System;
using UnityEngine;

public enum SpiritId { None = 0, Arc = 1, Smash = 2, Sentry = 3 }

/// <summary>
/// He o Spirit (kieu Ori 2): 2 o gan vao 2 nut (o 1: Q / LT, o 2: F / B). Moi Spirit nam toi da 1 o;
/// doi Spirit o menu Pause. Spirit Edge (danh thuong) luon co san, khong chiem o. Luu bang PlayerPrefs.
/// </summary>
public sealed class PrototypeSpiritSlots : MonoBehaviour
{
    public const int SlotCount = 2;
    private const string SaveKey = "DangDangPrototype.Slots";
    private static readonly SpiritId[] Cycle = { SpiritId.Arc, SpiritId.Smash, SpiritId.Sentry, SpiritId.None };

    private PlayerInputReader controls;
    private readonly SpiritId[] slots = { SpiritId.Arc, SpiritId.Smash };

    /// <summary>Phat khi gan lai o: Spirit dang ngam/gong phai huy.</summary>
    public event Action Changed;

    public void Initialize(PlayerInputReader reader)
    {
        controls = reader;
        Load();
    }

    public SpiritId Get(int slot) => slots[slot];
    public bool IsEquipped(SpiritId id) => id != SpiritId.None && (slots[0] == id || slots[1] == id);

    public bool Pressed(SpiritId id)
    {
        if (controls == null) return false;
        return slots[0] == id && controls.Slot1Pressed || slots[1] == id && controls.Slot2Pressed;
    }

    public bool Held(SpiritId id)
    {
        if (controls == null) return false;
        return slots[0] == id && controls.Slot1Held || slots[1] == id && controls.Slot2Held;
    }

    /// <summary>Doi o sang Spirit ke tiep (Arc, Smash, Trong). Neu Spirit do dang o o kia thi hai o doi cho.</summary>
    public void CycleSlot(int slot)
    {
        int index = Array.IndexOf(Cycle, slots[slot]);
        SpiritId next = Cycle[(index + 1) % Cycle.Length];
        int other = 1 - slot;
        if (next != SpiritId.None && slots[other] == next) slots[other] = slots[slot];
        slots[slot] = next;
        Save();
        Changed?.Invoke();
    }

    public void ResetDefaults()
    {
        slots[0] = SpiritId.Arc; slots[1] = SpiritId.Smash;
        Save();
        Changed?.Invoke();
    }

    public static string DisplayName(SpiritId id)
    {
        switch (id)
        {
            case SpiritId.Arc: return "Spirit Arc";
            case SpiritId.Smash: return "Spirit Smash";
            case SpiritId.Sentry: return "Spirit Sentry";
            default: return "(empty)";
        }
    }

    public static string KeyName(int slot, bool pad) => slot == 0 ? (pad ? "LT" : "Q") : (pad ? "B" : "F");

    /// <summary>Doan chu cho thanh huong dan phim: "Q Arc   F Smash".</summary>
    public string Legend(bool pad)
    {
        string text = "";
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == SpiritId.None) continue;
            text += KeyName(i, pad) + " " + slots[i].ToString() + "   ";
        }
        return text;
    }

    /// <summary>Phim cua mot Spirit ("Q/LT") hoac "--" neu chua gan.</summary>
    public string KeyHint(SpiritId id)
    {
        for (int i = 0; i < SlotCount; i++)
            if (slots[i] == id) return KeyName(i, false) + "/" + KeyName(i, true);
        return "--";
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
            string raw = PlayerPrefs.GetString(SaveKey, "");
            string[] parts = raw.Split(',');
            if (parts.Length != SlotCount) return;
            int a, b;
            if (!int.TryParse(parts[0], out a) || !int.TryParse(parts[1], out b)) return;
            if (a < 0 || a > 3 || b < 0 || b > 3 || (a != 0 && a == b)) return;
            slots[0] = (SpiritId)a; slots[1] = (SpiritId)b;
        }
        catch (Exception) { }
    }
}
