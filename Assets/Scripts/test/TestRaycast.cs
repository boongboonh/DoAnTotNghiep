using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRaycast : MonoBehaviour
{
    public float distance = 3f;

    private void Update()
    {
        IsGrounded();
    }
    private void IsGrounded()
    {

        Debug.DrawRay(transform.position, new Vector2(0, -distance), Color.green);

       /* RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, layerMask);
        if (hit.collider != null)
        {
            return true;
        }

        return false;*/
    }
}
