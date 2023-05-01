using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InforManager : MonoBehaviour
{
    [SerializeField] List<GameObject> tabInfors;

    private void Start()
    {
        EventManager.Instance.clickButtonInforTab += OpenTab;

        //hien thi thong tin dau tien
        EventManager.Instance.OpenTabInfor(0);
    }

    public void OpenTab(int id)
    {
        for (int i = 0; i < tabInfors.Count; i++)
        {
            if (i == id)
            {
                Debug.Log("id hien tai la: "+id);
                tabInfors[i].SetActive(true);
            }
            else
            {
                tabInfors[i].SetActive(false);
            }
            
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.clickButtonInforTab -= OpenTab;
    }
}

