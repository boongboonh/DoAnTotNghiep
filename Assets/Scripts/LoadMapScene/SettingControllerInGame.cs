using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingControllerInGame : MonoBehaviour
{
    [SerializeField] GameObject settingBoard;
    [SerializeField] GameObject iconList;


    //goi trong button setting
    public void OpenSetting()
    {
        settingBoard.SetActive(true);
        iconList.SetActive(false);

        EventManager.Instance.OnHideInfor();        //an thong tin mau va mana // event 

        Time.timeScale = 0f;
    }

    //gan trong button tat bang cai dat
    public void CloseSetting()
    {
        iconList.SetActive(true);
        settingBoard.SetActive(false);

        EventManager.Instance.OnShowInfor();        //hien thi mau va mana

        Time.timeScale = 1f;
    }
}
