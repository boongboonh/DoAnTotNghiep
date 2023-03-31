using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{

    private Rigidbody2D _rb;
    private PlayerMove _player;
    [SerializeField] private GameObject EffectDash;

    private float _baseGravity;

    [Header("Setting Dash")]
    [SerializeField] private float _dashingTime = 0.3f;
    [SerializeField] private float _dashForce = 20f;
    [SerializeField] private float _timeCanDash = 0.5f;
    private bool _isDashing;
    private bool _canDash = true;
    public bool IsDashing => _isDashing;



    private void OnEnable()
    {
        endDash();
    }


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<PlayerMove>();
        _baseGravity = _rb.gravityScale;

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1)&& _canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private int playerDirection(bool faceRight)
    {
        if (faceRight)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
    private IEnumerator Dash()
    {
        EffectDash.SetActive(true);
        _isDashing = true;
        _canDash = false;
        _rb.gravityScale = 0f;
        _rb.velocity = new Vector2(playerDirection(_player.Direction) * _dashForce, 0f);
        yield return new WaitForSeconds(_dashingTime);

        endDash();

        yield return new WaitForSeconds(_timeCanDash);
        
        _canDash = true;

    }

    private void endDash()
    {
        EffectDash.SetActive(false);
        _isDashing = false;
        _rb.gravityScale = _baseGravity;
    }
}
