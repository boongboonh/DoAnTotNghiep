using DG.Tweening;
using UnityEngine;

/// <summary>
/// Logic dung chung de hien/an panel UI bang DOTween (thay LeanTween).
/// Scale nghi cua panel luon la (1,1,1), ke ca khi da bi an.
/// </summary>
public static class UIPanelTween
{
    public const float DefaultDuration = 0.25f;

    public static void Show(GameObject panel, float duration = DefaultDuration)
    {
        if (panel == null) return;
        if (panel.TryGetComponent<UIPopupTransition>(out var popup))
        {
            popup.Show(duration);
            return;
        }

        var t = panel.transform;
        DOTween.Kill(t);

        panel.SetActive(true);
        t.localScale = new Vector3(0f, 1f, 1f);
        t.DOScale(Vector3.one, duration).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(panel);
    }

    public static void Hide(GameObject panel, float duration = DefaultDuration)
    {
        if (panel == null) return;
        if (panel.TryGetComponent<UIPopupTransition>(out var popup))
        {
            popup.Hide(duration);
            return;
        }

        var t = panel.transform;
        DOTween.Kill(t);

        if (!panel.activeSelf) { t.localScale = Vector3.one; return; }

        t.DOScale(new Vector3(0f, 1f, 1f), duration).SetEase(Ease.InQuad).SetUpdate(true).SetLink(panel)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                t.localScale = Vector3.one;
            });
    }
}
