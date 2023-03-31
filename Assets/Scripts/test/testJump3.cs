using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testJump3 : MonoBehaviour
{
    public GameObject player;
    public Transform enemy;

    [SerializeField]  private float speed = 10f;
    private float playerX;
    private float enemyX;

    private float dist;
    private float nextX;
    private float baseY;
    private float height;

    [SerializeField] private bool up = true;
    
    private float a = -1f;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        a = randomBullet();
    }

    private float randomBullet()
    {
        float rand = -1f;
        if (up)
        {
            rand = Random.Range(-0.1f, -1f);
        }
        else
        {
            rand = Random.Range(0.1f, 1f);
        }

        return rand;
    }
    private void Update()
    {
        playerX = player.transform.position.x;
        enemyX = enemy.transform.position.x;

        dist = enemyX - playerX;
        nextX = Mathf.MoveTowards(transform.position.x, enemyX, speed * Time.deltaTime);
        baseY = Mathf.Lerp(player.transform.position.y, enemy.transform.position.y, (nextX - playerX) / dist);
        height = 2 * (nextX - playerX) * (nextX - enemyX) / (a * dist * dist);
        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        transform.rotation = LookAtTarget(movePosition - transform.position);
        transform.position = movePosition;
        if (transform.position == enemy.transform.position)
        {
            Debug.Log("hit enemy");
        }
    }

    public static Quaternion LookAtTarget(Vector2 rotation)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg);
    }

}
