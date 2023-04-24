using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMana : MonoBehaviour
{
    //hoi mana cho nguoi choi

    [Header("sound item collect")]
    [SerializeField] private AudioSource collectSound;
    AudioClip audioClipCollect;

    private void Start()
    {
        audioClipCollect = collectSound.clip;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<ManaManager>().ManaHealing();

            //chay am thanh
            AudioSource.PlayClipAtPoint(audioClipCollect, this.transform.position);

            Destroy(gameObject);
        }
    }
}
