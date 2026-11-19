using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PrototypeTrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 5;
    [SerializeField] private SpriteRenderer bodyVisual;
    [SerializeField] private Transform healthFill;
    [SerializeField] private TextMesh healthLabel;
    private Rigidbody2D body;
    private Collider2D[] hitboxes;
    private Vector2 home;
    private float knockVelocity;
    private float respawnAt;
    private Color restColor;
    private Tween flash;
    public int Health { get; private set; }
    public int HitsTaken { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        home = body.position;
        hitboxes = GetComponentsInChildren<Collider2D>();
        restColor = bodyVisual.color;
        ResetDummy();
    }

    public void TakeDamage(int amount, Vector2 hitPoint)
    {
        if (Health <= 0 || amount <= 0) return;
        HitsTaken++;
        Health = Mathf.Max(0, Health - amount);
        knockVelocity = (hitPoint.x <= body.position.x ? 1f : -1f) * 3.5f;
        flash?.Kill();
        bodyVisual.color = Color.white;
        flash = DOVirtual.Color(Color.white, restColor, .16f, x => bodyVisual.color = x);
        UpdateHealth();
        if (Health == 0)
        {
            flash.Kill();
            bodyVisual.color = new Color(.3f, .3f, .3f);
            foreach (var c in hitboxes) c.enabled = false;
            respawnAt = Time.time + 1f;
        }
    }

    private void FixedUpdate()
    {
        if (Health <= 0) return;
        knockVelocity = Mathf.MoveTowards(knockVelocity, 0f, 12f * Time.fixedDeltaTime);
        float x = Mathf.Clamp(body.position.x + knockVelocity * Time.fixedDeltaTime, home.x - .8f, home.x + .8f);
        if (Mathf.Abs(knockVelocity) < .01f) x = Mathf.MoveTowards(x, home.x, .8f * Time.fixedDeltaTime);
        body.MovePosition(new Vector2(x, home.y));
    }

    public void PushAwayFrom(Vector2 source, float speed = 3.5f)
    {
        knockVelocity = (source.x <= body.position.x ? 1f : -1f) * speed;
    }

    private void Update()
    {
        if (Health == 0 && Time.time >= respawnAt) ResetDummy();
    }

    public void ResetDummy()
    {
        flash?.Kill();
        Health = maxHealth;
        HitsTaken = 0;
        knockVelocity = 0f;
        body.position = home;
        body.linearVelocity = Vector2.zero;
        bodyVisual.color = restColor;
        foreach (var c in hitboxes) c.enabled = true;
        UpdateHealth();
    }

    private void UpdateHealth()
    {
        float ratio = (float)Health / maxHealth;
        healthFill.localScale = new Vector3(ratio, 1f, 1f);
        healthFill.localPosition = new Vector3((ratio - 1f) * .5f, 0f, 0f);
        healthLabel.text = Health + " / " + maxHealth;
    }

    private void OnDisable() => flash?.Kill();
}
