using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryEndOneRun : StoryRun
{
    [SerializeField] private GameObject BackMenuButton;

    //viet lai ham chuyen canh
    //hien thi button quay ve menu
   protected override void ChangeScene()
    {
        BackMenuButton.SetActive(true);
    }

    //chuyen ve menu game
    public void ButtonBackToMenu()
    {
        float volumeBackground = PlayerPrefs.GetFloat("soundBackgroundMix");
        float volumeEffect = PlayerPrefs.GetFloat("soundEffectMix");

        //xoa toan bo du lieu
        PlayerPrefs.DeleteAll();                          // xoa toan bo du lieu truoc do 

        //lu lai gia tri am thanh
        PlayerPrefs.SetFloat("soundBackgroundMix", volumeBackground);
        PlayerPrefs.SetFloat("soundEffectMix", volumeEffect);

        Debug.Log("chuyen ve menu- ket thuc end story 1");
        SceneManager.LoadScene("Menu");
    }
}
