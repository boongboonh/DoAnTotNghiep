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
        if (PlayerPrefs.GetInt(FirstPlay) != 1) return;
        continuteButton.SetActive(true);                        //neu da choi truoc do => hien thi nut tiep tuc de choi
    }


    //gawn trong button choi tiep
    public void continuteGame()
    {
        
        HideMenu();
        ShowLoadingScreen();

        PlayerPrefs.SetInt(NumberPlay, 1);          //tang so lan choi

        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene"));                                  //load giao dien game 
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));           //load nhan vat
        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));               //load giao dien HP MP
        if (PlayerPrefs.HasKey("nameMapCurrent"))
        {
            string name = PlayerPrefs.GetString("nameMapCurrent");
            Debug.Log(name);
            scenesToLoad.Add(SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive));        //load map hien tai
        }

        StartCoroutine(LoadingScreen());
        
    }

    private void HideMenu()
    {
        buttonMenu.SetActive(false);
    }

    private void ShowLoadingScreen()
    {
        loadingScreen.SetActive(true);
    }

   
    IEnumerator LoadingScreen()
    {
        Debug.Log("scene Can load " + scenesToLoad.Count);

        float totalProgress = 0f;
        for (int i = 0; i < scenesToLoad.Count; ++i)
        {
            Debug.Log("scene " + i);
            while (!scenesToLoad[i].isDone)
            {
                totalProgress += scenesToLoad[i].progress;

                sliderLoading.fillAmount = totalProgress / scenesToLoad.Count;

                yield return null;
            }
        }
    }


    /*private void startGame()
    {
        PlayerPrefs.DeleteAll();                          // xoa toan bo du lieu truoc do 

        PlayerPrefs.SetInt(FirstPlay, 1);                 //neu choi game lan dau thi la 1
        PlayerPrefs.SetFloat("FirstPlayPosX", -34.42f);
        PlayerPrefs.SetFloat("FirstPlayPosY", -5f);


        scenesToLoad.Add(SceneManager.LoadSceneAsync("Map1"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));
        
        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));

    }*/



    public void startButton()                   // gan trong button start
    {
        if(PlayerPrefs.GetInt(FirstPlay) != 1)  //neu nguoi choi chua choi thi bat dau game;
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

        HideMenu();
        ShowLoadingScreen();
        scenesToLoad.Add(SceneManager.LoadSceneAsync("StoryStart"));


        //SceneManager.LoadScene("StoryStart");

        StartCoroutine(LoadingScreen());

    }

    //mo cau hoi xac nhan choi moi
    private void openConfirmStart()
    {
        buttonMenu.SetActive(false);
        confirmStartNewGame.SetActive(true);
    }

    //gan trong button khoong ddoong ys choi moi
    public void NoStartConfirm()
    {
        buttonMenu.SetActive(true);
        confirmStartNewGame.SetActive(false);
    }
    
    //gan trong button dong y choi moi
    public void YesStartConfirm()
    {
        PlayerPrefs.DeleteAll();                // xoa toan bo du lieu truoc do 
        PlayerPrefs.SetInt(FirstPlay,0);        //dat lan choi ve 0 
        continuteButton.SetActive(false);       //an nut tiep tuc choi
        confirmStartNewGame.SetActive(false);   //tat xac nhan
        buttonMenu.SetActive(true);             //hien thi lai menu
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
        buttonMenu.SetActive(false);
        confirmQuit.SetActive(true);
    }

   
    //gan trong button khong dong ys thoat game
    public void closeQuitConfirm()
    {
        buttonMenu.SetActive(true);
        confirmQuit.SetActive(false);
    }


    //goi trong animation

}
