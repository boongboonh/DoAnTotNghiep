using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InforController : MonoBehaviour
{
    [SerializeField] GameObject inforGame;
    [SerializeField] GameObject iconList;
   

    //goi trong button icon infor
    public void OpenInforGame()
    {
        inforGame.SetActive(true);
        iconList.SetActive(false);

        EventManager.Instance.OnHideInfor();        //tat hien thi thong tin mau va mana

        Time.timeScale = 0f;
    }


    //goi trong button thoat infor
    public void CloseInforGame()
    {
        iconList.SetActive(true);
        inforGame.SetActive(false);

        EventManager.Instance.OnShowInfor();        //hien thi lai mau va mana

        Time.timeScale = 1f;
    }

}
