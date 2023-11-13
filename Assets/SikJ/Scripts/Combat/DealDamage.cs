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

        // 방어된 경우
        if (layerMask == (int)_attackController.BlockableLayer)
        {
            if (!other.gameObject.TryGetComponent(out BlockController targetBlockController)
                || !other.gameObject.TryGetComponent(out Health targetHealth)
                || !other.gameObject.TryGetComponent(out Stamina targetStamina))
                return;

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
            // 실패 - 스태미나 불충분
            else
            {
                targetBlockController.BlockFailed();
                _attackController.Attack(targetHealth, damage, false);
            }
        }
		// 적중된 경우
		else if (layerMask == (int)_attackController.AttackLayer)
        {
            if (!other.gameObject.TryGetComponent(out Health targetHealth))
                return;
            
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
