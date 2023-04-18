using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kunaiSword : MonoBehaviour
{
    [SerializeField] float speed = 7f;
    [SerializeField] float time = 7f;
    [SerializeField] float timeDelay = 1f;

    private bool isMove = false;

    private void Start()
    {
        StartCoroutine(delayAttack());
    }
    void Update()
    {
        move();
        Destroy(transform.gameObject, time);
    }


    IEnumerator delayAttack()
    {
        yield return new WaitForSeconds(timeDelay);
        isMove = true;
    }

    private void move() // di chuyen tu tren xuong duoi
    {
        if (!isMove) return;
        transform.position += Vector3.down * speed * Time.deltaTime;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //neu cham vao player tru 1 hp
            collision.GetComponent<HealthPlayer>().takeDame(1);

            Destroy(transform.gameObject);
        }

        if(collision.CompareTag("Ground")){
            isMove = false;
        }
    }
}
