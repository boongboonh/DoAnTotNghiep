using UnityEngine;

/// <summary>Giữ nền HUD vừa với số ô máu/năng lượng đang mở khóa.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class HudVitalsPanel : MonoBehaviour
{
    private RectTransform panel;
    private int previousCount = -1;

    private void Awake() => panel = (RectTransform)transform;

    private void LateUpdate()
    {
        int health = HealthPlayer.Instance != null ? HealthPlayer.Instance.HealthPlayerMax : 3;
        int mana = ManaManager.Instance != null ? ManaManager.Instance.ManaPlayerMax : 3;
        int count = Mathf.Clamp(Mathf.Max(health, mana), 3, 9);
        if (count == previousCount) return;
        previousCount = count;
        float width = Mathf.Max(680f, count * 79f + 380f);
        panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
        panel.anchoredPosition = new Vector2(width * .5f + 20f, panel.anchoredPosition.y);
    }
}
