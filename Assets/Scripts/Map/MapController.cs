using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] GameObject miniMap;
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
        miniMap.SetActive(false);
    }
}
