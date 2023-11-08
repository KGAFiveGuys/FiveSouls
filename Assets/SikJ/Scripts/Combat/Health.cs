using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field:Header("Value")]
    [field:SerializeField] public float MaxHP { get; private set; } = 100f;
    public float CurrentHP { get; private set; }

    public event Action OnHealthChanged;
    public event Action OnDead;

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    public void GetDamage(float damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        OnHealthChanged?.Invoke();

        if (CurrentHP <= 0)
            OnDead?.Invoke();
    }
}
