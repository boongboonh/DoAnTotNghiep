using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLogic : MonoBehaviour
{
    [SerializeField] private List<GameObject> listWayShock;
    private BoxCollider2D coll;

    public LayerMask layerMask;
    private bool isDropAttack = false;

    public float speedDrop = 5f;
    [SerializeField] float timeDelayAttackDrop = 0.5f;
    [SerializeField] float heightDropAttack = 5f;           //do cao tan cong

    [SerializeField] Transform heightAttackKunai;           //do cao tan cong kunai

    [SerializeField] List<Transform> pointSpawnkunai;
    [SerializeField] GameObject kunai;

    private bool isChangePattenBoss = false;
    private Rigidbody2D rb;

    [SerializeField] int PattenOld = -1;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isDropAttack)
        {
            isChangePattenBoss = true;
        }

        if (isChangePattenBoss)
        {
            isChangePattenBoss = false;

            /*float[] attackWeights = new float[] { 0.4f, 0.4f, 0.2f }; // trong so random <song, dap, kiem>
            int attackIndex = WeightedRandom(attackWeights);*/

            int attackIndex = randomNoDuplicate();

            switch (attackIndex)
            {
                case 0:
                    pattenWayShockBoss();
                    break;
                case 1:
                    pattenAttackDrop();
                    break;
                case 2:
                    pattenThreeSword();
                    break;

                default:
                    Debug.Log("khong co gia tri random dung");
                    break;
            }
        }

        dropAtackLogic();

    }

    int randomNoDuplicate()
    {
        Debug.Log("chay ham random");
        float[] attackWeights = new float[] { 0.4f, 0.4f, 0.2f }; // trong so random <song, dap, kiem>
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

   

    //goi de thuc hien tan cong dap goi 1 lan
    private void pattenAttackDrop()
    {
        StartCoroutine(attackDelayDrop());
    }

    // dung 1 nhip roi lao xuong
    IEnumerator attackDelayDrop()
    {
        rb.gravityScale = 0;
        for (int i = 0; i < 3; i++)
        {
            //doi den khi thuc hien xong lan tan cong truoc do moi tan cong tiep
            yield return new WaitUntil(() => !isDropAttack);

            tele(getPlayerPos().position.x);
            yield return new WaitForSeconds(timeDelayAttackDrop);

            isDropAttack = true;
        }

        rb.gravityScale = 1;
        isChangePattenBoss = true;
        Debug.Log("ket thu tan cong dap");
    }

    // logic tan cong khi dap
    private void dropAtackLogic()
    {

        if (!isDropAttack) return;

        dropAttack();

        if (!IsGround()) return;

        isDropAttack = false;
    }


    //dich chuyen den vi tri nguoi choi
    private void tele(float x)
    {
        transform.position = new Vector2(x, heightDropAttack);
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
    
   

    //trieu hoi 3 thanh kiem lao toi nguoi choi
    private void pattenThreeSword()
    {
        StartCoroutine(spawSword());
    }
    //done

    IEnumerator spawSword()
    {
        tele(heightAttackKunai.position.x);
        rb.gravityScale = 0;
        for (int i = 0; i < pointSpawnkunai.Count; i++)
        {
            yield return new WaitForSeconds(.5f);    //doi .5 giay spawn 1 chiec
            Instantiate(kunai, pointSpawnkunai[i].position, kunai.transform.rotation);
        }

        //doi 3 giay het patten
        yield return new WaitForSeconds(3f);
        rb.gravityScale = 1;
        isChangePattenBoss = true;

    }

    //ban song 
    private void pattenWayShockBoss()
    {

        StartCoroutine(spawnShockWay());
    }

    IEnumerator spawnShockWay()
    {
        yield return new WaitForSeconds(2f);

        if (IsGround())
        {
            for (int i = 0; i < listWayShock.Count; i++)
            {
                Instantiate(listWayShock[i], gameObject.transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(1f);
            isChangePattenBoss = true;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            isChangePattenBoss = true;
        }

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthPlayer>().takeDame(2);
        }
    }

    //check cham dat
    public bool IsGround()// tra ve true neu cham dat
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, ExtraHight, layerMask);
        return raycastHit2D.collider != null;
    }
}
