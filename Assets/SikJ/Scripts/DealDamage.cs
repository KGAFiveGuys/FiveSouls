using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    [SerializeField] private AttackController dealDamage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != (int)dealDamage.TargetLayer)
            return;

        var targetHealth = other.gameObject.GetComponent<Health>();

        float damage = 0f;
        switch (dealDamage.CurrentAttackType)
        {
            case AttackType.Weak:
                damage = dealDamage.WeakAttackBaseDamage;
                break;
            case AttackType.Strong:
                damage = dealDamage.StrongAttackBaseDamage;
                break;
        }

        dealDamage.Damage(targetHealth, damage);
    }
}
