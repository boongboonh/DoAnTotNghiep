using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] GameObject miniMap;
    [SerializeField] GameObject teleButton;
    bool isShow = false;

    private void Update()
    {
        if (isShow) return;
        if (Input.GetKeyDown(KeyCode.M))
        {
            isShow = true;
            miniMap.SetActive(true);
        }
    }

    public void ExitMinimap()
    {
        isShow = false;
        teleButton.SetActive(false);        //tat nut dich chuyen khi khong dich chuyen
        miniMap.SetActive(false);           //tat hien thi map
    }
}
