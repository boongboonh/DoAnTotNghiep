using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] GameObject miniMap;
    [SerializeField] GameObject teleButton;
    [SerializeField] GameObject iconInGame;

    bool isShow = false;

    private void Update()
    {
        if (isShow) return;
        if (Input.GetKeyDown(KeyCode.M))
        {
            OpentMiniMap();
        }
    }

    public void OpentMiniMap()
    {
        iconInGame.SetActive(false);
        isShow = true;

        miniMap.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ExitMinimap()
    {
        isShow = false;
        teleButton.SetActive(false);        //tat nut dich chuyen khi khong dich chuyen
        miniMap.SetActive(false);           //tat hien thi map

        iconInGame.SetActive(true);         //hien thi cac nut icon tren man hinh game

        Time.timeScale = 1f;
    }
}
