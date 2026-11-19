using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Mau va co chu cua bang ky nang. Doi o day, khong can sua code.</summary>
[Serializable]
public class SkillBoardStyle
{
    public Vector2 windowSize = new Vector2(920f, 560f);
    public Color dimColor = new Color(0f, 0f, 0f, 0.65f);
    public Color windowColor = new Color(0.1f, 0.09f, 0.12f, 0.96f);
    public Color cellColor = new Color(0.2f, 0.18f, 0.24f, 1f);
    public Color cellLockedColor = new Color(0.13f, 0.12f, 0.15f, 1f);
    public Color cellSelectedColor = new Color(0.85f, 0.65f, 0.3f, 1f);
    public Color buttonColor = new Color(0.3f, 0.26f, 0.36f, 1f);
    public Color buttonOnColor = new Color(0.85f, 0.65f, 0.3f, 1f);
    public Color textColor = Color.white;
    public Color dimTextColor = new Color(1f, 1f, 1f, 0.55f);
    public Color warnColor = new Color(1f, 0.5f, 0.4f, 1f);

    [Header("Chu")]
    public int titleSize = 28;
    public int bodySize = 17;
    public int cellSize = 17;
}

/// <summary>
/// Bang ky nang: luoi cac skill (tu SkillCatalog), chi tiet skill dang chon, nang cap bac (bac 3 chon nhanh),
/// va 3 slot trang bi. Doc/ghi tien trinh qua SkillProgressManager, khong can Player trong scene.
///   - Dat component nay len mot RectTransform trong Canvas (nen la object luon bat, vd goc Canvas);
///     giao dien dung bang code, khong can prefab. Canvas phai co EventSystem.
///   - Mo/dong bang phim <see cref="toggleKey"/>; Esc de dong. Khi mo: tam dung game va khoa input player.
///   - <see cref="CanEdit"/> = false chi cho xem (nang cap, doi slot bi khoa). Dat true khi o Lang hoac diem luu
///     bang <see cref="SetCanEdit"/>.
/// </summary>
public class SkillBoardPanel : MonoBehaviour
{
    private const int Columns = 3;
    private static readonly string[] FallbackKeys = { "E", "R", "F" };

    [SerializeField] private SkillCatalog catalog;
    [SerializeField] private KeyCode toggleKey = KeyCode.K;
    [SerializeField] private bool canEdit = true;
    [SerializeField] private bool pauseWhileOpen = true;
    [SerializeField] private SkillBoardStyle style = new SkillBoardStyle();

    private sealed class Cell
    {
        public SkillData skill;
        public Button button;
        public Text label;
        public Text tier;
    }

    private SkillProgressManager progress;
    private PlayerInputReader controls;

    private GameObject dim;
    private RectTransform window;
    private Text sapText;
    private readonly List<Cell> cells = new List<Cell>();
    private readonly Button[] slotButtons = new Button[SkillProgressManager.SlotCount];
    private readonly Text[] slotTexts = new Text[SkillProgressManager.SlotCount];

    private Text detailTitle;
    private Text detailBody;
    private Text statusText;
    private readonly Button[] branchButtons = new Button[2];
    private readonly Text[] branchTexts = new Text[2];
    private Button upgradeButton;
    private Text upgradeText;
    private Button resetButton;
    private Text resetText;

    private SkillData selected;
    private int selectedBranch = -1;
    private bool built;
    private bool dirty;
    private float prevTimeScale = 1f;
    private bool lockedInput;

    private bool open;

    public bool IsOpen => open;
    public bool CanEdit => canEdit;

    public void SetCanEdit(bool value)
    {
        canEdit = value;
        dirty = true;
    }

    // ---------------- Vong doi ----------------

    private void Awake()
    {
        if (catalog == null) catalog = SkillCatalog.Load();
    }

    private void Start()
    {
        if (EventSystem.current == null)
            Debug.LogWarning("SkillBoardPanel: scene chua co EventSystem, bang ky nang se khong bam duoc.", this);
    }

    private void OnDisable()
    {
        if (IsOpen) CloseImmediate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (IsOpen) Close(); else Open();
        }
        else if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        if (dirty && IsOpen) Refresh();
    }

    // ---------------- Mo / dong ----------------

    public void Open()
    {
        if (IsOpen) return;
        if (catalog == null) catalog = SkillCatalog.Load();
        if (catalog == null)
        {
            Debug.LogWarning("SkillBoardPanel: chua co SkillCatalog (Resources/Skills/SkillCatalog).", this);
            return;
        }

        progress = SkillProgressManager.Instance;
        Subscribe();
        if (!built) Build();

        if (selected == null && catalog.Skills.Count > 0) selected = catalog.Skills[0];

        open = true;
        dim.SetActive(true);
        UIPanelTween.Show(window.gameObject);
        Refresh();

        if (pauseWhileOpen)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        if (controls == null) controls = FindFirstObjectByType<PlayerInputReader>();
        if (controls != null && !controls.Locked)
        {
            controls.Locked = true;
            lockedInput = true;
        }
    }

    public void Close()
    {
        if (!IsOpen) return;
        dim.SetActive(false);
        UIPanelTween.Hide(window.gameObject);
        Release();
    }

    private void CloseImmediate()
    {
        dim.SetActive(false);
        window.gameObject.SetActive(false);
        window.localScale = Vector3.one;
        Release();
    }

    private void Release()
    {
        open = false;
        Unsubscribe();
        if (pauseWhileOpen) Time.timeScale = prevTimeScale;
        if (lockedInput && controls != null) controls.Locked = false;
        lockedInput = false;
    }

    private void Subscribe()
    {
        progress.Unlocked += OnChanged;
        progress.TierChanged += OnChanged;
        progress.SapChanged += OnChanged;
        progress.LoadoutChanged += OnChanged;
    }

    private void Unsubscribe()
    {
        if (progress == null) return;
        progress.Unlocked -= OnChanged;
        progress.TierChanged -= OnChanged;
        progress.SapChanged -= OnChanged;
        progress.LoadoutChanged -= OnChanged;
    }

    private void OnChanged(SkillId id) => dirty = true;
    private void OnChanged(int value) => dirty = true;

    // ---------------- Dung giao dien ----------------

    private void Build()
    {
        built = true;
        var root = (RectTransform)transform;

        // Nen toi chan bam xuyen.
        var dimRect = NewRect("Dim", root);
        Stretch(dimRect);
        var dimImg = dimRect.gameObject.AddComponent<Image>();
        dimImg.color = style.dimColor;
        dim = dimRect.gameObject;

        window = NewRect("Window", root);
        window.anchorMin = window.anchorMax = new Vector2(0.5f, 0.5f);
        window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = style.windowSize;
        window.anchoredPosition = Vector2.zero;
        window.gameObject.AddComponent<Image>().color = style.windowColor;

        NewText("Title", window, 24, 16, 400, 40, style.titleSize, TextAnchor.MiddleLeft, style.textColor).text = "KỸ NĂNG";
        sapText = NewText("Sap", window, 460, 16, 436, 40, style.bodySize + 2, TextAnchor.MiddleRight, style.textColor);
        var hint = NewText("Hint", window, 24, 520, 872, 30, style.bodySize - 3, TextAnchor.MiddleLeft, style.dimTextColor);
        hint.text = "Chọn skill ở bên trái, bấm ô E / R / F bên dưới để trang bị. Phím K hoặc Esc để đóng.";

        // Luoi skill.
        for (int i = 0; i < catalog.Skills.Count; i++)
        {
            SkillData skill = catalog.Skills[i];
            if (skill == null) continue;

            int col = i % Columns, row = i / Columns;
            Button b = NewButton("Cell" + i, window, 24 + col * 148, 70 + row * 112, 136, 100, out Text label);
            label.fontSize = style.cellSize;

            Text tier = NewText("Tier", b.transform, 6, 66, 124, 28, style.cellSize - 3, TextAnchor.LowerCenter, style.dimTextColor);

            var cell = new Cell { skill = skill, button = b, label = label, tier = tier };
            b.onClick.AddListener(() => Select(cell.skill));
            cells.Add(cell);
        }

        // Chi tiet.
        const float dx = 490f;
        detailTitle = NewText("DetailTitle", window, dx, 70, 406, 40, style.titleSize - 2, TextAnchor.MiddleLeft, style.textColor);
        detailBody = NewText("DetailBody", window, dx, 116, 406, 190, style.bodySize, TextAnchor.UpperLeft, style.textColor);
        detailBody.verticalOverflow = VerticalWrapMode.Truncate;
        detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        detailBody.supportRichText = true;

        for (int i = 0; i < 2; i++)
        {
            int branch = i;
            branchButtons[i] = NewButton("Branch" + i, window, dx + i * 206, 312, 200, 62, out branchTexts[i]);
            branchTexts[i].fontSize = style.bodySize - 3;
            branchButtons[i].onClick.AddListener(() => { selectedBranch = branch; dirty = true; });
        }

        upgradeButton = NewButton("Upgrade", window, dx, 384, 250, 44, out upgradeText);
        upgradeButton.onClick.AddListener(OnUpgrade);
        resetButton = NewButton("ResetBranch", window, dx, 384, 250, 44, out resetText);
        resetButton.onClick.AddListener(OnResetBranch);
        statusText = NewText("Status", window, dx, 434, 406, 50, style.bodySize - 2, TextAnchor.UpperLeft, style.warnColor);

        // Slot trang bi.
        NewText("LoadoutLabel", window, 24, 436, 120, 30, style.bodySize, TextAnchor.MiddleLeft, style.dimTextColor).text = "Trang bị";
        for (int i = 0; i < SkillProgressManager.SlotCount; i++)
        {
            int slot = i;
            slotButtons[i] = NewButton("Slot" + i, window, 24 + i * 148, 466, 136, 48, out slotTexts[i]);
            slotTexts[i].fontSize = style.bodySize - 1;
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(slot));
        }

        window.gameObject.SetActive(false);
        dim.SetActive(false);
    }

    // ---------------- Cap nhat ----------------

    private string KeyName(int slot)
    {
        if (controls == null) controls = FindFirstObjectByType<PlayerInputReader>();
        string n = controls != null ? controls.SkillKeyName(slot) : string.Empty;
        return string.IsNullOrEmpty(n) ? FallbackKeys[slot] : n;
    }

    private int EquippedSlot(SkillData skill)
    {
        if (skill == null || skill.id == SkillId.None) return -1;
        for (int i = 0; i < SkillProgressManager.SlotCount; i++)
            if (progress.GetLoadout(i) == skill.id) return i;
        return -1;
    }

    private void Select(SkillData skill)
    {
        if (selected != skill) selectedBranch = -1;
        selected = skill;
        dirty = true;
        Refresh();
    }

    private void Refresh()
    {
        dirty = false;
        sapText.text = "Nhựa thời gian: " + progress.Sap;

        foreach (Cell c in cells)
        {
            bool unlocked = progress.IsUnlocked(c.skill.id);
            int slot = EquippedSlot(c.skill);
            c.label.text = unlocked ? c.skill.displayName : "?";
            c.label.color = unlocked ? style.textColor : style.dimTextColor;
            c.tier.text = unlocked
                ? "Bậc " + progress.GetTier(c.skill.id) + (slot >= 0 ? "  [" + KeyName(slot) + "]" : string.Empty)
                : string.Empty;
            SetButtonColor(c.button, c.skill == selected ? style.cellSelectedColor
                : unlocked ? style.cellColor : style.cellLockedColor);
        }

        for (int i = 0; i < SkillProgressManager.SlotCount; i++)
        {
            SkillData s = catalog.Get(progress.GetLoadout(i));
            slotTexts[i].text = KeyName(i) + ": " + (s != null ? s.displayName : "—");
            SetButtonColor(slotButtons[i], s != null ? style.buttonOnColor : style.buttonColor);
            slotButtons[i].interactable = canEdit;
        }

        RefreshDetail();
    }

    private void RefreshDetail()
    {
        SetActive(branchButtons[0], false);
        SetActive(branchButtons[1], false);
        SetActive(upgradeButton, false);
        SetActive(resetButton, false);
        statusText.text = string.Empty;

        if (selected == null)
        {
            detailTitle.text = string.Empty;
            detailBody.text = string.Empty;
            return;
        }

        SkillId id = selected.id;
        int tier = progress.GetTier(id);

        if (tier == 0)
        {
            detailTitle.text = "?";
            detailBody.text = string.IsNullOrEmpty(selected.unlockHint) ? "???" : selected.unlockHint;
            return;
        }

        SkillStats stats = progress.StatsOf(selected);
        detailTitle.text = selected.displayName + "   Bậc " + tier + "/" + SkillData.MaxTier;

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(selected.description)) sb.AppendLine(selected.description).AppendLine();
        sb.Append("Mana ").Append(stats.ManaCost)
          .Append("  ·  Hồi chiêu ").Append(stats.Cooldown.ToString("0.#")).Append("s")
          .Append("  ·  Niệm ").Append(stats.Duration.ToString("0.##")).AppendLine("s");

        int branch = progress.GetBranch(id);
        if (tier >= 2 && !string.IsNullOrEmpty(selected.tier2.note))
            sb.AppendLine().Append("Bậc 2: ").AppendLine(selected.tier2.note);
        if (tier >= 3)
        {
            SkillTier b = selected.GetBranch(branch);
            if (b != null)
                sb.Append("Bậc 3 (").Append(string.IsNullOrEmpty(b.branchName) ? "nhánh " + (branch + 1) : b.branchName)
                  .Append("): ").AppendLine(b.note);
        }
        else if (tier == 1 && !string.IsNullOrEmpty(selected.tier2.note))
        {
            sb.AppendLine().Append("<color=#aaaaaa>Bậc kế: ").Append(selected.tier2.note).Append("</color>");
        }
        detailBody.text = sb.ToString();

        // Nut nang cap / chon nhanh / dat lai.
        if (tier == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                SkillTier b = selected.GetBranch(i);
                SetActive(branchButtons[i], b != null);
                if (b == null) continue;
                string name = string.IsNullOrEmpty(b.branchName) ? "Nhánh " + (i + 1) : b.branchName;
                branchTexts[i].text = name + (string.IsNullOrEmpty(b.note) ? string.Empty : "\n" + b.note);
                SetButtonColor(branchButtons[i], selectedBranch == i ? style.buttonOnColor : style.buttonColor);
                branchButtons[i].interactable = canEdit;
            }
        }

        if (tier < SkillData.MaxTier)
        {
            SetActive(upgradeButton, true);
            SkillUpgradeResult r = progress.CheckUpgrade(selected, selectedBranch);
            int cost = progress.UpgradeCost(selected, selectedBranch);

            upgradeText.text = r == SkillUpgradeResult.NeedBranch
                ? "Chọn nhánh bậc 3"
                : "Nâng cấp bậc " + (tier + 1) + (cost >= 0 ? " (" + cost + " nhựa)" : string.Empty);
            upgradeButton.interactable = canEdit && r == SkillUpgradeResult.Ok;
            SetButtonColor(upgradeButton, style.buttonColor);

            if (!canEdit) statusText.text = "Chỉ chỉnh được ở Làng hoặc điểm lưu.";
            else if (r == SkillUpgradeResult.NotEnoughSap) statusText.text = "Chưa đủ nhựa thời gian.";
        }
        else
        {
            SetActive(resetButton, true);
            resetText.text = "Đặt lại nhánh (" + SkillProgressManager.ResetBranchCost + " nhựa)";
            resetButton.interactable = canEdit && progress.Sap >= SkillProgressManager.ResetBranchCost;
            SetButtonColor(resetButton, style.buttonColor);
            if (!canEdit) statusText.text = "Chỉ chỉnh được ở Làng hoặc điểm lưu.";
        }
    }

    // ---------------- Thao tac ----------------

    private void OnUpgrade()
    {
        if (!canEdit || selected == null) return;
        if (progress.TryUpgrade(selected, selectedBranch) == SkillUpgradeResult.Ok) selectedBranch = -1;
        dirty = true;
    }

    private void OnResetBranch()
    {
        if (!canEdit || selected == null) return;
        progress.TryResetBranch(selected);
        selectedBranch = -1;
        dirty = true;
    }

    private void OnSlotClicked(int slot)
    {
        if (!canEdit || selected == null || !progress.IsUnlocked(selected.id)) return;
        if (progress.GetLoadout(slot) == selected.id) progress.Unequip(slot);
        else progress.Equip(slot, selected.id);
        dirty = true;
    }

    // ---------------- Tien ich dung UI ----------------

    private static Font font;

    private static Font DefaultFont
    {
        get
        {
            if (font == null) font = TMPro.TMP_Settings.defaultFontAsset?.sourceFontFile ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    private static void SetActive(Component c, bool active)
    {
        if (c.gameObject.activeSelf != active) c.gameObject.SetActive(active);
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    /// <summary>Dat o goc tren-trai cua parent: (x, y) tinh tu goc, y huong xuong.</summary>
    private static RectTransform NewBox(string name, Transform parent, float x, float y, float w, float h)
    {
        RectTransform rt = NewRect(name, parent);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    private static Text NewText(string name, Transform parent, float x, float y, float w, float h,
        int size, TextAnchor anchor, Color color)
    {
        RectTransform rt = NewBox(name, parent, x, y, w, h);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = DefaultFont;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private static Button NewButton(string name, Transform parent, float x, float y, float w, float h, out Text label)
    {
        RectTransform rt = NewBox(name, parent, x, y, w, h);
        var img = rt.gameObject.AddComponent<Image>();
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        label = NewText("Label", rt, 4, 4, w - 8, h - 8, 16, TextAnchor.MiddleCenter, Color.white);
        return btn;
    }

    private static void SetButtonColor(Button b, Color c)
    {
        var colors = b.colors;
        colors.normalColor = c;
        colors.highlightedColor = Color.Lerp(c, Color.white, 0.2f);
        colors.selectedColor = c;
        colors.pressedColor = Color.Lerp(c, Color.black, 0.2f);
        colors.disabledColor = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 0.7f);
        b.colors = colors;
    }
}
