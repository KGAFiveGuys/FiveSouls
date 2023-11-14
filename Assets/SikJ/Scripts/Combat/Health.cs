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

    public event Action OnHealthChanged;
    public event Action OnDead;

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    private void OnEnable()
    {
        OnDead += () => gameObject.layer = LayerMask.NameToLayer("Ghost");
    }

    public void GetDamage(AttackType type, float damage)
    {
        LastHitType = type;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        OnHealthChanged?.Invoke();

        if (CurrentHP <= 0)
            OnDead?.Invoke();
    }

    public void GetHeal(float amount)
    {
        CurrentHP += Mathf.Min(MaxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke();
    }
}
