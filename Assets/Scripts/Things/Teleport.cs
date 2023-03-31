using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private GameObject DoorOutput;
    private float width;
    void Start()
    {
        width = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //lays do rong vaatj the tele
        float objectTele = collision.GetComponent<SpriteRenderer>().bounds.size.x;

        // lay huowng va cham
        var direction = transform.InverseTransformPoint(collision.transform.position); 

        if (direction.x > 0f)//right
        {
            //lay toa do va cham
            var collisionPoint = collision.ClosestPoint(transform.position);
            collision.transform.position = new Vector2(DoorOutput.transform.position.x - ((width + objectTele) / 2 + 0.02f), DoorOutput.transform.position.y - transform.position.y + collisionPoint.y);
            //cong-tru do dai vat the de khong bi trung collider khi dich chuyen
            
            Debug.Log("teleport done");

        }
        else if (direction.x < 0f)//left
        {
            var collisionPoint = collision.ClosestPoint(transform.position);
            collision.transform.position = new Vector2(DoorOutput.transform.position.x + ((width + objectTele) / 2 + 0.02f), DoorOutput.transform.position.y - transform.position.y + collisionPoint.y);
            Debug.Log("teleport done");
        }
        

        

    }

}
