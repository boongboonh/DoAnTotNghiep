using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] List<string> nameMap;

    [SerializeField] List<GameObject> mapPiece;

    [SerializeField] List<string> nameTree;

    [SerializeField] List<GameObject> treeIconTele;

    private void OnEnable()
    {
        //hien thi map
        for (int i = 0; i < mapPiece.Count; i++)
        {
            if (PlayerPrefs.GetInt(nameMap[i]) == 1)
            {
                mapPiece[i].SetActive(true);
            }
        }
        
        //hien thi cay
        for (int i = 0; i < treeIconTele.Count; i++)
        {
            if (PlayerPrefs.GetInt(nameTree[i]) == 1)
            {
                treeIconTele[i].SetActive(true);
            }
        }


    }
}
