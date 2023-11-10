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

        // ���� ���
        if (layerMask == (int)_attackController.BlockableLayer)
        {
			BlockController targetBlockController = other.gameObject.GetComponent<BlockDamage>().BlockController;
            Health targetHealth = targetBlockController.gameObject.GetComponent<Health>();
            Stamina targetStamina = targetBlockController.gameObject.GetComponent<Stamina>();
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

            var isStaminaEnough = targetStamina.CurrentStamina >= targetStamina.BlockSuccessCost;
            if (isStaminaEnough)
            {
                damage *= (1 - targetBlockController.BlockDampRate);
                targetBlockController.BlockSucceed(damage);
                _attackController.Attack(targetHealth, damage, true);
            }
            // ���� - ���¹̳� �����
            else
            {
                targetBlockController.BlockFailed();
                _attackController.Attack(targetHealth, damage, false);
            }
        }
		// ���ߵ� ���
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
