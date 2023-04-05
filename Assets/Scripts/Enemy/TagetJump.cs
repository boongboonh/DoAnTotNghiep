using UnityEngine;

public class TagetJump : MonoBehaviour
{

    Transform Player;
    private Transform playerpos;

    private Rigidbody2D rb;
    [SerializeField] private float time = 3f;
    private bool attack;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Player = GetPosPlayer.Instance.PlayerPos;

        playerpos = Player.transform;
        attack = true;
    }
    private void Update()
    {
        Vector3 V0 = CalculateVelocity(playerpos.position, transform.position, time);
        if (attack)
        {
            attack = false;
            rb.velocity = V0;

        }
        Destroy(gameObject,5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Player"))
        {
            Debug.Log("taget jump player take dame");
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
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
}
