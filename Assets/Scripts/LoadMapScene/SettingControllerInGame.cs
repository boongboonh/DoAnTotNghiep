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

        UIPanelTween.Show(settingBoard);

        iconList.SetActive(false);

        EventManager.Instance.OnHideInfor();        //an thong tin mau va mana // event 

        Time.timeScale = 0f;
    }

    private void shockSettingEffect()
    {
        LeanTween.scale(settingBoard, new Vector3(1.2f, 1, 1), 0.1f).setEaseShake().setIgnoreTimeScale(true);
    }

    //gan trong button tat bang cai dat
    public void CloseSetting()
    {
        iconList.SetActive(true);
        //settingBoard.SetActive(false);
        UIPanelTween.Hide(settingBoard);

        EventManager.Instance.OnShowInfor();        //hien thi mau va mana
        
        Time.timeScale = 1f;
    }

    private void hideSettingBoard()
    {
        settingBoard.SetActive(false);
    }
}
