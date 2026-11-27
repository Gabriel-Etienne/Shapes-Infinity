
using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStateTest))]
[RequireComponent(typeof(PlayerInvulnerability))]
[RequireComponent(typeof(PlayerKnockback))]
[RequireComponent(typeof(PlayerDamageFeedback))]
[RequireComponent(typeof(PlayerBlink))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("References")]
    [SerializeField] private PlayerHealthVisual healthVisual;

    private PlayerStateTest playerState;
    private PlayerInvulnerability invulnerability;
    private PlayerKnockback knockback;
    private PlayerDamageFeedback damageFeedback;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    public bool IsDead => playerState.IsDead;
    public bool IsInvulnerable => invulnerability.IsInvulnerable;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        playerState = GetComponent<PlayerStateTest>();
        invulnerability = GetComponent<PlayerInvulnerability>();
        knockback = GetComponent<PlayerKnockback>();
        damageFeedback = GetComponent<PlayerDamageFeedback>();

        maxHealth = Mathf.Max(1f, maxHealth);
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        UpdateHealthVisual();
    }

    public bool TakeDamage(float damage)
    {
        Vector2 defaultSource =
            (Vector2)transform.position - Vector2.right;

        return TakeDamage(damage, defaultSource);
    }

    public bool TakeDamage(float damage, Vector2 damageSourcePosition)
    {
        if (IsDead ||
            IsInvulnerable ||
            playerState.IsDashing ||
            damage <= 0f)
        {
            return false;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        UpdateHealthVisual();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        damageFeedback.PlayDamageFeedback();

        Debug.Log(
            $"Player took {damage} damage. Health: {CurrentHealth}/{MaxHealth}"
        );

        if (CurrentHealth <= 0f)
        {
            Die();
        }
        else
        {
            knockback.Apply(damageSourcePosition);
            invulnerability.StartInvulnerability();
        }

        return true;
    }

    public bool Heal(float amount)
    {
        if (IsDead ||
            amount <= 0f ||
            CurrentHealth >= MaxHealth)
        {
            return false;
        }

        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Min(
            MaxHealth,
            CurrentHealth + amount
        );

        if (Mathf.Approximately(previousHealth, CurrentHealth))
            return false;

        UpdateHealthVisual();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        Debug.Log(
            $"Player healed {CurrentHealth - previousHealth}. " +
            $"Health: {CurrentHealth}/{MaxHealth}"
        );

        return true;
    }

    public void SetHealth(float amount)
    {
        if (IsDead)
            return;

        CurrentHealth = Mathf.Clamp(amount, 0f, MaxHealth);

        UpdateHealthVisual();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (IsDead)
            return;

        // Bloque les autres systèmes de déplacement.
        playerState.SetDead();

        // Interrompt les effets encore actifs.
        knockback.Cancel();
        invulnerability.CancelInvulnerability();

        Debug.Log("Player died!");

        OnDeath?.Invoke();
    }

    private void UpdateHealthVisual()
    {
        if (healthVisual != null)
            healthVisual.UpdateHealth(CurrentHealth, MaxHealth);
    }
}
