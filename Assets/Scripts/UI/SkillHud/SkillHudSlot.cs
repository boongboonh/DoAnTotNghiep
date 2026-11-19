using UnityEngine;
using UnityEngine.UI;

/// <summary>Giao dien cua mot o skill tren HUD. Do SkillHud tao va cap nhat moi frame.</summary>
public class SkillHudSlot : MonoBehaviour
{
    private const float FailTime = 0.25f;
    private const float CastTime = 0.2f;

    private static Sprite whiteSprite;
    private static Font font;

    private SkillHudStyle style;
    private RectTransform content;
    private Image frame;
    private Image icon;
    private Image cooldownFill;
    private Text keyText;
    private Text nameText;
    private Text cooldownText;

    private float failTimer;
    private float castTimer;

    // ---------------- Tao giao dien bang code ----------------

    public static SkillHudSlot Create(RectTransform parent, SkillHudStyle style, string keyName, string objectName)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = style.slotSize;

        var slot = go.AddComponent<SkillHudSlot>();
        slot.style = style;

        slot.content = NewRect("Content", rt, 0f);
        slot.frame = NewImage("Frame", slot.content, style.frameSprite != null ? style.frameSprite : White, 0f);
        slot.icon = NewImage("Icon", slot.content, null, style.iconPadding);
        slot.icon.preserveAspect = true;

        slot.cooldownFill = NewImage("Cooldown", slot.content, White, style.iconPadding);
        slot.cooldownFill.color = style.cooldownColor;
        slot.cooldownFill.type = Image.Type.Filled;
        slot.cooldownFill.fillMethod = Image.FillMethod.Radial360;
        slot.cooldownFill.fillOrigin = (int)Image.Origin360.Top;
        slot.cooldownFill.fillClockwise = false;

        slot.nameText = NewText("Name", slot.content, style.nameFontSize, TextAnchor.MiddleCenter, style.keyColor);
        slot.cooldownText = NewText("CooldownText", slot.content, style.cooldownFontSize, TextAnchor.MiddleCenter, style.cooldownTextColor);
        if (style.keycapSprite != null && !string.IsNullOrEmpty(keyName))
        {
            UnityEngine.UI.Image keycap = NewImage("Keycap", slot.content, style.keycapSprite, 0f);
            RectTransform keycapRect = keycap.rectTransform;
            keycapRect.anchorMin = keycapRect.anchorMax = new Vector2(0.5f, 0f);
            keycapRect.anchoredPosition = Vector2.zero;
            keycapRect.sizeDelta = new Vector2(style.keyFontSize * 2.5f, style.keyFontSize * 1.25f);
            slot.keyText = NewText("Key", keycapRect, style.keyFontSize, TextAnchor.MiddleCenter, style.keyColor);
        }
        else
        {
            slot.keyText = NewText("Key", slot.content, style.keyFontSize, TextAnchor.LowerRight, style.keyColor);
        }
        slot.keyText.text = keyName;

        return slot;
    }

    // ---------------- Cap nhat ----------------

    /// <param name="state">NotEnoughMana / Airborne: icon bi lam toi; Cooldown / None: icon binh thuong.</param>
    public void Refresh(SkillData skill, float cooldown01, float remainingSeconds, SkillFailReason state, Sprite fallbackIcon = null)
    {
        float dt = Time.unscaledDeltaTime;
        failTimer = Mathf.Max(0f, failTimer - dt);
        castTimer = Mathf.Max(0f, castTimer - dt);

        bool empty = skill == null;
        Sprite activeIcon = empty ? null : skill.icon != null ? skill.icon : fallbackIcon;
        bool hasIcon = activeIcon != null;

        icon.enabled = hasIcon;
        if (hasIcon) icon.sprite = activeIcon;
        nameText.text = !empty && !hasIcon ? skill.displayName : string.Empty;

        bool unusable = state == SkillFailReason.NotEnoughMana || state == SkillFailReason.Airborne;
        icon.color = unusable ? style.unusableColor : Color.white;
        nameText.color = unusable ? style.unusableColor : style.keyColor;

        Color frameColor = empty ? style.emptyColor : style.frameColor;
        frame.color = Color.Lerp(frameColor, style.failColor, failTimer / FailTime);

        cooldownFill.enabled = !empty && cooldown01 > 0f;
        cooldownFill.fillAmount = cooldown01;
        cooldownText.text = remainingSeconds > 0f
            ? (remainingSeconds >= 1f ? Mathf.CeilToInt(remainingSeconds).ToString() : remainingSeconds.ToString("0.0"))
            : string.Empty;

        keyText.enabled = true;

        // Nay nhe khi niem skill, rung nhe khi bam khong niem duoc.
        float pulse = 1f + style.castPulse * Mathf.Sin(castTimer / CastTime * Mathf.PI);
        content.localScale = new Vector3(pulse, pulse, 1f);
        float shake = failTimer > 0f ? Mathf.Sin(failTimer * 70f) * style.failShake * (failTimer / FailTime) : 0f;
        content.anchoredPosition = new Vector2(shake, 0f);
    }

    public void PlayCast() => castTimer = CastTime;

    public void PlayFail() => failTimer = FailTime;

    // ---------------- Tien ich dung UI ----------------

    private static Sprite White
    {
        get
        {
            if (whiteSprite == null)
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
                tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                tex.Apply();
                whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
                whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            }
            return whiteSprite;
        }
    }

    private static Font DefaultFont
    {
        get
        {
            if (font == null) font = TMPro.TMP_Settings.defaultFontAsset?.sourceFontFile ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    private static RectTransform NewRect(string name, RectTransform parent, float inset)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    private static Image NewImage(string name, RectTransform parent, Sprite sprite, float inset)
    {
        RectTransform rt = NewRect(name, parent, inset);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    private static Text NewText(string name, RectTransform parent, int size, TextAnchor anchor, Color color)
    {
        RectTransform rt = NewRect(name, parent, 3f);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = DefaultFont;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = string.Empty;
        return t;
    }
}
