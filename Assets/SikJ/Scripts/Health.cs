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
        Debug.Log($"{gameObject.name}ÇÇ°Ý! {CurrentHP}/{MaxHP}");

        if (gameObject.CompareTag("Player"))
            OnHealthChanged();

        if (CurrentHP <= 0)
        {
            OnDead();
            return;
        }
    }
}
