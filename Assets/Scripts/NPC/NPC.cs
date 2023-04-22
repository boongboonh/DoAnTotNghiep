using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // vawn banr ddoi thoai
    public string[] dialogue;

    

    private int index = 0;
    //[SerializeField] private int numberShowOption;
    //[SerializeField] private GameObject optionChoose;

    [SerializeField] private string[] dialogueOption1;
    [SerializeField] private string[] dialogueOption2;
     

    public float wordSpeed;
    public bool playerIsClose;

    public GameObject continueButton;
    public GameObject IconTalk;
    public GameObject playerInfor;

    void Start()
    {
        dialogueText.text = "";
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F) && playerIsClose)
        {
            playerInfor.SetActive(false);
            IconTalk.SetActive(false);
            if (!dialoguePanel.activeInHierarchy)
            {
                dialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
            else if (dialogueText.text == dialogue[index])
            {
                NextLine();
            }

            
        }

        //neu chay het chu thi hien thi nut tiep tuc
        if (dialogueText.text == dialogue[index])
        {
            continueButton.SetActive(true);
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
        dialoguePanel.SetActive(false);
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
        continueButton.SetActive(false);
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = true;
            IconTalk.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = false;
            RemoveText();
            IconTalk.SetActive(false);
            playerInfor.SetActive(true);
        }
    }
}
