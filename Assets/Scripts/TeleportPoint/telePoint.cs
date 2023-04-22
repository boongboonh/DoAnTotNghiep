using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class telePoint : MonoBehaviour
{
    [Header("name data")]
    [SerializeField] string nameTele = "teleTree1";     //ten luu du lieu
    [SerializeField] string TelePostX = "tree1PostX";   //vi tri x dich chuyen player
    [SerializeField] string TelePostY = "tree1PostY";   //vi tri y dich chuy player


    [Header("setting")]
    [SerializeField] GameObject clickTuto;
    [SerializeField] GameObject EffectPower;
    [SerializeField] bool isActive = false;

    private void Start()
    {
        if (PlayerPrefs.GetInt(nameTele) != 1) return;
        isActive = true;
        EffectPower.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (isActive) return;
        if (collision.CompareTag("Player"))
        {
            clickTuto.SetActive(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isActive) return;
        if (!collision.CompareTag("Player")) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            isActive = true;
            EffectPower.SetActive(true);
            PlayerPrefs.SetInt(nameTele, 1);

            PlayerPrefs.SetFloat(TelePostX, transform.parent.position.x);
            PlayerPrefs.SetFloat(TelePostY, transform.parent.position.y + 1);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            clickTuto.SetActive(false);
        }
    }

}
