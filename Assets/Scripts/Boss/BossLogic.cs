using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLogic : MonoBehaviour
{
    [SerializeField] private Transform pointAppear;

    [SerializeField] private GameObject WayShockRight;
    [SerializeField] private GameObject WayShockLeft;
    [SerializeField] private Transform pointShockWayRight;
    [SerializeField] private Transform pointShockWayLeft;

    float[] attackWeights = new float[] { 0.4f, 0.4f, 0.2f }; // trong so random <song, dap, kiem>

    public float distance = 1.78f;
    public LayerMask layerMask;

    public float speedDrop = 5f;
    [SerializeField] float heightDropAttack = 5f;           //do cao tan cong
    bool delayDropAttack = false;

    [SerializeField] Transform heightAttackKunai;           //do cao tan cong kunai

    [SerializeField] List<Transform> pointSpawnkunai;
    [SerializeField] GameObject kunai;
    bool isChangeAttack = false;

    private bool isChangePattenBoss = false;
    private Rigidbody2D rb;

    [SerializeField] int PattenOld = -1;

    Animator animator;

    public bool inRanger = false;                       //bien kiem tra trang thai nguoi choi co ow trong vung tan cong khong.

    [Header("audio patten")]
    [SerializeField] public AudioSource pattenSword;
    [SerializeField] public AudioSource pattenDrop;
    [SerializeField] public AudioSource pattenShockWay;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.keepAnimatorControllerStateOnDisable = true;
        
    }
    private void OnEnable()
    {
        //CancelInvoke("DelayChangePatten");      //huy trang thai tao lai patten
        rb.gravityScale = 0f;
        PattenOld = -1;
        gameObject.transform.SetPositionAndRotation(pointAppear.position, Quaternion.identity);

        Debug.Log("bat nhac chien dau");
        EventManager.Instance.onMusicFighting();                        //bat nhac chien dau

    }

    private void OnDisable()
    {

        Debug.Log("tat nhac chien dau");
        EventManager.Instance.offMusicFighting();       //tat nhac chien dau
    }

    //ham goi trong trigger area cho phep tan cong khi vao khu vuc
    public void ActiveAttackBoss()
    {
        Invoke("DelayAttackBoss", 2f);
        //DelayAttackBoss();
    }

    private void DelayAttackBoss()
    {
        inRanger = true;
        isChangePattenBoss = true;
    }

    void Update()
    {

        /*if (!inRanger)
        {
            animator.SetInteger("StateBoss", 0);
            return;
        }
*/

        if (isChangePattenBoss)
        {
            isChangePattenBoss = false;

            int attackIndex = randomNoDuplicate();                  //random 1 kieu tan cong khong trung lap

            switch (attackIndex)
            {
                case 0:
                    //pattenWayShockBoss();
                    if (IsGround())
                    {
                        animator.SetInteger("StateBoss", 2);
                    }
                    else
                    {
                        animator.SetInteger("StateBoss", 0);
                        Invoke("DelayChangePatten", 1f);
                    }
                    break;
                case 1:
                    //pattenAttackDrop();
                    tanCongDap();

                    break;
                case 2:
                    //pattenSword

                    tele(heightAttackKunai.position.x);                 //dich chuyen den vi tri spam kiem
                    rb.gravityScale = 0;                                //treo ow do
                    
                    //am thanh
                    pattenSword.Play();
                    animator.SetInteger("StateBoss", 3);                //chay animation        //hanh dong tan cong duoc goi trong animation
                    
                    break;

                default:
                    Debug.Log("khong co gia tri random dung");
                    break;
            }

        }
        dropAtackLogic();
    }

    //random ra 1 patten khong trung
    int randomNoDuplicate()
    {
        Debug.Log("chay ham random");
       
        int attackIndex = WeightedRandom(attackWeights);
        if(attackIndex == PattenOld)
        {
            return randomNoDuplicate();
        }
        else
        {
            PattenOld = attackIndex; //luu patten vua thuc hien
            return attackIndex;
        }

    }

    //random trong so
    int WeightedRandom(float[] weights)
    {
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.value * totalWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (randomValue < weights[i])
            {
                return i;
            }

            randomValue -= weights[i];
        }

        return weights.Length - 1;
    }

  
   

    //dich chuyen den vi tri nguoi choi
    private void tele(float x)
    {
        transform.position = new Vector2(x, heightDropAttack);
        Debug.Log("dich chuyen ");
    }

    private Transform getPlayerPos()
    {
        Transform playerPos;
        playerPos = GetPosPlayer.Instance.PlayerPos;
        if (playerPos == null)
        {
            return transform;
        }
        return playerPos;
    }

    private void dropAttack()
    {
        transform.Translate(Vector2.down * Time.deltaTime * speedDrop);
    }
    
   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthPlayer>().takeDame(0);      //can sua 
        }
    }

   
    private bool IsGround()
    {
        Debug.DrawRay(transform.position, new Vector2(0, -distance), Color.green);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, layerMask);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }


    //goi trong animation mua kiem


    public void muaKiem()
    {
        isChangeAttack = false;
        StartCoroutine(delayTaoKiem());
    }
    
    IEnumerator delayTaoKiem()
    {
        for (int i = 0; i < pointSpawnkunai.Count; i++)
        {
            yield return new WaitForSeconds(.4f);    //doi .4 giay spawn 1 chiec
            Instantiate(kunai, pointSpawnkunai[i].position, kunai.transform.rotation);
        }

        yield return new WaitUntil(() => isChangeAttack);           //doi den khi ischangeattack dung thi chay
        animator.SetInteger("StateBoss", 0);

        yield return new WaitForSeconds(1f);                        //doi them 1 giay roi dap dat
        rb.gravityScale = 1;
        
        Invoke("DelayChangePatten", 2f);                             //delay 2 giay roi cho chuyen patten quai
    }

    //ham chuyen bien chuyen trang thai tan cong. de goi trong invoke
    private void DelayChangePatten()
    {
        isChangePattenBoss = true;
    }

    //goi cuoi animation de thoat trang thai tan cong kiem
    public void changeAttackMuaKiem()
    {
        isChangeAttack = true;
    }


    //spam song goi trong animation shock way patten
    public void banSong()
    {
        //chay am thanh
        pattenShockWay.Play();

        Instantiate(WayShockLeft, pointShockWayLeft.position, Quaternion.identity);
        Instantiate(WayShockRight, pointShockWayRight.position, Quaternion.identity);
    }


    //goi cuoi animation ban song de doi animation
    public void outPattenBanSong()
    {
        //chay animation nhan doi
        animator.SetInteger("StateBoss", 0);

        //chuyen trang thai tan cong sau 2giay
        Invoke("DelayChangePatten", 2f);
    }



    //tan cong khi dap// goi trong code
    public void tanCongDap()
    {
        StartCoroutine(delayTanCongDap());
    }

    IEnumerator delayTanCongDap()
    {
        rb.gravityScale = 0;

        for (int i = 0; i < 3; i++)
        {
            tele(getPlayerPos().position.x);


            animator.SetInteger("StateBoss", 1);

            //chay am thanh chuan bi tan cong
            pattenDrop.Play();

            yield return new WaitUntil(() => IsGround());

        }
        
        
        rb.gravityScale = 1;

        animator.SetInteger("StateBoss", 0);
        Debug.Log("ket thu tan cong dap");

        Invoke("DelayChangePatten", 2f);                    //chuyen trang thai tan cong sau 2 giay nghi
    }

    // logic tan cong khi dap
    private void dropAtackLogic()
    {
        if (!delayDropAttack) return;

        dropAttack();

        if (!IsGround()) return;

        delayDropAttack = false;
    }

    //goi o cuoi hoat anh de drop
    public void activeDrop()
    {

        delayDropAttack = true;
        animator.SetInteger("StateBoss", 4);
    }

}
