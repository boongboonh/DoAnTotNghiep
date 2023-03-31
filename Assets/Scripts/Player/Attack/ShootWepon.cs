using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWepon : MonoBehaviour
{
    [SerializeField] private Transform PositionBullet;
    [HideInInspector] public static GameObject enemy;
    [HideInInspector] public static Transform PosBulletClone;
    [SerializeField] private float distanceAttack = 5f;
    [SerializeField] private GameObject bulletPlayerShootPrfab;
    [SerializeField] private float speedFolow = 20f;
    [SerializeField] private float colldown = 0.5f;

    private float timeColldown = 0;
    private bool isAttack;
    [HideInInspector] public static int bulletType = 1;
    ManaManager mana;
    //test
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mana = player.GetComponent<ManaManager>();
        isAttack = false;
        timeColldown = 0;
        //enemy = FindEnemy();
    }


    public GameObject FindEnemy()
    {
        GameObject[] ene;
        ene = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in ene)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }


    void Update()
    {
        CaculationColldown();
        followPlayer();

        if (Input.GetMouseButtonDown(0) && isAttack)
        {
            enemy = FindEnemy();

            if (enemy != null && checkDistancePlayerEnemy())
            {
                timeColldown = 0;
                PosBulletClone = gameObject.transform;

                Instantiate(bulletPlayerShootPrfab, transform.position, Quaternion.identity);
            }

        }

        //ki nawng e ban dan chum 5 vien
        if (Input.GetKeyDown(KeyCode.E) && mana.NowMana > 0)
        {
            enemy = FindEnemy();
            if (enemy != null && checkDistancePlayerEnemy())
            {
                mana.UseMana();
                StartCoroutine(skill1());
            }
            
        }
    }

    private void followPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, PositionBullet.position, Time.deltaTime * speedFolow);
    }
    IEnumerator skill1()
    {
        for (int i = 0; i < 5; i++)
        {
            PosBulletClone = gameObject.transform;
            Instantiate(bulletPlayerShootPrfab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private void CaculationColldown()
    {
        if (timeColldown >= colldown)
        {
            isAttack = true;
        }
        else
        {
            timeColldown += Time.deltaTime;
            isAttack = false;
        }
    }

    private bool checkDistancePlayerEnemy()
    {
        if (Vector2.Distance(PositionBullet.position, enemy.transform.position) <= distanceAttack)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceAttack);
    }
}
