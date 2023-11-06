using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Collider blockCollider;
    [SerializeField] private ParticleSystem blockParticle;
    [field: Tooltip("������ ���� ����")]
    [field: SerializeField] [field: Range(0f, 1f)] public float BlockDampRate { get; private set; } = .2f;
    [SerializeField] private float knockBackDurationPerDamage = .2f;
    [SerializeField] private float knockBackSpeed = 200f;
    [SerializeField] private AnimationCurve knockBackTimeSlowDownIntensity;
    

    [SerializeField] private Health characterHealth;

    private void OnEnable()
    {
        characterHealth.OnDead += () =>
        {
            if (lastKnockBack != null)
            {
                StopCoroutine(lastKnockBack);
                lastKnockBack = null;
            }
        };
    }

    // Animation Event
    public void TurnOnBlockCollider()
    {
        blockCollider.gameObject.SetActive(true);
    }

    public void TurnOffBlockCollider()
    {
        blockCollider.gameObject.SetActive(false);
    }

    public void Block(float damage)
    {
        if (characterHealth.CurrentHP == 0)
            return;

        TurnOffBlockCollider();

        // �̹� KnockBack ���� ��� �ߴ�
        // ���� ��� �� �ʿ�
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
}
