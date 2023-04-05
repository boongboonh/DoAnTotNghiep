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
    [SerializeField] float heightDropAttack = 6f;           //do cao tan cong

    private void Start()
    {
        coll = GetComponent<BoxCollider2D>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isDropAttack)
        {
            //wayShockBoss(); //done
            //pattenAttackDrop();// done with dropattacklogic
            
        }

        dropAtackLogic();
    }

    
    //goi de thuc hien tan cong dap goi 1 lan
    private void pattenAttackDrop()
    {
        tele();
        StartCoroutine(attackDelayDrop());
    }

    // dung 1 nhip roi lao xuong
    IEnumerator attackDelayDrop()
    {
        yield return new WaitForSeconds(timeDelayAttackDrop);
        isDropAttack = true;
    }

    // logic tan cong khi dap
    private void dropAtackLogic()
    {

        if (!isDropAttack) return;

        dropAttack();

        if (!IsGround()) return;

        isDropAttack = false;
    }


    // trang thai boss

    private void logicBoss()
    {

    }

    //dich chuyen den vi tri nguoi choi
    private void tele()
    {
        transform.parent.position = new Vector2(getPlayerPos().position.x, heightDropAttack);
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
        transform.parent.Translate(Vector2.down * Time.deltaTime * speedDrop);
    }

   

    //trieu hoi 3 thanh kiem lao toi nguoi choi
    private void threeSword()
    {

    }
    //done



    //ban song 
    private void wayShockBoss()
    {
        for (int i = 0; i < listWayShock.Count; i++)
        {
            Instantiate(listWayShock[i], gameObject.transform.position, Quaternion.identity);
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
