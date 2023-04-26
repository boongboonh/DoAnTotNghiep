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
        settingBoard.SetActive(true);
        buttonMenu.SetActive(false);
    }

    //gan trong button tat bang cai dat
    public void CloseSetting()
    {
        buttonMenu.SetActive(true);
        settingBoard.SetActive(false);
    }


}
