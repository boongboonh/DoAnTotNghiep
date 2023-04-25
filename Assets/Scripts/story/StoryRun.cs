using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoryRun : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // van ban chay
    public List<string> dialogue;

    private int index = 0;
    public float wordSpeed;
    public GameObject speedUpButton;

    private bool runNextText;

    [Header("sound text story")]
    [SerializeField] private AudioSource textSound;

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

    public void NextLine() // chuyen sang cau tiep theo
    {
        if (index < dialogue.Count - 1)
        {
            index++;
            dialogueText.text += "\n";
            StartCoroutine(Typing());
        }
        else
        {
            //chuyen canh 
            runNextText = false;
            Debug.Log("end story");

            Invoke("ChangeScene", 2f);    //chuyen canh sau 2 giay
        }
    }

    //chuyen canh
    private void ChangeScene()
    {
        Debug.Log("chuyen den story 1");
    }

    //goi tron button
    public void SpeedUpButton()
    {
        wordSpeed = 0;
        speedUpButton.SetActive(false);
    }
}


