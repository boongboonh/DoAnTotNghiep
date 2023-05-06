using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointMini : MonoBehaviour
{
    [SerializeField] private Transform waypointUp;
    [SerializeField] private Transform waypointDown;
    //khởi tạo vị trí hiện tại đang tiến đến

    //tốc độ di chuyển của platform
    [SerializeField] private float speed = 2f;
    private float speedclone = 2f;


    //thoi gian dung khi cham waypoint
    [SerializeField] private float timedelay = 2f;

    private void Start()
    {
        speedclone = speed;
    }
    private void OnEnable()
    {
        speed = speedclone;
    }
    private void FixedUpdate()
    {
        changePoint();
        move();
    }
    private void changePoint()
    {
        if (Vector2.Distance(waypointUp.position, transform.position) <= 0.1f)
        {
            StartCoroutine(delay());
        }
    }


    private void reMove()
    {
        gameObject.transform.SetPositionAndRotation(waypointDown.position, Quaternion.identity);
        gameObject.SetActive(true);
    }

    private void move()
    {
        //movetoward di chuyển đối tượng đến đối tượng chỉ định
        transform.position = Vector2.MoveTowards(transform.position, waypointUp.transform.position, Time.fixedDeltaTime * speed);

    }



    IEnumerator delay() //dung doi tuong  1 time thì bat dau di chuyen tiep
    {
        speed = 0;
        yield return new WaitForSeconds(timedelay);
        gameObject.SetActive(false);
        Invoke("reMove", 2f);
    }

}
