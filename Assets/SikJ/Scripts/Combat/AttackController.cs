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
    [SerializeField] private Collider attackCollider;
    [field: SerializeField] public float WeakAttackBaseDamage { get; private set; } = 10f;
    [field: SerializeField] public float StrongAttackBaseDamage { get; private set; } = 20f;

    public event Action OnWeakAttackCast;
    public event Action OnWeakAttackHit;
    public event Action OnStrongAttackCast;
    public event Action OnStrongAttackHit;

    // Animation Event
    public void TurnOnAttackCollider()
    {
        attackCollider.gameObject.SetActive(true);
        if(OnWeakAttackCast != null)
            OnWeakAttackCast();
    }

    // Animation Event
    public void TurnOffAttackCollider()
    {
        attackCollider.gameObject.SetActive(false);
    }

    public void ChangeAttackType(AttackType type)
    {
        CurrentAttackType = type;
    }

    public void Attack(Health targetHealth, float damage, bool isBlocked)
    {
        if (isBlocked == false)
        {
            switch (CurrentAttackType)
            {
                case AttackType.Weak:
                    if (OnWeakAttackHit != null)
                        OnWeakAttackHit();
                    break;
                case AttackType.Strong:
                    if (OnStrongAttackHit != null)
                        OnStrongAttackHit();
                    break;
            }
        }

        targetHealth.GetDamage(damage);
        TurnOffAttackCollider();
    }
}
