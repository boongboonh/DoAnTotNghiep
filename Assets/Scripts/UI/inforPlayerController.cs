using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inforPlayerController : MonoBehaviour
{
    [SerializeField] GameObject inforPlayer;

    void Start()
    {
        // Đăng kí event
        EventManager.Instance.OnShowInfor += Show;
        EventManager.Instance.OnHideInfor += Hide;
    }


    public void Show()
    {
        inforPlayer.SetActive(true);
    }

    private void Hide()
    {
        inforPlayer.SetActive(false);
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnShowInfor -= Show;
        EventManager.Instance.OnHideInfor -= Hide;
    }
}
