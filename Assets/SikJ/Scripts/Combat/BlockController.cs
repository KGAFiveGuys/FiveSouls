using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Stamina))]
[RequireComponent(typeof(AttackController))]
public class BlockController : MonoBehaviour
{
    [SerializeField] private Collider blockCollider;
    [SerializeField] private ParticleSystem blockParticle;
    [field: Tooltip("데미지 감소 비율")]
    [field: SerializeField] [field: Range(0f, 1f)] public float BlockDampRate { get; private set; } = .2f;
    [SerializeField] private float knockBackDurationPerDamage = .2f;
    [SerializeField] private float knockBackSpeed = 200f;
    [SerializeField] private AnimationCurve knockBackTimeSlowDownIntensity;

    private Health _characterHealth;
    private Stamina _characterStamina;
    private AttackController _attackController;

    public event Action OnBlockCast;
    public event Action OnBlockSucceed;
    public event Action OnBlockFailed;
    public event Action OnKnockBackFinished;

    private void Awake()
    {
        TryGetComponent(out _characterHealth);
        TryGetComponent(out _characterStamina);
        TryGetComponent(out _attackController);
    }

    private void OnEnable()
    {
        OnBlockSucceed += ReadyCounterAttack;
        OnBlockSucceed += UseStaminaOnBlockSucceed;
        _characterHealth.OnDead += StopKnockBack;
    }

    private void OnDisable()
    {
        OnBlockSucceed -= ReadyCounterAttack;
        OnBlockSucceed -= UseStaminaOnBlockSucceed;
        _characterHealth.OnDead -= StopKnockBack;
    }

    // Animation Event
    public void TurnOnBlockCollider()
    {
        blockCollider.gameObject.SetActive(true);
        OnBlockCast?.Invoke();
    }

    public void TurnOffBlockCollider()
    {
        blockCollider.gameObject.SetActive(false);
    }

    private void UseStaminaOnBlockSucceed()
	{
        if (_characterStamina != null)
            _characterStamina.Consume(_characterStamina.BlockSuccessCost);
	}

    public void BlockSucceed(float damage)
    {
        if (_characterHealth.CurrentHP == 0)
            return;

        var duration = damage * knockBackDurationPerDamage;
        knockBackDuration = duration;
        OnBlockSucceed?.Invoke();

        TurnOffBlockCollider();

        // 이미 KnockBack 중인 경우 중단
        // 연속 방어 시 필요
        if (lastKnockBack != null)
        {
            StopCoroutine(lastKnockBack);
            lastKnockBack = null;
        }
        lastKnockBack = KnockBack();
        StartCoroutine(lastKnockBack);

        blockParticle.Play();
        SFXManager.Instance.OnTimeSlowDown(duration);
        SFXManager.Instance.OnPlayerBlockSucceed(duration);
    }

    public void BlockFailed()
    {
        if (_characterHealth.CurrentHP == 0)
            return;

        OnBlockFailed?.Invoke();

        TurnOffBlockCollider();

        SFXManager.Instance.OnPlayerBlockFailed();
    }

    private void ReadyCounterAttack()
    {
        _attackController.CounterAttackThreshold = knockBackDuration;
        _attackController.StartCounterAttackTime();
    }

    private IEnumerator lastKnockBack;
    [SerializeField] private float knockBackDuration;
    private IEnumerator KnockBack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < knockBackDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.Translate(-transform.forward * knockBackSpeed * Time.deltaTime, Space.World);
            Debug.DrawLine(transform.position, transform.position + -transform.forward * knockBackSpeed, Color.red);
            
            // TimeSlowDown
            Time.timeScale = knockBackTimeSlowDownIntensity.Evaluate(elapsedTime / knockBackDuration);

            yield return null;
        }
        Time.timeScale = 1f;
        lastKnockBack = null;

        OnKnockBackFinished();
    }

    public void StopKnockBack()
    {
        if (lastKnockBack != null)
        {
            StopCoroutine(lastKnockBack);
            Time.timeScale = 1f;
            lastKnockBack = null;
        }

        if(!_attackController.IsCounterAttack)
            OnKnockBackFinished();
    }
}
