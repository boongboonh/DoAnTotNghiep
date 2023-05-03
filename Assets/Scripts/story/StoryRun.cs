using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryRun : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // van ban chay
    public List<string> dialogue;

    private int index = 0;
    public float wordSpeed;
    public GameObject speedUpButton;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image sliderLoading;

    private bool runNextText;

    [Header("sound text story")]
    [SerializeField] private AudioSource textSound;


    List<AsyncOperation> scenesToLoad = new List<AsyncOperation>();

    void Start()
    {
        runNextText = false;
        dialogueText.text = "";
        StartCoroutine(Typing());
    }

    void Update()
    {
        if (!runNextText) return;    //neu khong cho chay tiep cau tiep theo
        
        NextLine();
    }

    // tao chu hien thi tu tu
    IEnumerator Typing()
    {
        runNextText = false;

        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;

            // am thanh chay chu
            textSound.Play();

            yield return new WaitForSeconds(wordSpeed);
        }

        runNextText = true;
    }

    public virtual void NextLine() // chuyen sang cau tiep theo
    {
        if (index < dialogue.Count - 1)
        {
            index++;
            dialogueText.text += "\n";
            StartCoroutine(Typing());
        }
        else
        {
            speedUpButton.SetActive(false);//tat button speed up

            //chuyen canh 
            runNextText = false;
            Debug.Log("end story");

            Invoke("ChangeScene", 2f);    //chuyen canh sau 2 giay
        }
    }

    //chuyen canh
    protected virtual void ChangeScene()
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

    //goi tron button
    public void SpeedUpButton()
    {
        wordSpeed = 0;
        speedUpButton.SetActive(false);
    }
}



