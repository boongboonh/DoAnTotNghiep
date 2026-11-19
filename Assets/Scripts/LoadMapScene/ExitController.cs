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
        UIPanelTween.Show(confirmExit);

    
        iconList.SetActive(false);

        EventManager.Instance.OnHideInfor();        //an thong tin mau va mana
        //khoa time
        Time.timeScale = 0f;
        
    }

    private void shockConfirmEffect()
    {
        LeanTween.scale(confirmExit, new Vector3(1.2f, 1, 1), 0.1f).setEaseShake().setIgnoreTimeScale(true);
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

        UIPanelTween.Hide(confirmExit);


        EventManager.Instance.OnShowInfor();        //hien thi thong tin mau va mana

        Time.timeScale = 1f;
    }

    private void hideConfirm()
    {
        confirmExit.SetActive(false);
    }
}
