using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMainManager : MonoBehaviour
{

    [SerializeField] private GameObject buttonMenu;                     //Cac nut trong menu chinh
    [SerializeField] private GameObject confirmQuit;                    // nut hien thi cau hoi xac nhan thoat
    [SerializeField] private GameObject continuteButton;                // nut choi tiep
    [SerializeField] private GameObject confirmStartNewGame;            // nut hien thi cau hoi xac nhan choi lai game


    [SerializeField] private string FirstPlay = "FirstPlay";

    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();


    private void Start()
    {
        if (PlayerPrefs.GetInt(FirstPlay) != 1) return;
        continuteButton.SetActive(true);                        //neu da choi truoc do => hien thi nut tiep tuc de choi

    }

    public void continuteGame()
    {
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));
    }


    private void startGame()
    {
        PlayerPrefs.SetInt(FirstPlay, 1);                 //neu choi game lan dau thi la 1
        PlayerPrefs.SetFloat("FirstPlayPosX", -34.42f);
        PlayerPrefs.SetFloat("FirstPlayPosY", -5f);


        scenesToLoad.Add(SceneManager.LoadSceneAsync("Map1"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));
        
        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));

    }

    public void startButton()                   // gan trong button start
    {
        if(PlayerPrefs.GetInt(FirstPlay) != 1)  //neu nguoi choi chua choi thi bat dau game;
        {
            startGame();
        }
        else
        {
            openConfirmStart();                 //neu nguoi choi choi roi thi xac nhan co choi lai tu dau khong
        }
    }

    private void openConfirmStart()
    {
        buttonMenu.SetActive(false);
        confirmStartNewGame.SetActive(true);
    }

    public void NoStartConfirm()
    {
        buttonMenu.SetActive(true);
        confirmStartNewGame.SetActive(false);
    }
    
    public void YesStartConfirm()
    {
        PlayerPrefs.SetInt(FirstPlay,0);        //dat lan choi ve 0 
        continuteButton.SetActive(false);       //an nut tiep tuc choi
        confirmStartNewGame.SetActive(false);   //tat xac nhan
        buttonMenu.SetActive(true);             //hien thi lai menu
    }



    public void settingGame()
    {

    }

    public void quitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }

    public void openQuitConfirm()
    {
        buttonMenu.SetActive(false);
        confirmQuit.SetActive(true);
    }

   
    public void closeQuitConfirm()
    {
        buttonMenu.SetActive(true);
        confirmQuit.SetActive(false);
    }


    //goi trong animation

}
