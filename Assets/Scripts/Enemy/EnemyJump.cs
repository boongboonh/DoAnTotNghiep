using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyJump : MonoBehaviour
{
    // dang test
    private Vector3 Enemy;
    [SerializeField] GameObject Player;
    [SerializeField] private float timeJump = 2f;
    private bool isAttack = false;
    private bool AttackEnd = true;
    [SerializeField] float timeDelayAttack = 1f;
    [SerializeField] float distanceAttack = 4f;

    private Rigidbody2D rb;
    private Vector3 playerPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Enemy = transform.position;
        Player = GameObject.FindGameObjectWithTag("Player");
        isAttack = false;
        AttackEnd = true;
    }

    void Update()
    {
        Vector3 V0 = CalculateVelocity(playerPos, transform.position, timeJump);
        if (isAttack)
        {
            AttackEnd = false;
            rb.velocity = V0;
        }

        if(AttackEnd)
        {
            StartCoroutine(checkPlayerPos());
        }
    }

    IEnumerator checkPlayerPos()
    {
        yield return new WaitForSeconds(timeDelayAttack);

        float dist = Vector3.Distance(Player.transform.position, transform.position);
        if (dist <= distanceAttack)
        {
            playerPos = Player.transform.position;
            isAttack = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("enemy jump- jump player");
        }
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        //define the distance x and y first
        Vector3 distance = target - origin;
        Vector3 distance_x_z = distance;
        distance_x_z.Normalize();
        distance_x_z.y = 0;

        //creating a float that represents our distance 
        float sy = distance.y;
        float sxz = distance.magnitude;


        //calculating initial x velocity
        //Vx = x / t
        float Vxz = sxz / time;

        ////calculating initial y velocity
        //Vy0 = y/t + 1/2 * g * t
        float Vy = sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distance_x_z * Vxz;
        result.y = Vy;


        return result;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, distanceAttack);
    }
}
