using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// NPC hoi thoai dang state machine (dung StateMachine theo kieu composition vi NPC da ke thua InteractableZone):
///   Idle     -> player o ngoai vung.
///   Prompt   -> player trong vung, hien icon "bam F de noi chuyen".
///   Typing   -> go chu tung ky tu cho dong thoai hien tai.
///   LineDone -> go xong dong, hien nut tiep tuc (NextLine).
///   Choice   -> dong tiep theo la "" => hien cau hoi lua chon (Agree / Reject).
///   Talked   -> da noi xong, player van trong vung; khong cho bam F lai cho den khi player roi vung.
/// Ten field public va ham goi tu button (NextLine, RemoveText, AgreeOptionButton, RejectOptionButton) giu nguyen.
/// </summary>
public class NPC : InteractableZone
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // van ban doi thoai
    [SerializeField] List<string> dialogue;

    private int numberTalk = 0;         //so lan noi chuyen

    private int index = 0;              //chi so dong dang hien thi

    [SerializeField] List<string> dialogueOption1;
    [SerializeField] List<string> dialogueOption2;

    [SerializeField] List<string> dialogueEnd;

    List<string> currenDialogue;                        //luu danh sach thoai chay hien tai

    public float wordSpeed;

    public GameObject continueButton;
    public GameObject IconTalk;
    public GameObject exitButton;

    [SerializeField] private GameObject optionQuestion;     //Dap an lua chon tra loi

    [SerializeField] GameObject teleBox;

    [SerializeField] private GameObject player;

    private StateMachine<NPC> machine;
    private PlayerMoveStory playerMove;

    private PlayerMoveStory PlayerMove
    {
        get
        {
            if (playerMove == null && player != null) playerMove = player.GetComponent<PlayerMoveStory>();
            return playerMove;
        }
    }

    // chi cho bam F khi dang o Prompt (khong dang noi chuyen / da noi xong cho den khi player roi vung roi quay lai)
    protected override bool CanInteract => machine != null && machine.IsIn<PromptState>();

    private void Awake()
    {
        machine = new StateMachine<NPC>(this);
        machine.AddState<IdleState>();
        machine.AddState<PromptState>();
        machine.AddState<TypingState>();
        machine.AddState<LineDoneState>();
        machine.AddState<ChoiceState>();
        machine.AddState<TalkedState>();
        machine.SetInitialState<IdleState>();
        // Cac state chuyen qua nhau bang su kien: OnPlayerEnter/Exit, OnInteract, NextLine, nut lua chon.
    }

    private void OnEnable()
    {
        machine.Start();
    }

    void Start()
    {
        dialogueText.text = "";
    }

    protected override void Update()
    {
        base.Update();      // doc phim tuong tac
        machine.Tick();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (InConversation()) SetPlayerMove(true);      // tat NPC giua luc noi chuyen: khong de player bi khoa
        machine.Stop();
    }

    // ---------------- States ----------------

    private class IdleState : State<NPC> { }

    private class PromptState : State<NPC>
    {
        public override void Enter() => Owner.IconTalk.SetActive(true);
    }

    private class TypingState : State<NPC>
    {
        private int typed;
        private float timer;
        private float nextCharTime;

        public override void Enter()
        {
            Owner.dialogueText.text = "";
            typed = 0;
            timer = 0f;
            nextCharTime = 0f;
        }

        public override void Tick()
        {
            string line = Owner.currenDialogue[Owner.index];

            timer += Time.deltaTime;
            bool changed = false;
            while (typed < line.Length && timer >= nextCharTime)
            {
                typed++;
                nextCharTime += Owner.wordSpeed;
                changed = true;
            }
            if (changed) Owner.dialogueText.text = line.Substring(0, typed);

            //go het chu thi cho phep bam tiep tuc
            if (typed >= line.Length) Machine.ChangeState<LineDoneState>();
        }
    }

    private class LineDoneState : State<NPC>
    {
        public override void Enter() => Owner.continueButton.SetActive(true);
        public override void Exit() => Owner.continueButton.SetActive(false);
    }

    private class ChoiceState : State<NPC>
    {
        public override void Enter() => Owner.optionQuestion.SetActive(true);
        public override void Exit() => Owner.optionQuestion.SetActive(false);
    }

    private class TalkedState : State<NPC>
    {
        public override void Enter() => Owner.SetPlayerMove(true);
    }

    // ---------------- Interaction ----------------

    protected override void OnPlayerEnter(Collider2D playerCol)
    {
        if (machine.IsIn<IdleState>()) machine.ChangeState<PromptState>();
    }

    protected override void OnPlayerExit(Collider2D playerCol)
    {
        if (PlayerInside) return;               // con collider khac cua player trong vung

        exitButton.SetActive(true);             //hien thi lai
        ResetDialogueUI();
        IconTalk.SetActive(false);
        SetPlayerMove(true);
        machine.ChangeState<IdleState>();
    }

    protected override void OnInteract(Collider2D playerCol)
    {
        switch (numberTalk)
        {
            case 0:
                currenDialogue = dialogue;      //truyen danh sach vao
                break;
            case 1:
                currenDialogue = dialogueEnd;   //truyen hoi thoai ket thuc vao
                break;
            default:
                Debug.Log("loi so lan noi chuyen");
                break;
        }

        numberTalk = 1; //so lan noi chuyen la 1
        SetPlayerMove(false);                   //khong cho nguoi choi di chuyen
        exitButton.SetActive(false);            //khong cho thoat
        IconTalk.SetActive(false);

        StartConversation();
    }

    private void StartConversation()
    {
        ResetDialogueUI();
        dialoguePanel.SetActive(true);
        machine.ChangeState<TypingState>(true);
    }

    private void SetPlayerMove(bool value)
    {
        if (PlayerMove != null) PlayerMove.isMove = value;
    }

    // dong thoai tiep theo la "" => mo cau hoi lua chon
    private bool checkOpenOption()
    {
        return currenDialogue[index + 1] == "";
    }

    private void ResetDialogueUI()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);
    }

    private bool InConversation()
    {
        return machine.IsIn<TypingState>() || machine.IsIn<LineDoneState>() || machine.IsIn<ChoiceState>();
    }

    // ---------------- Ham goi trong button ----------------

    //tat hoi thoai
    public void RemoveText()
    {
        ResetDialogueUI();
        if (InConversation()) machine.ChangeState<TalkedState>();
    }

    public void NextLine() // chuyen sang cau tiep theo
    {
        if (!machine.IsIn<LineDoneState>()) return;

        if (index < currenDialogue.Count - 1)
        {
            if (checkOpenOption())
            {
                machine.ChangeState<ChoiceState>();
                return;
            }

            index++;
            machine.ChangeState<TypingState>();
        }
        else
        {
            RemoveText();
        }
    }

    public void AgreeOptionButton()       //dong y nhan nhiem vu
    {
        if (!machine.IsIn<ChoiceState>()) return;

        currenDialogue = dialogueOption1;
        ActiveTeleBoxStart();
        StartConversation();
    }

    public void RejectOptionButton()        //tu choi nhan nhiem vu
    {
        if (!machine.IsIn<ChoiceState>()) return;

        currenDialogue = dialogueOption2;
        ActiveTeleBoxEnd();
        StartConversation();
    }

    private void ActiveTeleBoxEnd()     //truyen tham so ket thuc truyen
    {
        teleBox.GetComponent<TeleBoxStory>().statusBox = 2;
    }

    private void ActiveTeleBoxStart()   //truyen tham so bat dau game
    {
        teleBox.GetComponent<TeleBoxStory>().statusBox = 1;
    }
}
