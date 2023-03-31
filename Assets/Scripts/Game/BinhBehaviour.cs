using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinhBehaviour : MonoBehaviour
{
    protected virtual void Reset()
    {
        this.LoadComponents();
        this.ResetValue();
    }

    protected virtual void Start()
    {

    }
    protected virtual void Awake()
    {
        this.LoadComponents();
    }
    protected virtual void LoadComponents()
    {
        // this.LoadPrefab();
    }

    protected virtual void ResetValue()
    {

    }
    protected virtual void OnEnable()
    {

    }
}
