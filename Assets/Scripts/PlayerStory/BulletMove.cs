using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMove : MonoBehaviour
{
    [SerializeField] private Transform PositionBullet;
    [SerializeField] private float speedFolow = 20f;

    private void FixedUpdate()
    {
        followPlayer();
    }

    private void followPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, PositionBullet.position, Time.deltaTime * speedFolow);
    }
}
