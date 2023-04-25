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

    [Header("sound text story")]
    [SerializeField] private AudioSource textSound;


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
    }

    //tat hoi thoai
    //tao chu hien thi tu tu
    IEnumerator Typing()
    {
        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;

            //am thanh chay chu
            textSound.Play();

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
    }
}

