using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitController : MonoBehaviour
{
    [SerializeField] GameObject confirmExit;
    [SerializeField] GameObject iconList;


    //goi trong button icon exit
    public void OpentConfirm()
    {
        confirmExit.SetActive(true);
        iconList.SetActive(false);

        EventManager.Instance.OnHideInfor();        //an thong tin mau va mana
        //khoa time
        Time.timeScale = 0f;
        
    }


    //goi trong button thoat

    public void YesConfirmExit()
    {
        Application.Quit();
        Debug.Log("out game");
    }

    //goi trong button choi tiep
    public void NoConfirmExit()
    {
        iconList.SetActive(true);
        confirmExit.SetActive(false);

        EventManager.Instance.OnShowInfor();        //hien thi thong tin mau va mana

        Time.timeScale = 1f;
    }

}
