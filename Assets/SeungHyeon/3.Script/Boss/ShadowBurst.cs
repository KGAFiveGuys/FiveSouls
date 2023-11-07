using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBurst : MonoBehaviour
{
    [SerializeField]private ParticleSystem ps;
    [SerializeField] private AttackController _attackController;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other)
    {
        var layerMask = other.layer;

        if (layerMask != (int)_attackController.BlockableLayer
           && layerMask != (int)_attackController.AttackLayer)
            return;

        // 规绢等 版快
        if (layerMask == (int)_attackController.BlockableLayer)
        {
            var targetBlockController = other.GetComponent<BlockDamage>().BlockController;
            Health targetHealth = targetBlockController.gameObject.GetComponent<Health>();
            float damage = 0f;
            switch (_attackController.CurrentAttackType)
            {
                case AttackType.Weak:
                    damage = _attackController.WeakAttackBaseDamage;
                    damage *= (1 - targetBlockController.BlockDampRate);
                    break;
                case AttackType.Strong:
                    damage = _attackController.StrongAttackBaseDamage;
                    damage *= (1 - targetBlockController.BlockDampRate);
                    break;
            }

            targetBlockController.Block(damage);
            _attackController.Attack(targetHealth, damage, true);
        }
        // 利吝等 版快
        else if (layerMask == (int)_attackController.AttackLayer)
        {
            Health targetHealth = other.GetComponent<Health>();
            float damage = 0f;
            switch (_attackController.CurrentAttackType)
            {
                case AttackType.Weak:
                    damage = _attackController.WeakAttackBaseDamage;
                    break;
                case AttackType.Strong:
                    damage = _attackController.StrongAttackBaseDamage;
                    break;
            }

            _attackController.Attack(targetHealth, damage, false);
        }
        var collisionModule = ps.collision;
            collisionModule.enabled =   false;
        }
}
