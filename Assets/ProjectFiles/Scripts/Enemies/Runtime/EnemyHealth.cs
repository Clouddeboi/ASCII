using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyHealth : MonoBehaviour, IKillable
{
    private EnemyInstance enemyInstance;
    private float currentHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => enemyInstance.Data.health.maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        enemyInstance = GetComponent<EnemyInstance>();
        currentHealth = enemyInstance.Data.health.maxHealth;
    }

    public void TakeDamage(float amount, DamageType type)
    {
        if (amount <= 0f || IsDead) return;

        float multiplier = enemyInstance.Data.GetDamageMultiplier(type);
        float finalAmount = amount * multiplier;
        if (finalAmount <= 0f) return;

        Debug.Log($"[EnemyHealth] '{name}' took {finalAmount} {type} damage ({currentHealth} -> {currentHealth - finalAmount}).", this);
        SetHealth(currentHealth - finalAmount);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead) return;
        SetHealth(currentHealth + amount);
    }

    public void Kill()
    {
        SetHealth(0f);
    }

    private void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        switch (enemyInstance.Data.health.deathBehavior)
        {
            case DeathBehavior.Disable:
                gameObject.SetActive(false);
                break;
            case DeathBehavior.PlayAnimationThenDestroy:
                Destroy(gameObject);
                break;
            case DeathBehavior.Destroy:
            default:
                Destroy(gameObject);
                break;
        }
    }
}
