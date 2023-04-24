using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAddHPvsMana : MonoBehaviour
{
    [Header("effect")]
    [SerializeField] private GameObject effectRingPower;

    [Header("name PlayerPrefs")]
    [SerializeField] private string nameItem = "itemUp1";

    [Header("sound Effect")]
    [SerializeField] private AudioSource CollecItem;

    AudioClip audioClipCollect;
    private void Start()
    {
        if (PlayerPrefs.GetInt(nameItem) == 1)
        {
            gameObject.SetActive(false);
        }  
        audioClipCollect = CollecItem.clip;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //chay am thanh
            AudioSource.PlayClipAtPoint(audioClipCollect, this.transform.position);

            //luu du lieu == 1 da collect
            PlayerPrefs.SetInt(nameItem, 1);

            collision.gameObject.GetComponent<HealthPlayer>().addHealth();
            collision.gameObject.GetComponent<ManaManager>().addMana();
            GameObject cloneEffect = Instantiate(effectRingPower, transform.position, Quaternion.identity);
            Destroy(cloneEffect, 2f);

            gameObject.SetActive(false);
        }
    }
    
}
