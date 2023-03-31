using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletPlayer : MonoBehaviour
{
    private Transform bulletPosition;
    private float playerX;
    private float enemyX;

    public GameObject effectBullet;

    private float dist;
    private float nextX;
    private float baseY;
    private float height;

    private int bulletType = 1;

    private float a = -1f;
    

    GameObject enemy;

    [SerializeField] private float speed = 30f;

    private void Start()
    {
        bulletType = ShootWepon.bulletType;
        enemy = ShootWepon.enemy;
        bulletPosition = ShootWepon.PosBulletClone;
        a = randomBullet();

        if (enemy == null)
        {
            Destroy(gameObject);
        }
    }


    private float randomBullet()
    {
        float rand;
        if (bulletType == 1)
        {
            rand = Random.Range(-0.1f, -1f);
            ShootWepon.bulletType = 2;
        }
        else
        {
            rand = Random.Range(0.1f, 1f);
            ShootWepon.bulletType = 1;
        }

        return rand;
    }
    private void Update()
    {
        if (enemy == null || Vector2.Distance(gameObject.transform.position,enemy.transform.position)<0.1f)
        {
            destroyBullet();
        }

        playerX = bulletPosition.position.x;
        enemyX = enemy.transform.position.x;

        dist = enemyX - playerX;
        nextX = Mathf.MoveTowards(transform.position.x, enemyX, speed * Time.deltaTime);
        baseY = Mathf.Lerp(bulletPosition.position.y, enemy.transform.position.y, (nextX - playerX) / dist);
        height = 2 * (nextX - playerX) * (nextX - enemyX) / (a * dist * dist);
        Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
        transform.rotation = LookAtTarget(movePosition - transform.position);
        transform.position = movePosition;
    }

    public static Quaternion LookAtTarget(Vector2 rotation)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("hit enemy");
            destroyBullet();
        }
    }

    private void destroyBullet()
    {
        Destroy(gameObject);
        GameObject EffectClone = Instantiate(effectBullet, transform.position, Quaternion.identity);
        Destroy(EffectClone, 0.3f);
    }
}
