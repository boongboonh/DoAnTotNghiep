using UnityEngine;

/// <summary>
/// Dong bo bo trang bi da luu (<see cref="SkillProgressManager"/>) vao cac slot cua <see cref="PlayerSkills"/>.
/// Gan len cung object voi PlayerSkills. Tra cuu SkillData qua <see cref="SkillCatalog"/> (Resources/Skills/SkillCatalog).
/// Chua co catalog thi khong lam gi, giu nguyen slot gan tay trong Inspector.
/// </summary>
[RequireComponent(typeof(PlayerSkills))]
public class SkillLoadout : MonoBehaviour
{
    [SerializeField] private PlayerSkills skills;
    [Tooltip("De trong = tai tu Resources/Skills/SkillCatalog.")]
    [SerializeField] private SkillCatalog catalog;

    private SkillProgressManager progress;
    private int pendingMask;

    private void Awake()
    {
        if (skills == null) skills = GetComponent<PlayerSkills>();
        if (catalog == null) catalog = SkillCatalog.Load();
    }

    private bool started;

    private void OnEnable()
    {
        if (catalog == null) return;
        progress = SkillProgressManager.Instance;
        progress.LoadoutChanged += OnLoadoutChanged;
        if (started) ApplyAll();
    }

    // Start (khong phai OnEnable) de chac chan PlayerSkills.Awake da chay xong.
    private void Start()
    {
        started = true;
        if (progress != null) ApplyAll();
    }

    private void ApplyAll()
    {
        for (int i = 0; i < SkillProgressManager.SlotCount; i++) Apply(i);
    }

    private void OnDisable()
    {
        if (progress != null) progress.LoadoutChanged -= OnLoadoutChanged;
        progress = null;
    }

    private void Update()
    {
        // Slot doi luc dang niem se duoc ap dung ngay khi niem xong.
        if (pendingMask == 0 || skills.Active) return;
        for (int i = 0; i < SkillProgressManager.SlotCount; i++)
            if ((pendingMask & (1 << i)) != 0) Apply(i);
    }

    private void OnLoadoutChanged(int slot) => Apply(slot);

    private void Apply(int slot)
    {
        if (slot >= skills.SlotCount) return;

        SkillData data = catalog.Get(progress.GetLoadout(slot));
        if (skills.SetSlot(slot, data)) pendingMask &= ~(1 << slot);
        else pendingMask |= 1 << slot;
    }
}
