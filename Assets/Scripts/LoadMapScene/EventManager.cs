using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    //su kien click button infor
    public event Action<int> clickButtonInforTab;
    public void OpenTabInfor(int id)
    {
        //clickButtonInforTab = null;

        if (clickButtonInforTab != null)
        {
            clickButtonInforTab?.Invoke(id);
        }
    }

    //su kien an/ hien thong tin mau va mana

    public delegate void ShowInfor();
    public ShowInfor OnShowInfor;

    public delegate void HideInfor();
    public HideInfor OnHideInfor;

    public void Show()
    {
        if (OnShowInfor != null)
        {
            OnShowInfor();
        }
    }

    public void Hide()
    {
        if (OnShowInfor != null)
        {
            OnShowInfor();
        }
    }


    //su kien dong mo cua boss

    public delegate void DoorEvent();
    public event DoorEvent DoorOpened;
    public event DoorEvent DoorClosed;

    public void OpenDoor()
    {
        if (DoorOpened != null)
            DoorOpened();
    }

    public void CloseDoor()
    {
        if (DoorClosed != null)
            DoorClosed();
    }
}
