using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] List<string> nameMap;

    [SerializeField] List<GameObject> mapPiece;

    private void OnEnable()
    {
        for (int i = 0; i < mapPiece.Count; i++)
        {
            if (PlayerPrefs.GetInt(nameMap[i]) == 1)
            {
                mapPiece[i].SetActive(true);
            }
        }

    }
}
