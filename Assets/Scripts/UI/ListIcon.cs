using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ListIcon : BinhBehaviour
{
    [SerializeField] protected List<RectTransform> icons;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    private int previousValue = -1;
    private int previousMax = -1;

    protected void RefreshIcons(int value, int max)
    {
        if (value == previousValue && max == previousMax) return;

        for (int i = 0; i < icons.Count; i++)
        {
            RectTransform icon = icons[i];
            icon.gameObject.SetActive(i < max);
            if (i >= max) continue;

            Image image = icon.GetComponent<Image>();
            bool filled = i < value;
            if (fullSprite != null && emptySprite != null)
            {
                image.sprite = filled ? fullSprite : emptySprite;
                image.color = Color.white;
            }
            else
            {
                image.color = filled ? Color.white : Color.gray;
            }

            if (previousValue >= 0 && filled && i >= previousValue)
            {
                LeanTween.cancel(icon);
                LeanTween.scale(icon, new Vector3(1.5f, 1.5f, 1f), 0.25f)
                    .setEaseShake().setIgnoreTimeScale(true);
            }
        }

        previousValue = value;
        previousMax = max;
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadIcons();
    }

 
    protected virtual void LoadIcons()
    {
        if (this.icons.Count > 0) return;
        foreach (RectTransform point in transform)
        {
            point.gameObject.SetActive(false);
            this.icons.Add(point);
            
        }
        Debug.Log(transform.name + ": Load image icon ", gameObject);
    }

   

}
