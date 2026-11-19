using DG.Tweening;
using UnityEngine;

/// <summary>
/// Hieu ung lap yoyo cho decor (di chuyen / xoay Z / scale) bang DOTween.
/// Gia tri nghi lay luc OnEnable; tween di tu nghi -> nghi + offset roi quay lai.
/// </summary>
[DisallowMultipleComponent]
public class UILoopTween : MonoBehaviour
{
    [SerializeField] private Vector2 moveOffset;
    [SerializeField] private float rotateZOffset;
    [SerializeField] private float scaleOffset;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float delay;
    [SerializeField] private Ease ease = Ease.InOutSine;

    private RectTransform rect;
    private Vector2 startPos;
    private Vector3 startEuler;
    private Vector3 startScale;
    private Tween tween;

    private void OnEnable()
    {
        rect = transform as RectTransform;
        startPos = rect != null ? rect.anchoredPosition : (Vector2)transform.localPosition;
        startEuler = transform.localEulerAngles;
        startScale = transform.localScale;

        tween = DOVirtual.Float(0f, 1f, duration, Apply)
            .SetDelay(delay)
            .SetEase(ease)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        tween?.Kill();
        tween = null;
        Apply(0f);
    }

    private void Apply(float k)
    {
        if (rect != null) rect.anchoredPosition = startPos + moveOffset * k;
        else transform.localPosition = (Vector3)(startPos + moveOffset * k);

        transform.localEulerAngles = new Vector3(startEuler.x, startEuler.y, startEuler.z + rotateZOffset * k);
        transform.localScale = startScale + Vector3.one * (scaleOffset * k);
    }
}
