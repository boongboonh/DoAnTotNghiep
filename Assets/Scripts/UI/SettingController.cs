using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingController : MonoBehaviour
{
    [SerializeField] GameObject buttonMenu;
    [SerializeField] GameObject settingBoard;
 

    //goi trong button setting o menu game
    public void OpenSetting()
    {
        UIPanelTween.Show(settingBoard);
        UIPanelTween.Hide(buttonMenu);          //tat nut hien thi bang setting
    }

    //gan trong button tat bang cai dat
    public void CloseSetting()
    {
        UIPanelTween.Show(buttonMenu);
        UIPanelTween.Hide(settingBoard);
    }


}
