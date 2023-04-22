using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class StoryRun : MonoBehaviour
{

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // vawn banr ddoi thoai
    public string[] dialogue;



    private int index = 0;

    public float wordSpeed;

    public GameObject skipButton;


    void Start()
    {
        dialogueText.text = "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {

            StartCoroutine(Typing());
            if (dialogueText.text == dialogue[index])
            {
                NextLine();
            }
        }

        //click phim Q de tat hoi thoai
        if (Input.GetKeyDown(KeyCode.Q) && dialoguePanel.activeInHierarchy)
        {
            RemoveText();
        }
    }

    //tat hoi thoai
    public void RemoveText()
    {
        dialogueText.text = "";
        index = 0;
    }

    //tao chu hien thi tu tu
    IEnumerator Typing()
    {
        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine() // chuyen sang cau tiep theo
    {
        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StartCoroutine(Typing());
        }
        else
        {
            RemoveText();
        }
    }
}

