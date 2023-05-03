using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconPlayerController : MonoBehaviour
{
    [SerializeField] List<RectTransform> pointMap;
    [SerializeField] private GameObject playerIcon;

    private void OnEnable()
    {
        int mapCurrent = 1;
        if (PlayerPrefs.HasKey("MapCurrent"))
        {
            mapCurrent = PlayerPrefs.GetInt("MapCurrent");          //lay vi tri nguoi choi
        }

        switch (mapCurrent)
        {
            case 1:
                {
                    setIconPlayer(pointMap[0]);
                    break;
                }
            case 2:
                {
                    setIconPlayer(pointMap[1]);
                    break;
                }
            case 3:
                {
                    setIconPlayer(pointMap[2]);
                    break;
                }
            case 4:
                {
                    setIconPlayer(pointMap[3]);
                    break;
                
                }
            case 5:
                {
                    setIconPlayer(pointMap[4]);
                    break;
                }
            case 6:
                {
                    setIconPlayer(pointMap[5]);
                    break;
                }
            case 7:
                {
                    setIconPlayer(pointMap[6]);
                    break;
                }
            default:
                {
                    Debug.Log("loi dau vao map point icon");
                    break;
                }
        }
    }

    private void setIconPlayer(RectTransform pointMap)
    {
        playerIcon.transform.SetParent(pointMap.transform,false);               //truyen false giu nguyen toa do ban dau cua icon
    }
}
