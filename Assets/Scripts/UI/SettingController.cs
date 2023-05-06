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
        settingBoard.transform.localScale = new Vector3(0, 1, 1);
        LeanTween.scale(settingBoard, new Vector3(1, 1, 1), 0.25f);
        LeanTween.scale(buttonMenu, new Vector3(0, 1, 1), 0.25f).setOnComplete(hideButtonMenu);     //tat nut hien thi bang setting
    }

    //an button menu
    private void hideButtonMenu()
    {
        buttonMenu.SetActive(false);
    }
    //an bang cai dat
    private void hideSettinBoard()
    {
        settingBoard.SetActive(false);
    }

    //gan trong button tat bang cai dat
    public void CloseSetting()
    {
        buttonMenu.SetActive(true);
        LeanTween.scale(buttonMenu, new Vector3(1, 1, 1), 0.25f);
        LeanTween.scale(settingBoard, new Vector3(0, 1, 1), 0.25f).setOnComplete(hideSettinBoard);
    }


}
