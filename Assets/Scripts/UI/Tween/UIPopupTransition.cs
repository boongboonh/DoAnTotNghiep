using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIPopupTransition : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private CanvasGroup backdrop;

    private const float OpenDurationMultiplier = 1.12f;
    private const float CloseDurationMultiplier = 0.72f;
    private static readonly Vector2 OpenOffset = new Vector2(0f, -36f);
    private static readonly Vector2 CloseOffset = new Vector2(0f, -20f);
    private CanvasGroup contentGroup;
    private Vector2 restingPosition;
    private Sequence transition;

    public void Show(float duration)
    {
        Initialize();
        transition?.Kill();
        if (!gameObject.activeSelf)
        {
            content.anchoredPosition = restingPosition + OpenOffset;
            content.localScale = Vector3.one * 0.96f;
            contentGroup.alpha = 0f;
            backdrop.alpha = 0f;
            gameObject.SetActive(true);
        }

        contentGroup.interactable = true;
        contentGroup.blocksRaycasts = true;
        duration *= OpenDurationMultiplier;
        transition = DOTween.Sequence().SetUpdate(true);
        transition.Join(backdrop.DOFade(1f, duration * 0.74f).SetEase(Ease.OutQuad));
        transition.Join(contentGroup.DOFade(1f, duration * 0.65f).SetEase(Ease.OutQuad));
        transition.Join(content.DOAnchorPos(restingPosition, duration).SetEase(Ease.OutCubic));
        transition.Join(content.DOScale(Vector3.one, duration).SetEase(Ease.OutCubic));
    }

    public void Hide(float duration)
    {
        if (!gameObject.activeSelf) return;

        Initialize();
        transition?.Kill();
        contentGroup.interactable = false;
        contentGroup.blocksRaycasts = false;
        duration *= CloseDurationMultiplier;
        transition = DOTween.Sequence().SetUpdate(true);
        transition.Join(content.DOAnchorPos(restingPosition + CloseOffset, duration).SetEase(Ease.InQuad));
        transition.Join(content.DOScale(Vector3.one * 0.98f, duration).SetEase(Ease.InQuad));
        transition.Join(contentGroup.DOFade(0f, duration * 0.82f).SetEase(Ease.InQuad));
        transition.Join(backdrop.DOFade(0f, duration * 1.15f).SetEase(Ease.InOutSine));
        transition.OnComplete(() =>
        {
            transition = null;
            gameObject.SetActive(false);
        });
    }

    private void Initialize()
    {
        if (contentGroup) return;

        restingPosition = content.anchoredPosition;
        if (!content.TryGetComponent(out contentGroup))
            contentGroup = content.gameObject.AddComponent<CanvasGroup>();
    }

    private void OnDisable()
    {
        transition?.Kill();
        transition = null;
    }
}
