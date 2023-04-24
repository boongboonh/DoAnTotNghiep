using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHealing : MonoBehaviour
{
    //hoi mau cho nguoi choi
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
            collision.gameObject.GetComponent<HealthPlayer>().healing();

            //chay am thanh
            AudioSource.PlayClipAtPoint(audioClipCollect, this.transform.position);

            Destroy(gameObject);
        }
    }
}
