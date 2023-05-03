using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerMiniMap : MonoBehaviour
{

    //luu vi tri nguoi choi dang o
    [SerializeField] private int NumberMap;

    [SerializeField] private string mapPlayerprefs = "MapCurrent";
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerPrefs.SetInt(mapPlayerprefs, NumberMap);
        }
    }
}
