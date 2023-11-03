using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field:Header("Value")]
    // Total Max Health = MaxHP * DigitScale
    [field:SerializeField] public float MaxHP { get; private set; } = 1000f;
    // Total Current Health = CurrentHP * DigitScale
    [field: SerializeField] public float CurrentHP { get; private set; }

    public event Action OnHealthChanged;

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    public void GetDamage(float damage)
    {
        if (CurrentHP <= 0)
        {
            Debug.Log($"{gameObject.name} 사망한 상태!");
            return;
        }

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        Debug.Log($"{gameObject.name}피격! {CurrentHP}/{MaxHP}");

        OnHealthChanged();
    }
}
