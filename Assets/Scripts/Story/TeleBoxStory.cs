using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TeleBoxStory : InteractableZone
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

    // statusBox == 0: chua xong cot chuyen => chua cho tuong tac
    protected override bool CanInteract => !isActive && statusBox != 0;

    protected override void OnPlayerEnter(Collider2D player)
    {
        if (statusBox == 0) return;

        TextTuto.SetActive(true);

        //am thanh
        playSoundOpen();
    }

    protected override void OnInteract(Collider2D player)
    {
        isActive = true;
        player.GetComponent<PlayerMoveStory>().isMove = false;       //dung di chuyen
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
    }


    //load man hinh game
    private void LoadGame()
    {
        //lay thong so am thanh
        float volumeBackground = PlayerPrefsSaveSystem.LoadFloat("soundBackgroundMix");
        float volumeEffect = PlayerPrefsSaveSystem.LoadFloat("soundEffectMix");

        //xoa toan bo du lieu
        PlayerPrefsSaveSystem.ClearAll();                          // xoa toan bo du lieu truoc do

        //lu lai gia tri am thanh
        PlayerPrefsSaveSystem.SetFloat("soundBackgroundMix", volumeBackground);
        PlayerPrefsSaveSystem.SetFloat("soundEffectMix", volumeEffect);

        //cai dat thong tin moi
        PlayerPrefsSaveSystem.SetInt(FirstPlay, 1);                 //neu choi game lan dau thi la 1
        UpdatedWorld.StartNewGame();

        //hien thi man hinh load game
        HideIconOut();
        ShowLoadingScreen();
        

        //load scene khong dong bo
        scenesToLoad.Add(SceneManager.LoadSceneAsync(UpdatedWorld.SceneName));

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
        while (true)
        {
            float sum = 0f;
            bool done = true;
            foreach (var op in scenesToLoad)
            {
                sum += Mathf.Clamp01(op.progress / 0.9f);     // progress dung o 0.9 khi chua kich hoat
                if (!op.isDone) done = false;
            }
            fillLoading.fillAmount = scenesToLoad.Count > 0 ? sum / scenesToLoad.Count : 1f;
            if (done) break;
            yield return null;
        }

        //yield return new WaitForSeconds(1f);
    }

    protected override void OnPlayerExit(Collider2D player)
    {
        if (statusBox == 0) return;

        TextTuto.SetActive(false);
        //am thanh
        playSoundClose();
    }


    private void playSoundOpen()
    {
        AudioManager.PlaySource(openBoxSound, true);
    }
    private void playSoundClose()
    {
        AudioManager.PlaySource(closeBoxSound, true);
    }
}
