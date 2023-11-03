using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatLayerMask
{
    Enemy = 8,
    Player = 9,
}

public enum AttackType
{
    Weak = 0,
    Strong = 1,
}

public class AttackController : MonoBehaviour
{
    [field: SerializeField] public AttackType CurrentAttackType { get; private set; } = AttackType.Weak;
    [field: Header("Value")]
    [SerializeField] private Collider attackCollider;
    [field: SerializeField] public CombatLayerMask TargetLayer { get; set; } = CombatLayerMask.Enemy;
    [field: SerializeField] public float WeakAttackBaseDamage { get; private set; } = 10f;
    [field: SerializeField] public float StrongAttackBaseDamage { get; private set; } = 20f;

    // Animation Event
    public void TurnOnAttackCollider()
    {
        attackCollider.gameObject.SetActive(true);
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

    public void Damage(Health targetHealth, float damage)
    {
        targetHealth.GetDamage(damage);
        TurnOffAttackCollider();
    }
}
