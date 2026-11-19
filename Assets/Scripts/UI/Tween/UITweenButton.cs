using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Base logic cho moi tuong tac cua button UI bang DOTween (thay cho Animator):
/// hover (enter/exit hoac select/deselect), hold (pointer down/up) va click.
/// Scale nghi cua object luon la (1,1,1); moi hieu ung chi la tween tam thoi.
/// Dat tren GameObject co Button (hoac bat ky Selectable/Graphic nao).
/// </summary>
[DisallowMultipleComponent]
public class UITweenButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    ISelectHandler, IDeselectHandler
{
    public enum VisualState { Normal, Hover, Pressed }

    [Header("Targets")]
    [SerializeField] private Graphic background;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Graphic decorLeft;
    [SerializeField] private Graphic decorRight;

    [Header("Button scale")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float clickPunch = 0.12f;

    [Header("Background alpha (Normal lay tu mau ban dau)")]
    [SerializeField] private float hoverAlpha = 0.78f;
    [SerializeField] private float pressAlpha = 0.55f;

    [Header("Decor")]
    [SerializeField] private float decorHiddenScale = 0.1f;
    [SerializeField] private float decorShownScale = 1f;

    [Header("Timing")]
    [SerializeField] private float enterDuration = 0.2f;
    [SerializeField] private float exitDuration = 0.2f;
    [SerializeField] private float pressDuration = 0.08f;

    [Header("Misc")]
    [SerializeField] private bool boldTitleOnHover = true;

    private readonly object scaleKey = new object();
    private readonly object fadeKey = new object();
    private readonly object decorKey = new object();

    private Selectable selectable;
    private float normalAlpha = 1f;
    private bool initialized;
    private bool hovering;
    private bool pressing;

    private bool Interactable => selectable == null || selectable.IsInteractable();

    private void Init()
    {
        if (initialized) return;
        initialized = true;

        selectable = GetComponent<Selectable>();
        if (background == null) background = GetComponent<Graphic>();
        if (background != null) normalAlpha = background.color.a;

        // decor khong duoc chan raycast cua button
        if (decorLeft != null) decorLeft.raycastTarget = false;
        if (decorRight != null) decorRight.raycastTarget = false;
    }

    private void Awake() => Init();

    private void OnEnable()
    {
        Init();
        hovering = pressing = false;
        Apply(VisualState.Normal, true);
    }

    private void OnDisable()
    {
        KillAll();
        hovering = pressing = false;
        if (initialized) Apply(VisualState.Normal, true);
    }

    private void KillAll()
    {
        DOTween.Kill(scaleKey);
        DOTween.Kill(fadeKey);
        DOTween.Kill(decorKey);
    }

    // ---------------- pointer / selection events ----------------

    public void OnPointerEnter(PointerEventData e)
    {
        hovering = true;
        if (!Interactable) return;
        if (!pressing) Apply(VisualState.Hover);
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovering = false;
        if (!pressing) Apply(VisualState.Normal);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!Interactable) return;
        pressing = true;
        Apply(VisualState.Pressed);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!pressing) return;
        pressing = false;
        Apply(hovering ? VisualState.Hover : VisualState.Normal);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!Interactable || clickPunch <= 0f) return;

        DOTween.Kill(scaleKey);
        float peak = hoverScale + clickPunch;
        DOTween.Sequence().SetId(scaleKey).SetUpdate(true).SetLink(gameObject)
            .Append(transform.DOScale(peak, 0.08f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(hoverScale, 0.14f).SetEase(Ease.OutBack));
    }

    public void OnSelect(BaseEventData e)
    {
        if (Interactable && !pressing) Apply(VisualState.Hover);
    }

    public void OnDeselect(BaseEventData e)
    {
        if (!pressing && !hovering) Apply(VisualState.Normal);
    }

    // ---------------- visuals ----------------

    private void Apply(VisualState state, bool instant = false)
    {
        float scale, alpha, decorScale, decorAlpha, dur;
        Ease scaleEase, decorEase;

        switch (state)
        {
            case VisualState.Hover:
                scale = hoverScale; alpha = hoverAlpha;
                decorScale = decorShownScale; decorAlpha = 1f;
                dur = enterDuration; scaleEase = Ease.OutBack; decorEase = Ease.OutBack;
                break;
            case VisualState.Pressed:
                scale = pressScale; alpha = pressAlpha;
                decorScale = decorShownScale; decorAlpha = 1f;
                dur = pressDuration; scaleEase = Ease.OutQuad; decorEase = Ease.OutQuad;
                break;
            default:
                scale = 1f; alpha = normalAlpha;
                decorScale = decorHiddenScale; decorAlpha = 0f;
                dur = exitDuration; scaleEase = Ease.OutQuad; decorEase = Ease.InQuad;
                break;
        }

        if (title != null && boldTitleOnHover)
            title.fontStyle = state == VisualState.Normal ? FontStyles.Normal : FontStyles.Bold;

        KillAll();

        if (instant)
        {
            transform.localScale = Vector3.one * scale;
            SetAlpha(background, alpha);
            SetDecor(decorLeft, decorScale, decorAlpha);
            SetDecor(decorRight, decorScale, decorAlpha);
            return;
        }

        transform.DOScale(scale, dur).SetEase(scaleEase).SetId(scaleKey).SetUpdate(true).SetLink(gameObject);
        if (background != null)
            background.DOFade(alpha, dur).SetId(fadeKey).SetUpdate(true).SetLink(gameObject);
        TweenDecor(decorLeft, decorScale, decorAlpha, dur, decorEase);
        TweenDecor(decorRight, decorScale, decorAlpha, dur, decorEase);
    }

    private void TweenDecor(Graphic g, float scale, float alpha, float dur, Ease ease)
    {
        if (g == null) return;
        g.rectTransform.DOScale(scale, dur).SetEase(ease).SetId(decorKey).SetUpdate(true).SetLink(gameObject);
        g.DOFade(alpha, dur).SetId(decorKey).SetUpdate(true).SetLink(gameObject);
    }

    private static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        var c = g.color; c.a = a; g.color = c;
    }

    private static void SetDecor(Graphic g, float scale, float alpha)
    {
        if (g == null) return;
        g.rectTransform.localScale = Vector3.one * scale;
        SetAlpha(g, alpha);
    }
}
