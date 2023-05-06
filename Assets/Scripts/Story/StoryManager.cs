using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{

    [SerializeField] private List<TextMeshProUGUI> listStory;
    [SerializeField] private float timeDelay = 4f;
    [SerializeField]  private float timeTextAppear = 3f;
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

        //tuw dong chuyen canh khi chay het cot truyen;
        ChangeScene();

    }

    void updateValueExampleCallback(Color32 val, TextMeshProUGUI text)
    {
        text.color = val;
    }


    //chuyen canh
    private void ChangeScene()
    {
        GoToStory1();
        Debug.Log("chuyen den story 1");
    }


    private void GoToStory1()
    {
        Debug.Log("sceneName to load: Story1");         //chay cot chuyen 1
        ShowLoadingScreen();

        scenesToLoad.Add(SceneManager.LoadSceneAsync("Story1"));
        //SceneManager.LoadScene("Story1");

        StartCoroutine(LoadingScreen());

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





    //gan vao nut bo qua
    public void BtSkip()
    {
        timeDelay = 0f;
        //timeTextAppear = 0f;
        timeDelayStart = 5f;
        buttonSkip.SetActive(false);
    }
}
