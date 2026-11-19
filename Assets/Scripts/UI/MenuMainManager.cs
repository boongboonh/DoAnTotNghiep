using GF.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuMainManager : MonoBehaviour
{

    [SerializeField] private GameObject buttonMenu;                     //Cac nut trong menu chinh
    [SerializeField] private GameObject confirmQuit;                    // nut hien thi cau hoi xac nhan thoat
    [SerializeField] private GameObject continuteButton;                // nut choi tiep
    [SerializeField] private GameObject confirmStartNewGame;            // nut hien thi cau hoi xac nhan choi lai game


    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image sliderLoading;


    [SerializeField] private string FirstPlay = "FirstPlay";
    [SerializeField] private string NumberPlay = "NumberPlay";

    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();


    private void Start()
    {
        if (PlayerPrefsSaveSystem.LoadInt(FirstPlay) != 1) return;
        continuteButton.SetActive(true);                        //neu da choi truoc do => hien thi nut tiep tuc de choi
    }


    //gawn trong button choi tiep
    public void continuteGame()
    {

        HideTween(buttonMenu);
        ShowLoadingScreen();

        PlayerPrefsSaveSystem.SetInt(NumberPlay, 1);          //tang so lan choi

        scenesToLoad.Add(SceneManager.LoadSceneAsync(UpdatedWorld.SceneName));

        StartCoroutine(LoadingScreen());
        
    }


    private void HideTween(GameObject gameObject)
    {
        UIPanelTween.Hide(gameObject);
    }

    private void ShowTween(GameObject gameObject)
    {
        UIPanelTween.Show(gameObject);
    }

    private void ShowLoadingScreen()
    {
        loadingScreen.SetActive(true);
    }

   
    IEnumerator LoadingScreen()
    {
        Debug.Log("scene Can load " + scenesToLoad.Count);

        while (true)
        {
            float sum = 0f;
            bool done = true;
            foreach (var op in scenesToLoad)
            {
                sum += Mathf.Clamp01(op.progress / 0.9f);     // progress dung o 0.9 khi chua kich hoat
                if (!op.isDone) done = false;
            }
            sliderLoading.fillAmount = scenesToLoad.Count > 0 ? sum / scenesToLoad.Count : 1f;
            if (done) break;
            yield return null;
        }
    }


    /*private void startGame()
    {
        PlayerPrefsSaveSystem.ClearAll();                          // xoa toan bo du lieu truoc do

        PlayerPrefsSaveSystem.SetInt(FirstPlay, 1);                 //neu choi game lan dau thi la 1
        PlayerPrefsSaveSystem.SetFloat("FirstPlayPosX", -34.42f);
        PlayerPrefsSaveSystem.SetFloat("FirstPlayPosY", -5f);


        scenesToLoad.Add(SceneManager.LoadSceneAsync("Map1"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));
        
        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));

    }*/



    public void startButton()                   // gan trong button start
    {
        if(PlayerPrefsSaveSystem.LoadInt(FirstPlay) != 1)  //neu nguoi choi chua choi thi bat dau game;
        {
            //startGame();
            startStory();

        }
        else
        {
            openConfirmStart();                 //neu nguoi choi choi roi thi xac nhan co choi lai tu dau khong
        }
    }

    private void startStory()
    {
        Debug.Log("sceneName to load: StoryStart");         //tai cot truyen

        HideTween(buttonMenu);
        ShowLoadingScreen();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("StoryStart"));


        //SceneManager.LoadScene("StoryStart");

        StartCoroutine(LoadingScreen());

    }

    //mo cau hoi xac nhan choi moi
    private void openConfirmStart()
    {
        HideTween(buttonMenu);
        ShowTween(confirmStartNewGame);
        //confirmStartNewGame.SetActive(true);
    }

    //gan trong button khoong ddoong ys choi moi
    public void NoStartConfirm()
    {
        ShowTween(buttonMenu);
        //confirmStartNewGame.SetActive(false);
        HideTween(confirmStartNewGame);
    }
    
    //gan trong button dong y choi moi
    public void YesStartConfirm()
    {
        PlayerPrefsSaveSystem.ClearAll();                // xoa toan bo du lieu truoc do
        PlayerPrefsSaveSystem.SetInt(FirstPlay,0);        //dat lan choi ve 0
        continuteButton.SetActive(false);       //an nut tiep tuc choi

        HideTween(confirmStartNewGame);

        //confirmStartNewGame.SetActive(false);   //tat xac nhan
        ShowTween(buttonMenu);             //hien thi lai menu
    }


    //gan trong button dong y thoat game
    public void quitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }

    //gan trong button thoat de mo cau hoi xac nhan
    public void openQuitConfirm()
    {
        HideTween(buttonMenu);

        ShowTween(confirmQuit);
        //confirmQuit.SetActive(true);
    }

   
    //gan trong button khong dong ys thoat game
    public void closeQuitConfirm()
    {
        ShowTween(buttonMenu);

        HideTween(confirmQuit);
        //confirmQuit.SetActive(false);
    }


    //goi trong animation

}
