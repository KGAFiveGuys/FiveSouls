using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    [SerializeField] private AttackController _attackController;

    private void OnTriggerEnter(Collider other)
    {
        var layerMask = other.gameObject.layer;

        if (layerMask != (int)_attackController.BlockableLayer
           && layerMask != (int)_attackController.AttackLayer)
            return;

        // 规绢等 版快
        if (layerMask == (int)_attackController.BlockableLayer)
        {
            var targetBlockController = other.gameObject.GetComponent<BlockDamage>().BlockController;
            Health targetHealth = targetBlockController.gameObject.GetComponent<Health>();
            float damage = 0f;
            switch (_attackController.CurrentAttackType)
            {
                case AttackType.Weak:
                    damage = _attackController.WeakAttackBaseDamage;
                    break;
                case AttackType.Strong:
                    damage = _attackController.StrongAttackBaseDamage;
                    break;
                case AttackType.Counter:
                    damage = _attackController.CounterAttackBaseDamage;
                    break;
            }
            damage *= (1 - targetBlockController.BlockDampRate);
            targetBlockController.Block(damage);
            _attackController.Attack(targetHealth, damage, true);
        }
        // 利吝等 版快
        else if (layerMask == (int)_attackController.AttackLayer)
        {
            Health targetHealth = other.gameObject.GetComponent<Health>();
            float damage = 0f;
            switch (_attackController.CurrentAttackType)
            {
                case AttackType.Weak:
                    damage = _attackController.WeakAttackBaseDamage;
                    break;
                case AttackType.Strong:
                    damage = _attackController.StrongAttackBaseDamage;
                    break;
                case AttackType.Counter:
                    damage = _attackController.CounterAttackBaseDamage;
                    break;
            }

            _attackController.Attack(targetHealth, damage, false);
        }
    }
}
