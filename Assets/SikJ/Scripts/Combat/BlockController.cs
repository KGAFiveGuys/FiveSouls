using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
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
    private AttackController _attackController;

    public event Action OnBlockCast;
    public event Action OnBlockSucceed;

    private void Awake()
    {
        TryGetComponent(out _characterHealth);
        TryGetComponent(out _attackController);
    }

    private void OnEnable()
    {
        OnBlockSucceed += NotifyBlockSucceed;
        _characterHealth.OnDead += StopKnockBack;
    }

    private void OnDisable()
    {
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

    public void Block(float damage)
    {
        if (_characterHealth.CurrentHP == 0)
            return;

        OnBlockSucceed?.Invoke();

        TurnOffBlockCollider();

        // 이미 KnockBack 중인 경우 중단
        // 연속 방어 시 필요
        if (lastKnockBack != null)
        {
            StopCoroutine(lastKnockBack);
            lastKnockBack = null;
        }

        var duration = damage * knockBackDurationPerDamage;
        lastKnockBack = KnockBack(duration);
        StartCoroutine(lastKnockBack);
        blockParticle.Play();
        SFXManager.Instance.OnTimeSlowDown(duration);
        SFXManager.Instance.OnPlayerBlock(duration);
    }

    private void NotifyBlockSucceed()
    {
        _attackController.StartCounterAttackTime();
    }

    private IEnumerator lastKnockBack;
    private IEnumerator KnockBack(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.Translate(-transform.forward * knockBackSpeed * Time.deltaTime, Space.World);
            Debug.DrawLine(transform.position, transform.position + -transform.forward * knockBackSpeed, Color.red);
            
            // TimeSlowDown
            Time.timeScale = knockBackTimeSlowDownIntensity.Evaluate(elapsedTime / duration);

            yield return null;
        }
        Time.timeScale = 1f;
        lastKnockBack = null;
    }

    private void StopKnockBack()
    {
        if (lastKnockBack != null)
        {
            StopCoroutine(lastKnockBack);
            lastKnockBack = null;
            Time.timeScale = 1f;
        }
    }
}
