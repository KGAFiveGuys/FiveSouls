using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;

    [field:Header("Value")]
    [field:SerializeField] public float MaxHP { get; private set; } = 100f;
    public float CurrentHP { get; private set; }
    public AttackType LastHitType { get; set; }

    public event Action OnHealthChanged;
    public event Action OnDead;

    private void Awake()
    {
        CurrentHP = MaxHP;
        TryGetComponent(out hitCollider);
    }

    private void OnEnable()
    {
        if (hitCollider != null)
            OnDead += () => hitCollider.enabled = false;
    }

    public void GetDamage(AttackType type, float damage)
    {
        LastHitType = type;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        OnHealthChanged?.Invoke();

        if (CurrentHP <= 0)
            OnDead?.Invoke();
    }
}
