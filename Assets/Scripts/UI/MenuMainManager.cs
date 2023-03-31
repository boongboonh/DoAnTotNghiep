using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMainManager : MonoBehaviour
{

    [SerializeField] private GameObject buttonMenu;
    [SerializeField] private GameObject confirmQuit;

    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();

    public void startGame()
    {
        PlayerPrefs.SetFloat("FirstPlayPosX", -34.42f);
        PlayerPrefs.SetFloat("FirstPlayPosY", -5f);


        scenesToLoad.Add(SceneManager.LoadSceneAsync("Map1"));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("PlayerScene", LoadSceneMode.Additive));
        
        scenesToLoad.Add(SceneManager.LoadSceneAsync("LoadMapScene", LoadSceneMode.Additive));
        scenesToLoad.Add(SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive));

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
