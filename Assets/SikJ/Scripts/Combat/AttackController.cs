using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatLayerMask
{
    Enemy = 8,
    Player = 9,
    Shield = 11,
}

public enum AttackType
{
    Weak = 0,
    Strong = 1,
}

public class AttackController : MonoBehaviour
{
    [field: SerializeField] public AttackType CurrentAttackType { get; private set; } = AttackType.Weak;
    [field: SerializeField] public CombatLayerMask AttackLayer { get; set; } = CombatLayerMask.Enemy;
    [field: SerializeField] public CombatLayerMask BlockableLayer { get; set; } = CombatLayerMask.Shield;
    [field: SerializeField] public Collider AttackCollider { get; set; }
    [field: SerializeField] public float WeakAttackBaseDamage { get; set; } = 10f;
    [field: SerializeField] public float StrongAttackBaseDamage { get; set; } = 20f;

    private event Action OnAttackCast = null;
    private event Action OnAttackHit = null;
    public event Action OnWeakAttackCast;
    public event Action OnWeakAttackHit;
    public event Action OnStrongAttackCast;
    public event Action OnStrongAttackHit;

    // Animation Event
    public void TurnOnAttackCollider()
    {
        AttackCollider.gameObject.SetActive(true);
        OnAttackCast?.Invoke();
    }

    // Animation Event
    public void TurnOffAttackCollider()
    {
        AttackCollider.gameObject.SetActive(false);
    }

    public void ChangeAttackType(AttackType type)
    {
        CurrentAttackType = type;
        switch (CurrentAttackType)
        {
            case AttackType.Weak:
                OnAttackCast = OnWeakAttackCast;
                break;
            case AttackType.Strong:
                OnAttackCast = OnStrongAttackCast;
                break;
            default:
                break;
        }
    }

    public void Attack(Health targetHealth, float damage, bool isBlocked)
    {
        if (isBlocked == false)
        {
            switch (CurrentAttackType)
            {
                case AttackType.Weak:
                    OnAttackHit = OnWeakAttackHit;
                    break;
                case AttackType.Strong:
                    OnAttackHit = OnStrongAttackHit;
                    break;
            }
            OnAttackHit?.Invoke();
        }

        targetHealth.GetDamage(damage);
        TurnOffAttackCollider();
    }
}
