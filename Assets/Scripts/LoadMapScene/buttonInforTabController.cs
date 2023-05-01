using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonInforTabController : MonoBehaviour
{
    [SerializeField] int id;

    public void openTabInforButton()
    {
        EventManager.Instance.OpenTabInfor(id);
    }
}
