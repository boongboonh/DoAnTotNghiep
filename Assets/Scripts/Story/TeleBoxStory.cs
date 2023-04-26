using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleBoxStory : MonoBehaviour
{
    [SerializeField] private GameObject TextTuto;

    [SerializeField] AudioSource openBoxSound;
    [SerializeField] AudioSource closeBoxSound;

    bool isActive = false;

    public int statusBox = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (statusBox == 0) return;

        if (collision.CompareTag("Player"))
        {
            TextTuto.SetActive(true);

            //am thanh
            playSoundOpen();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isActive) return;           //chua duoc kich hoat
        if (statusBox == 0) return;     //chua xong coot chuyen bo qua
        if (collision.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                collision.GetComponent<PlayerMoveStory>().isMove = false;       //dung di chuyen
                TextTuto.SetActive(false);
                isActive = true;
                switch (statusBox)
                {
                    case 1:
                        {
                            Debug.Log("chay game");
                            break;
                        }
                    case 2:
                        {
                            Debug.Log("Ket chuyen 1");
                            break;
                        }
                    default:
                        {
                            Debug.Log("loi khong co gia tri dung status");
                            break;
                        }
                }

            } //kiem tra click button

        }//kiem tra nguwoi chowi
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (statusBox == 0) return;


        if (collision.CompareTag("Player"))
        {
            TextTuto.SetActive(false);
            //am thanh
            playSoundClose();
        }
    }


    private void playSoundOpen()
    {
        if (openBoxSound.isPlaying) return;
        openBoxSound.Play();
    }
    private void playSoundClose()
    {
        if (closeBoxSound.isPlaying) return;
        closeBoxSound.Play();
    }
}
