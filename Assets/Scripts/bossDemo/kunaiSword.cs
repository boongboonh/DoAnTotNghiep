using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kunaiSword : MonoBehaviour
{
    [SerializeField] float speed = 7f;
    [SerializeField] float time = 7f;
    [SerializeField] float timetaget = 1f;
    [SerializeField] float timeRotate = .5f;
    [SerializeField] GameObject pointAttack;

    private bool isStartDrop = false;   //check duoc phep chay ham lao vao nguoi choi chua
    Transform playerPos;

    private void Start()
    {
        getPlayerPosition();
        pointAttack.transform.position = playerPos.position;
        StartCoroutine(rotatePlayer());
    }

    void getPlayerPosition()
    {
        playerPos = GetPosPlayer.Instance.PlayerPos;
    }

    void Update()
    {
        StartCoroutine(tagetPlayer());
        Destroy(gameObject, time);
    }
    IEnumerator rotatePlayer()
    {
        yield return new WaitForSeconds(timeRotate);
        
        rotate();
    }

    IEnumerator tagetPlayer()
    {
        yield return new WaitForSeconds(timetaget);
        move();
    }

    void rotate()
    {
        isStartDrop = false;
        //lay vi tri nguoi choi
        

        //quay 
        Vector2 vectorToTarget = gameObject.transform.position - pointAttack.transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + 180;
        gameObject.transform.rotation = Quaternion.AngleAxis(angle,Vector3.forward);

        isStartDrop = true;

    }

    private void move() // di chuyen den vi tri nguowi choi da taget
    {
        if (!isStartDrop) return;
        transform.position = Vector2.MoveTowards(transform.position, pointAttack.transform.position, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //neu cham vao player tru 1 hp
            collision.GetComponent<HealthPlayer>().takeDame(1);

            //Destroy(gameObject);
        }
    }

    // goij trong animation
    

}
