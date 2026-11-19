using System.Collections;
using UnityEngine;

/// <summary>
/// Phan ung khi player bi thuong: day lui + choang (qua PlayerController), bat tu co nhap nhay.
///   - Nguon sat thuong goi <see cref="Hit"/> (kem vi tri nguon de day lui dung huong).
///   - Duong cu <c>HealthPlayer.takeDame(n)</c> van dung duoc: hurt tu phan ung qua event Damaged,
///     day lui nguoc huong dang nhin.
///   - Nhap nhay ap len moi SpriteRenderer con, nen dung duoc cho nhan vat tach nhieu bo phan.
/// Mau/chet van do HealthPlayer quan ly; PlayerHurt khong giu so mau.
/// </summary>
public class PlayerHurt : MonoBehaviour
{
    [SerializeField] private HealthPlayer health;
    [SerializeField] private PlayerController controller;
    [Tooltip("De trong = tu lay tat ca SpriteRenderer con luc khoi tao.")]
    [SerializeField] private SpriteRenderer[] renderers = new SpriteRenderer[0];

    private Vector2 pendingSource;
    private bool hasPendingSource;
    private Coroutine invincibility;
    private Color[] originalColors;

    private void Awake()
    {
        if (health == null) health = GetComponent<HealthPlayer>();
        if (controller == null) controller = GetComponent<PlayerController>();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void OnEnable()
    {
        health.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        health.Damaged -= OnDamaged;
        StopInvincibility();
    }

    /// <summary>Gay sat thuong len player. Bo qua neu dang bat tu.</summary>
    /// <param name="sourcePosition">Vi tri nguon (the gioi) de day lui ra xa.</param>
    public void Hit(int damage, Vector2 sourcePosition)
    {
        if (health.Invincible) return;
        pendingSource = sourcePosition;
        hasPendingSource = true;
        health.takeDame(damage);
        hasPendingSource = false;
    }

    private void OnDamaged(int amount)
    {
        float dirX;
        if (hasPendingSource && Mathf.Abs(transform.position.x - pendingSource.x) > 0.01f)
            dirX = Mathf.Sign(transform.position.x - pendingSource.x);
        else
            dirX = controller.Motor.FacingRight ? -1f : 1f;

        PlayerMovementConfig cfg = controller.Config;
        controller.EnterHurt(new Vector2(dirX * cfg.hurtKnockback.x, cfg.hurtKnockback.y));

        StopInvincibility();
        invincibility = StartCoroutine(InvincibilityRoutine(cfg.invincibleTime, cfg.flickerInterval));
    }

    private IEnumerator InvincibilityRoutine(float duration, float interval)
    {
        health.Invincible = true;

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) originalColors[i] = renderers[i].color;

        interval = Mathf.Max(0.02f, interval);
        bool dim = true;
        for (float t = 0f; t < duration; t += interval)
        {
            SetAlpha(dim ? 0.5f : 1f);
            dim = !dim;
            yield return new WaitForSeconds(interval);
        }

        RestoreColors();
        health.Invincible = false;
        invincibility = null;
    }

    private void StopInvincibility()
    {
        if (invincibility != null)
        {
            StopCoroutine(invincibility);
            invincibility = null;
        }
        RestoreColors();
        if (health != null) health.Invincible = false;
    }

    private void SetAlpha(float a)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = originalColors[i];
            c.a = originalColors[i].a * a;
            renderers[i].color = c;
        }
    }

    private void RestoreColors()
    {
        if (originalColors == null) return;
        for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
            if (renderers[i] != null) renderers[i].color = originalColors[i];
        originalColors = null;
    }
}
