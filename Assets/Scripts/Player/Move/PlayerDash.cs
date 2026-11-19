using UnityEngine;

/// <summary>
/// Ability dash cua player (thu dong): giu du lieu cau hinh, hieu ung, gravity va thoi gian hoi.
/// KHONG tu doc input hay tu chay coroutine; PlayerMove (state machine) goi BeginDash / EndDash qua DashState.
/// Ten field serialized giu nguyen de khong mat du lieu o scene/prefab.
/// </summary>
public class PlayerDash : MonoBehaviour
{
    [SerializeField] private GameObject EffectDash;

    [Header("Setting Dash")]
    [SerializeField] private float _dashingTime = 0.3f;
    [SerializeField] private float _dashForce = 20f;
    [SerializeField] private float _timeCanDash = 0.5f;

    private Rigidbody2D _rb;
    private float _baseGravity;
    private float _nextDashTime;

    public bool IsDashing { get; private set; }
    public float DashingTime => _dashingTime;

    /// <summary>Het hoi chieu va khong dang dash.</summary>
    public bool CanDash => !IsDashing && Time.time >= _nextDashTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _baseGravity = _rb.gravityScale;
    }

    private void OnEnable()
    {
        _nextDashTime = 0f;
        ResetState();
    }

    private void OnDisable()
    {
        ResetState();
    }

    /// <param name="direction">+1 sang phai, -1 sang trai.</param>
    public void BeginDash(int direction)
    {
        if (PlayerSounds.instance != null) PlayerSounds.instance.PlayDashAudio();

        if (EffectDash != null) EffectDash.SetActive(true);
        IsDashing = true;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = new Vector2(direction * _dashForce, 0f);
    }

    public void EndDash()
    {
        if (IsDashing) _nextDashTime = Time.time + _timeCanDash;
        ResetState();
    }

    private void ResetState()
    {
        if (EffectDash != null) EffectDash.SetActive(false);
        IsDashing = false;

        // chi tra gravity khi dang bi dash tat (tranh de len gravity do knockback dat)
        if (_rb != null && _rb.gravityScale == 0f) _rb.gravityScale = _baseGravity;
    }
}
