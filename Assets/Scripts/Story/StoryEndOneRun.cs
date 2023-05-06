using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class StoryEndOneRun : MonoBehaviour
{
    [SerializeField] private GameObject BackMenuButton;
    [SerializeField] private List<TextMeshProUGUI> listStory;
    [SerializeField] private float timeDelay = 3f;
    [SerializeField] private float timeTextAppear = 3f;
    public AnimationCurve animationCurve;                       //Duong quan ly animaion tween
    private float timeDelayStart = 3f;

    [SerializeField] GameObject buttonSkip;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image sliderLoading;

    [Header("sound text story")]
    [SerializeField] private AudioSource textSound;

    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();


    void Start()
    {
        RunStory();
    }

    private void RunStory()
    {
        StartCoroutine(Story());
    }

    IEnumerator Story()
    {
        foreach (TextMeshProUGUI text in listStory)
        {

            //chay am thanh
            textSound.Play();


            LeanTween.value(text.gameObject, (val) => updateValueExampleCallback(val, text), new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), timeTextAppear)
                    .setEase(animationCurve);
            yield return new WaitForSeconds(timeDelay);
            Debug.Log("run");
        }

        buttonSkip.SetActive(false);

        yield return new WaitForSeconds(timeDelayStart);        //thoi gian nghi de chay cot chuyen

        BackMenuButton.SetActive(true);

    }

    void updateValueExampleCallback(Color32 val, TextMeshProUGUI text)
    {
        text.color = val;
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
                sliderLoading.fillAmount = totalProgress / scenesToLoad.Count;
                yield return null;
            }
        }

        //yield return new WaitForSeconds(1f);
    }

    public void ButtonBackToMenu()
    {
        ShowLoadingScreen();
        float volumeBackground = PlayerPrefs.GetFloat("soundBackgroundMix");
        float volumeEffect = PlayerPrefs.GetFloat("soundEffectMix");

        //xoa toan bo du lieu
        PlayerPrefs.DeleteAll();                          // xoa toan bo du lieu truoc do 

        //lu lai gia tri am thanh
        PlayerPrefs.SetFloat("soundBackgroundMix", volumeBackground);
        PlayerPrefs.SetFloat("soundEffectMix", volumeEffect);

        Debug.Log("chuyen ve menu- ket thuc end story 1");
        scenesToLoad.Add(SceneManager.LoadSceneAsync("Menu"));

        StartCoroutine(LoadingScreen());
    }



    //gan vao nut bo qua
    public void BtSkip()
    {
        timeDelay = 0f;
        //timeTextAppear = 0f;
        //timeDelayStart = 5f;
        buttonSkip.SetActive(false);
        
    }

}
