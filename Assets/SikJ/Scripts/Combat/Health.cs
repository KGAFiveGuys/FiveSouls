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
    [SerializeField] private Collider lockOnCollider;
    [SerializeField] private Collider attackCollider;

    public event Action OnHealthChanged;
    public event Action OnDead;

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    public void GetDamage(float damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        OnHealthChanged();

        if (CurrentHP <= 0)
        {
            if (lockOnCollider != null)
                lockOnCollider.enabled = false;
            if (attackCollider != null)
                attackCollider.enabled = false;

            OnDead();
        }
    }
}
