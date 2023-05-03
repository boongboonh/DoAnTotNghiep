using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleBoxEndGame : MonoBehaviour
{
    [SerializeField] private GameObject TextTuto;

    [SerializeField] AudioSource openBoxSound;
    [SerializeField] AudioSource closeBoxSound;
    private bool isActive = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TextTuto.SetActive(true);

            //am thanh
            playSoundOpen();
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player"))
        {
            if (isActive) return;
            if (Input.GetKeyDown(KeyCode.F))
            {
                isActive = true;
                TextTuto.SetActive(false);
                collision.GetComponent<PlayerMove>().canMove = false;
                Invoke("loadEndGame2", 3f);

                Debug.Log(" chay story 2");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TextTuto.SetActive(false);

            //am thanh
            playSoundClose();
        }
    }
    private void loadEndGame2()
    {
        SceneManager.LoadScene("StoryEnd2");
    }


    //am thanh
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
