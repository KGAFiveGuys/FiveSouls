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
    [field: Tooltip("������ ���� ����")]
    [field: SerializeField] [field: Range(0f, 1f)] public float BlockDampRate { get; private set; } = .2f;
    [SerializeField] private float knockBackFixedDuration = .2f;
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

        OnBlockSucceed?.Invoke();

        TurnOffBlockCollider();

        // �̹� KnockBack ���� ��� �ߴ�
        // ���� ��� �� �ʿ�
        if (lastKnockBack != null)
        {
            StopCoroutine(lastKnockBack);
            lastKnockBack = null;
        }
        lastKnockBack = KnockBack();
        StartCoroutine(lastKnockBack);

        blockParticle.Play();
        SFXManager.Instance.OnTimeSlowDown(knockBackFixedDuration);
        SFXManager.Instance.OnPlayerBlockSucceed(knockBackFixedDuration);
    }

    public void BlockFailed()
    {
        if (_characterHealth.CurrentHP == 0)
            return;

        OnBlockFailed?.Invoke();

        TurnOffBlockCollider();
    }

    private void ReadyCounterAttack()
    {
        _attackController.CounterAttackThreshold = knockBackFixedDuration;
        _attackController.StartCounterAttackTime();
    }

    private IEnumerator lastKnockBack;
    private IEnumerator KnockBack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < knockBackFixedDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.Translate(-transform.forward * knockBackSpeed * Time.deltaTime, Space.World);
            Debug.DrawLine(transform.position, transform.position + -transform.forward * knockBackSpeed, Color.red);
            
            // TimeSlowDown
            Time.timeScale = knockBackTimeSlowDownIntensity.Evaluate(elapsedTime / knockBackFixedDuration);

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
