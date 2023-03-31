using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testJump : MonoBehaviour
{
    [Header("object taget")]
    private Rigidbody2D rd;
    [SerializeField] public Transform target;

    [Header("setting")]
    public float h = 25;
    public float gravity = -1;

    private void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        rd.gravityScale = 0;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Lauch();
        }
    }
    void Lauch()
    {
        Physics.gravity = Vector3.up * gravity;
        rd.gravityScale = 1;
        rd.velocity= CalcylateLauchVelocity();
        Debug.Log(rd.velocity);

    }
    Vector3 CalcylateLauchVelocity()
    {
        float displaycementY = target.position.y - transform.position.y;
        Vector3 displacementXZ = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);

        float time = (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displaycementY - h)) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;
        return velocityXZ + velocityY;

    }
}
