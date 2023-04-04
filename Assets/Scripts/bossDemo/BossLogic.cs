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
    private void Start()
    {
        coll = GetComponent<BoxCollider2D>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            //wayShockBoss(); //done
           
            tele();
            isDropAttack = true;
        }

        dropAtackLogic();
    }

    private void dropAtackLogic()
    {

        if (!isDropAttack) return;

        drop();

        if (!IsGround()) return;
        
        isDropAttack = false;
        Debug.Log("drop attack comple");
    }

    private void drop()
    {
        transform.parent.Translate(Vector2.down * Time.deltaTime * speedDrop);
    }

    //check cham dat
    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, ExtraHight, layerMask);
        return raycastHit2D.collider != null;
    }


    // trang thai boss

    private void logicBoss()
    {

    }

    //dich chuyen den vi tri nguoi choi
    private void tele()
    {
        transform.parent.position = new Vector2(getPlayerPos().position.x, 6);
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

    }

    //ban song 
    private void wayShockBoss()
    {
        for (int i = 0; i < listWayShock.Count; i++)
        {
            Instantiate(listWayShock[i], gameObject.transform.position , Quaternion.identity);
        }
    }

    //chem lieen tuc xung quanh
    private void chop()
    {

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthPlayer>().takeDame(2);
        }
    }
}
