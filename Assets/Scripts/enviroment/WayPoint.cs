using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;
    //khởi tạo vị trí hiện tại đang tiến đến

    private int currentWaypointIndex = 0;

    //tốc độ di chuyển của platform
    [SerializeField] private float speed = 2f;


    //thoi gian dung khi cham waypoint
    [SerializeField] private float timedelay = 0f;

    public void Update()
    {
        changePoint();
        move();
        
    }
    private void changePoint()
    {
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < 0.1f)
        {
            currentWaypointIndex++;
            StartCoroutine(delay());

            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }

    }

    private void move()
    {
        //movetoward di chuyển đối tượng đến đối tượng chỉ định
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, Time.deltaTime * speed);

    }

    

    IEnumerator delay() //dung doi tuong  1 time thì bat dau di chuyen tiep
    {
        float temp = speed;
        speed = 0;
        yield return new WaitForSeconds(timedelay);
        speed = temp;
    }

}
