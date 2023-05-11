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
    //[SerializeField] private float colldown = 0.5f;

    [SerializeField] public LayerMask layerMask;

    /*private float timeColldown = 0;
    private bool isAttack;*/
    [HideInInspector] public static int bulletType = 1;
    ManaManager mana;
    //test
    [SerializeField] private GameObject player;

    void Start()
    {
        mana = player.GetComponent<ManaManager>();
    }

    private void FixedUpdate()
    {
        followPlayer();
    }

    void Update()
    {
        //ki nawng e ban dan chum 5 vien
        if (Input.GetKeyDown(KeyCode.E) && mana.NowMana > 0)
        {
            enemy = findEnemyRaycast();
            if (enemy != null && checkDistancePlayerEnemy())
            {
                mana.UseMana();
                StartCoroutine(skill1());
            }
            
        }
/*

        //ki nang Q tao tinh linh ban dan

        if(Input.GetKeyDown(KeyCode.Q)&& mana.NowMana > 1)
        {

        }*/
    }

    //tim kiem enemy 
    GameObject findEnemyRaycast()
    {
        Vector2 origin = transform.position; //vi tri ban

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, distanceAttack,Vector2.zero, 0 , layerMask);

        float closestDistance = Mathf.Infinity; // khoang cach den doi tuong gan nhat
        GameObject closestObject = null; // doi tuong gan nhat

        foreach (RaycastHit2D hit in hits)
        {
            // tinh khoang cach
            float distance = Vector2.Distance(origin, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance; // capp nhat lai khoang cach
                closestObject = hit.transform.gameObject; // cap nhat doi tuong
            }
        }

        return closestObject;

    }

    private void followPlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, PositionBullet.position, Time.deltaTime * speedFolow);
    }
    IEnumerator skill1()
    {
        for (int i = 0; i < 5; i++)
        {
            //am thanh
            effectSoundShoot();


            PosBulletClone = gameObject.transform;
            Instantiate(bulletPlayerShootPrfab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    /*private void CaculationColldown()
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
    }*/

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

    private void effectSoundShoot()
    {
        PlayerSounds.instance.ShootAudio();
    }
}
