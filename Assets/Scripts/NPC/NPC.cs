using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // vawn banr ddoi thoai
    [SerializeField] List<string> dialogue;

    private int numberTalk = 0;         //so lan noi chuyen

    private int index = 0;              //chi so dong dang hien thi

    //[SerializeField] private int numberShowOption;
    //[SerializeField] private GameObject optionChoose;

    [SerializeField] List<string> dialogueOption1;
    [SerializeField] List<string> dialogueOption2;

    [SerializeField] List<string> dialogueEnd;

    List<string> currenDialogue;                        //luu danh sach thoai chay hien tai

    bool isClicked = false;

    public float wordSpeed;
    private bool playerIsClose;

    private bool hideContinue = true;

    public GameObject continueButton;
    public GameObject IconTalk;
    public GameObject exitButton;

    [SerializeField] private GameObject optionQuestion;     //Dap an lua chon tra loi

    [SerializeField] GameObject teleBox;

    [SerializeField] private GameObject player;

    void Start()
    {
        dialogueText.text = "";
    }

    void Update()
    {

        if (!playerIsClose) return;
        if (isClicked) return;

        switch (numberTalk)
        {
            case 0:
                {
                    currenDialogue = dialogue;  //truyen danh sach vao
                    break;
                }
            case 1:
                {
                    currenDialogue = dialogueEnd;  //truyen hoi thoai ket thuc vao
                    break;
                }
            default:
                {
                    Debug.Log("loi so lan noi chuyen");
                    break;
                }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            
            numberTalk = 1; //so lan noi chuyen la 1
            player.GetComponent<PlayerMoveStory>().isMove = false;  //khong cho nguwoi choi di chuyen
            hideContinue = false;
            exitButton.SetActive(false);//khong cho thoat

            isClicked = true;
            IconTalk.SetActive(false);
            RunText();
        }
    }

    private void RunText()
    {
        RemoveText();   //lam moi bang ghi

        if (!dialoguePanel.activeInHierarchy)
        {
            dialoguePanel.SetActive(true);
            StartCoroutine(Typing());
        }
        else if (dialogueText.text == currenDialogue[index])
        {
            NextLine();
        }
    }

    private void FixedUpdate()
    {
        if (!playerIsClose) return;

        if (!dialoguePanel.activeInHierarchy)
        {
            player.GetComponent<PlayerMoveStory>().isMove = true;
        }

        if (hideContinue) return;
        //neu chay het chu thi hien thi nut tiep tuc
        if (dialogueText.text == currenDialogue[index])
        {
            continueButton.SetActive(true);
        }
    }

    bool checkOpenOption()
    {
        if (currenDialogue[index + 1] != "") return false;
        
        
        return true;
 
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
        foreach (char letter in currenDialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine() // chuyen sang cau tiep theo
    {
        continueButton.SetActive(false);
        if (index < currenDialogue.Count - 1)
        {
            if (checkOpenOption()) {
                hideContinue = true;
                continueButton.SetActive(false);
                optionQuestion.SetActive(true);
                Debug.Log("Chay lua chon");
                return;
            }

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
        Debug.Log(collision.name);

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
            isClicked = false;
            exitButton.SetActive(true);     //hien thi lai
            playerIsClose = false;
            RemoveText();
            IconTalk.SetActive(false);
        }
    }

    //goi trong button

    public void AgreeOptionButton()       //dong y nhan nhiem vu
    {
        hideContinue = false;
        currenDialogue = dialogueOption1;
        ActiveTeleBoxStart();
        RunText();

        optionQuestion.SetActive(false);
    }

    public void RejectOptionButton()        //tu choi nhan nhiem vu
    {
        hideContinue = false;
        currenDialogue = dialogueOption2;
        ActiveTeleBoxEnd();
        RunText();

        optionQuestion.SetActive(false);
    }

    private void ActiveTeleBoxEnd()     //truyen tham so ket thuc truyen
    {
        teleBox.GetComponent<TeleBoxStory>().statusBox = 2;
        Debug.Log("Cong ket thuc game");
    }

    private void ActiveTeleBoxStart()   //truyen tham so bat dau game
    {
        teleBox.GetComponent<TeleBoxStory>().statusBox = 1;
        Debug.Log("cong dich chuyen game");
    }
}
