using System;
using UnityEngine;

/// <summary>
/// Controle simples de vida com eventos para UI.
/// </summary>
public class Health : MonoBehaviour
{
    [Tooltip("Vida máxima.")]
    public int maxHealth = 100;

    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    private void Start()
    {
        NotifyChange();
    }

    public void ResetHealth()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
        NotifyChange();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        NotifyChange();

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        NotifyChange();
    }

    private void NotifyChange()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}

