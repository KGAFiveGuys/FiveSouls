using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Collider blockCollider;
    [field: Tooltip("데미지 감소 비율")]
    [field: SerializeField] [field: Range(0f, 1f)] public float BlockDampRate { get; private set; } = .2f;
    [SerializeField] private float knockBackDurationPerDamage = .2f;
    [SerializeField] private float knockBackSpeed = 200f;

    // Animation Event
    public void TurnOnBlockCollider()
    {
        blockCollider.gameObject.SetActive(true);
        Debug.Log("Turn On Block Collider!!");
    }

    public void TurnOffBlockCollider()
    {
        blockCollider.gameObject.SetActive(false);
    }

    public void Block(float damage)
    {
        TurnOffBlockCollider();
        Debug.Log("Turn Off Block Collider!!");

        // 이미 KnockBack 중인 경우 중단
        // 연속 방어 시 필요
        if (lastKnockBack != null)
        {
            StopCoroutine(lastKnockBack);
            lastKnockBack = null;
        }

        lastKnockBack = KnockBack(damage);
        StartCoroutine(lastKnockBack);
    }

    private IEnumerator lastKnockBack;
    private IEnumerator KnockBack(float damage)
    {
        float elapsedTime = 0f;
        float duration = damage * knockBackDurationPerDamage;
        Debug.Log($"Duration:{duration}");
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.Translate(-transform.forward * knockBackSpeed * Time.deltaTime, Space.World);
            Debug.DrawLine(transform.position, transform.position + -transform.forward * knockBackSpeed, Color.red);
            yield return null;
        }
        lastKnockBack = null;
    }
}
