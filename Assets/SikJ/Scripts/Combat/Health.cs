using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field:Header("Value")]
    [field:SerializeField] public float MaxHP { get; private set; } = 100f;
    public float CurrentHP { get; private set; }
    [field:SerializeField] public AttackType LastHitType { get; set; }

    /// <summary>
    /// Update UI
    /// </summary>
    public event Action OnHealthChanged;
    /// <summary>
    /// Interrupt combat actions
    /// </summary>
    public event Action<AttackType> OnAttackHit;
    public event Action OnDead;

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    private void OnEnable()
    {
        OnDead += () => gameObject.layer = LayerMask.NameToLayer("Ghost");
    }

    public void GetDamage(AttackType type, float damage, bool isBlocked)
    {
        LastHitType = type;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        if (CurrentHP > 0 && !isBlocked)
            OnAttackHit?.Invoke(type);

        OnHealthChanged?.Invoke();

        if (CurrentHP <= 0)
            OnDead?.Invoke();
    }

    public void GetHeal(float amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke();
    }
}
