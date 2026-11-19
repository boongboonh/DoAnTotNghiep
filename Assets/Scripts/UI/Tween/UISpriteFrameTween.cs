using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Chay lap lai danh sach sprite tren Image bang DOTween (thay Animator doi sprite).
/// </summary>
[RequireComponent(typeof(Image))]
public class UISpriteFrameTween : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 12f;

    private Image image;
    private Tween tween;

    private void OnEnable()
    {
        image = GetComponent<Image>();
        if (frames == null || frames.Length == 0 || framesPerSecond <= 0f) return;

        float total = frames.Length / framesPerSecond;
        int last = -1;
        tween = DOVirtual.Float(0f, frames.Length, total, v =>
            {
                int i = Mathf.Min((int)v, frames.Length - 1);
                if (i == last) return;
                last = i;
                image.sprite = frames[i];
            })
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        tween?.Kill();
        tween = null;
    }
}
