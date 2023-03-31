using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCuaNhayMiniAI : MonoBehaviour
{
    [Header("setting jump move")]
    public float ForceJumpMovex = 2f;
    public float ForceJumpMovey = 5f;
    private float direction = 1f;

    private Transform targetObj;

    Animator _animator;
    private Rigidbody2D _rb;
    public LayerMask layerGround;
    Collider2D colli;
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        colli = GetComponent<Collider2D>();
    }

    void Update()
    {
        updateAnimation();

        if (!IsGround()) return;

        targetObj = GetPosPlayer.Instance.PlayerPos;
        if (targetObj == null) return;
           
        jump(checkDirectionTarget(direction));

    }

    public bool IsGround()
    {
        float ExtraHight = 0.03f;
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(colli.bounds.center, colli.bounds.size, 0f, Vector2.down, ExtraHight, layerGround);
        return raycastHit2D.collider != null;
    }

    void jump(float directionJump) // truyen vao huowng nhay - trai,+ phai;
    {
        _rb.velocity = new Vector2(ForceJumpMovex * directionJump, Random.Range(0.1f, ForceJumpMovey));
    }

    private void updateAnimation()
    {
        if (_rb.velocity.y > .1f)
        {
            _animator.SetInteger("state", 0);
        }

        if (_rb.velocity.y < -.1f)
        {
            _animator.SetInteger("state", 1);
        }
    }


    // kiem tra huong doi tuong nhay den
    private float checkDirectionTarget(float direction)
    {

        if (transform.position.x - targetObj.position.x > 0)
        {
            return -Mathf.Abs(direction); // player o ben trai
        }
        if (transform.position.x - targetObj.position.x < 0)
        {
            return Mathf.Abs(direction); // player o ben trai
        }
        return direction;
    }


}
