using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TeleBoxStory : MonoBehaviour
{
    [SerializeField] private GameObject TextTuto;
    

    [SerializeField] AudioSource openBoxSound;
    [SerializeField] AudioSource closeBoxSound;

    [SerializeField] private GameObject iconOut;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image fillLoading;

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
                isActive = true;
                collision.GetComponent<PlayerMoveStory>().isMove = false;       //dung di chuyen
                TextTuto.SetActive(false);
                
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
        //lay thong so am thanh
        float volumeBackground = PlayerPrefs.GetFloat("soundBackgroundMix");
        float volumeEffect = PlayerPrefs.GetFloat("soundEffectMix");

        //xoa toan bo du lieu
        PlayerPrefs.DeleteAll();                          // xoa toan bo du lieu truoc do 

        //lu lai gia tri am thanh
        PlayerPrefs.SetFloat("soundBackgroundMix", volumeBackground);
        PlayerPrefs.SetFloat("soundEffectMix", volumeEffect);

        //cai dat thong tin moi
        PlayerPrefs.SetInt(FirstPlay, 1);                 //neu choi game lan dau thi la 1
        PlayerPrefs.SetFloat("FirstPlayPosX", -34.42f);
        PlayerPrefs.SetFloat("FirstPlayPosY", -5f);

        //hien thi man hinh load game
        HideIconOut();
        ShowLoadingScreen();
        

        //load scene khong dong bo
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Map1"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));

        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));

        StartCoroutine(LoadingScreen());
    }

    private void loadEndGame1()
    {
        //hien thi man hinh load game
        HideIconOut();
        ShowLoadingScreen();

        //SceneManager.LoadScene("StoryEnd1");
        scenesToLoad.Add(SceneManager.LoadSceneAsync("StoryEnd1"));
        StartCoroutine(LoadingScreen());
    }

    private void HideIconOut()
    {
        iconOut.SetActive(false);
    }

    private void ShowLoadingScreen()
    {
        loadingScreen.SetActive(true);
    }

    IEnumerator LoadingScreen()
    {
        float totalProgress = 0;
        for (int i = 0; i < scenesToLoad.Count; i++)
        {
            while (!scenesToLoad[i].isDone)
            {
                totalProgress += scenesToLoad[i].progress;
                fillLoading.fillAmount = totalProgress / scenesToLoad.Count;
                yield return null;
            }
        }

        //yield return new WaitForSeconds(1f);
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
