using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TeleBoxStory : MonoBehaviour
{
    [SerializeField] private GameObject TextTuto;

    [SerializeField] AudioSource openBoxSound;
    [SerializeField] AudioSource closeBoxSound;

    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();

    [SerializeField] private string FirstPlay = "FirstPlay";

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
                            Invoke("LoadGame", 2f);
                            Debug.Log("chay game");
                            break;
                        }
                    case 2:
                        {
                            Invoke("loadEndGame1", 2f);
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


    //load man hinh game
    private void LoadGame()
    {
        PlayerPrefs.DeleteAll();                          // xoa toan bo du lieu truoc do 

        PlayerPrefs.SetInt(FirstPlay, 1);                 //neu choi game lan dau thi la 1
        PlayerPrefs.SetFloat("FirstPlayPosX", -34.42f);
        PlayerPrefs.SetFloat("FirstPlayPosY", -5f);


        scenesToLoad.Add(SceneManager.LoadSceneAsync("Map1"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));

        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));
    }

    private void loadEndGame1()
    {
        SceneManager.LoadScene("StoryEnd1");
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
