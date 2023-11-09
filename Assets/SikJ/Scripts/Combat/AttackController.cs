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
    Counter = 2,
}

public class AttackController : MonoBehaviour
{
    [field:SerializeField] public bool IsCounterAttack { get; set; } = false;

    [field:SerializeField] public AttackType CurrentAttackType { get; private set; } = AttackType.Weak;
    [field:SerializeField] public CombatLayerMask AttackLayer { get; set; } = CombatLayerMask.Enemy;
    [field:SerializeField] public CombatLayerMask BlockableLayer { get; set; } = CombatLayerMask.Shield;
    [field:SerializeField] public Collider AttackCollider { get; set; }
    [field:SerializeField] public float WeakAttackBaseDamage { get; set; } = 10f;
    [field:SerializeField] public float StrongAttackBaseDamage { get; set; } = 20f;

    private event Action _OnAttackCast = null;
    private event Action _OnAttackHit = null;

    public event Action OnWeakAttackCast = null;
    public event Action OnWeakAttackHit = null;
    public event Action OnStrongAttackCast = null;
    public event Action OnStrongAttackHit = null;
    public event Action OnCounterAttackCast = null;
    public event Action OnCounterAttackHit = null;

    // Animation Event
    public void TurnOnAttackCollider()
    {
        if (AttackCollider == null)
            return;

        AttackCollider.gameObject.SetActive(true);
        _OnAttackCast?.Invoke();
    }

    // Animation Event
    public void TurnOffAttackCollider()
    {
        if (AttackCollider == null)
            return;

        AttackCollider.gameObject.SetActive(false);
    }

    public void ChangeAttackType(AttackType type)
    {
        _OnAttackCast = null;

        CurrentAttackType = type;
        switch (CurrentAttackType)
        {
            case AttackType.Weak:
                _OnAttackCast += OnWeakAttackCast;
                break;
            case AttackType.Strong:
                _OnAttackCast += OnStrongAttackCast;
                break;
            case AttackType.Counter:
                _OnAttackCast += OnCounterAttackCast;
                break;
            default:
                break;
        }
    }

    public void Attack(Health targetHealth, float damage, bool isBlocked)
    {
        if (isBlocked == false)
        {
            _OnAttackHit = null;
            switch (CurrentAttackType)
            {
                case AttackType.Weak:
                    _OnAttackHit += OnWeakAttackHit;
                    break;
                case AttackType.Strong:
                    _OnAttackHit = OnStrongAttackHit;
                    break;
                // Test
                case AttackType.Counter:
                    _OnAttackHit += OnCounterAttackHit;
                    break;
            }
            _OnAttackHit?.Invoke();
        }

        targetHealth.GetDamage(CurrentAttackType, damage);
        TurnOffAttackCollider();
    }

    private IEnumerator lastTimer;
    public void StartCounterAttackTime() // Block 성공 시 호출
    {
        if (lastTimer != null)
        {
            StopCoroutine(lastTimer);
            lastTimer = null;
        }
        lastTimer = CounterAttackTimer();
        StartCoroutine(lastTimer);
    }
    public void StopCounterAttackTime()
    {
        if (lastTimer != null)
        {
            StopCoroutine(lastTimer);
            lastTimer = null;
        }
        IsCounterAttack = false;
    }

    [field:Header("카운터 공격 유지시간")]
    [field:SerializeField] public float CounterAttackThreshold { get; set; } = 1.5f;
    private IEnumerator CounterAttackTimer()
    {
        IsCounterAttack = true;

        float elapsedTime = 0f;
        while (elapsedTime < CounterAttackThreshold)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        IsCounterAttack = false;
        lastTimer = null;
    }
}
