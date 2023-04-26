using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryController : MonoBehaviour
{
    [SerializeField] private GameObject ExitButton;
    [SerializeField] private GameObject confirmOut;


    public void ExitOpenConfirmOut()
    {
        ExitButton.SetActive(false);
        confirmOut.SetActive(true);
    }


    public void YesOutCurrenGame()
    {

        BackToMenu();
        Debug.Log("out game");
    }

    public void NoOutCurrenGame()
    {
        confirmOut.SetActive(false);
        ExitButton.SetActive(true);
    }
    private void BackToMenu()
    {
        Debug.Log("sceneName to load: Menu");         //quay ve menu
        SceneManager.LoadScene("Menu");
    }

}
