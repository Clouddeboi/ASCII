using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IKillable
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Damage Camera Shake")]
    [SerializeField] private float damageTrauma = 0.3f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead) return;
        SetHealth(currentHealth - amount);
        PlayerCameraEffects.Instance?.AddTrauma(damageTrauma);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead) return;
        SetHealth(currentHealth + amount);
    }

    private void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }

    //Suicide pairing entry point.
    public void Kill()
    {
        SetHealth(0f);
    }

    //Used by save/load and checkpoint respawn to set both values directly without side effects like camera trauma.
    public void SetHealthDirect(float current, float max)
    {
        maxHealth = max;
        currentHealth = Mathf.Clamp(current, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
