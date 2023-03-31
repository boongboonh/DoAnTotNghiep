using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ListIcon : BinhBehaviour
{
    [SerializeField] protected List<RectTransform> icons;


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
