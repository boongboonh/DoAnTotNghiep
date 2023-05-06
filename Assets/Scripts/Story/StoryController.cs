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
        confirmOut.transform.localScale = new Vector3(0, 0, 0);
        
        confirmOut.SetActive(true);

        LeanTween.scale(confirmOut, new Vector3(1, 1, 1), 0.5f);
    }


    public void YesOutCurrenGame()
    {

        BackToMenu();
        Debug.Log("out game");
    }

    public void NoOutCurrenGame()
    {
        //confirmOut.SetActive(false);
        LeanTween.scale(confirmOut, new Vector3(0, 0, 0), 0.5f).setOnComplete(destroyConfirmOut);

        ExitButton.SetActive(true);
    }
    private void BackToMenu()
    {
        Debug.Log("sceneName to load: Menu");         //quay ve menu
        SceneManager.LoadScene("Menu");
    }


    private void destroyConfirmOut()
    {
        confirmOut.SetActive(false);
    }
}
