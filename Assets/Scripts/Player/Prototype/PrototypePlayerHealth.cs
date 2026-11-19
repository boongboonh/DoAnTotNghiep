using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public sealed class PrototypePlayerHealth : MonoBehaviour, ICombatReceiver
{
    [SerializeField, Min(1)] private int maxHealth = 5;
    private PlayerController controller;
    private PrototypeHealthState health;
    private TextMesh label;
    private float flashUntil;
    public int Health => health != null ? health.Current : maxHealth;
    public bool IsDead => health != null && health.IsDead;
    public event Action Died;
    public CombatFaction Faction => CombatFaction.Player;
    public bool ReceiveHit(int amount, Vector2 source, Vector2 point, CombatFaction attacker) =>
        attacker != Faction && !controller.IsDead && TryHit(amount, source);

    private int currentMax;
    public int Maximum => currentMax;

    /// <summary>Mau toi da = mau goc + thuong tien trinh. Doi gia tri thi hoi day.</summary>
    public void SetMaxBonus(int bonus)
    {
        int target = Mathf.Max(1, maxHealth + bonus);
        if (target == currentMax) return;
        currentMax = target;
        health = new PrototypeHealthState(currentMax);
        UpdateLabel();
    }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        currentMax = maxHealth;
        health = new PrototypeHealthState(maxHealth);
        label = PrototypeGeometry.Label(transform, "Player HP", new Vector2(0f, 1.45f), .075f);
        UpdateLabel();
    }

    public void TakeDamage(int amount, Vector2 hitPoint) => TryHit(amount, hitPoint);

    public bool TryHit(int amount, Vector2 source)
    {
        if (controller.ControlLocked || !health.TryDamage(amount, Time.time, controller.Config.invincibleTime)) return false;
        flashUntil = Time.time + .1f;
        if (health.IsDead)
        {
            controller.EnterDead();
            UpdateLabel();
            Died?.Invoke();
        }
        else
        {
            float direction = controller.Motor.Position.x >= source.x ? 1f : -1f;
            Vector2 force = controller.Config.hurtKnockback;
            controller.EnterHurt(new Vector2(force.x * direction, force.y));
            UpdateLabel();
        }
        DangDang.Combat.HitStop.Request(.04f);
        return true;
    }

    /// <summary>Hoi mau (vat pham). False neu da chet hoac day mau, de vat pham khong bi phi.</summary>
    public bool TryHeal(int amount)
    {
        if (controller.IsDead || health.Heal(amount) <= 0) return false;
        UpdateLabel();
        return true;
    }

    public Color DisplayColor(Color normal)
    {
        if (IsDead) return new Color(.3f, .3f, .3f);
        if (Time.time < flashUntil) return Color.white;
        if (Time.time < health.InvulnerableUntil)
            normal.a = Mathf.Repeat(Time.time, .12f) < .06f ? .35f : 1f;
        return normal;
    }

    public void ResetHealth()
    {
        health.Reset(Time.time, .45f);
        flashUntil = 0f;
        UpdateLabel();
    }

    private void LateUpdate() => label.transform.rotation = Quaternion.identity;

    private void UpdateLabel() => label.text = IsDead ? "DEAD - respawning" : "HP " + health.Current + " / " + health.Maximum;
}
