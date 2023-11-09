using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class Fury : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fill_Image;
    private Health _health;
    [SerializeField] private Health p_health;

    public bool Flag { get; private set; } = false;

    private float FuryGauge = 100f;
    [SerializeField] public float CurrentFuryGauge = 0f;
    [SerializeField] private float RecoveryGauge = 0.5f;
    private float DecreaseFuryValue; //�ǰ� �� �پ�� �������� 

    private Coroutine StopRecovery_co; // ������ �ڿ�ȸ�� ���ߴ� �ڷ�ƾ �ߺ����� ����
    [SerializeField] private float StopRecoveryTime = 1f;

    private void Awake()
    {
        TryGetComponent(out _health);
    }

    private void OnEnable()
    {
        _health.OnHealthChanged += CheckAttackType;
    }

    private void OnDisable()
    {
        _health.OnHealthChanged -= CheckAttackType;
    }

    private void CheckAttackType()
    {
        if (FuryGauge >= CurrentFuryGauge)
        {
            AttackType attackType = _health.LastHitType;
            HitType(attackType);
            DecreaseGauge();
            if (StopRecovery_co == null)
            {
                StopRecovery_co = StartCoroutine(StopRecovery());
            }
            else
            {
                StopRecovery_co = null;
                StopRecovery_co = StartCoroutine(StopRecovery());
            }
        }
    }

    private void FixedUpdate()
    {
        if (StopRecovery_co == null && CurrentFuryGauge <= FuryGauge && p_health.CurrentHP >= 0f)
        {
            CurrentFuryGauge += RecoveryGauge * Time.deltaTime;
        }
    }

    private void Update()
    {
        if (FuryGauge <= CurrentFuryGauge && Flag == false)
        {
            Flag = true;
        }
        slider.value = CurrentFuryGauge;

    }

    private IEnumerator StopRecovery()
    {
        yield return new WaitForSeconds(StopRecoveryTime);
        StopRecovery_co = null;
        yield break;
    }

    private void HitType(AttackType attackType)
    {
        if (attackType == AttackType.Weak)
        {
            DecreaseFuryValue = 1f;
        }
        else if (attackType == AttackType.Strong)
        {
            DecreaseFuryValue = 2f;
        }
        else
        {
            DecreaseFuryValue = 0f;
        }
    }

    private void DecreaseGauge()
    {
        if (CurrentFuryGauge < DecreaseFuryValue)
        {
            CurrentFuryGauge = 0f;
        }
        else
        {
            CurrentFuryGauge -= DecreaseFuryValue;
        }
    }

    public void OnSliderValueChanged()
    {
        float minPercentage = 0.2f; 
        float maxPercentage = 0.9f;
        float normalizedValue = Mathf.Clamp((CurrentFuryGauge - minPercentage) / (FuryGauge * maxPercentage), 0f, 1f); //�ִ�ġ1f�� ���� �ʰ��ϰ� 20�ۼ�Ʈ���� ���ϰ� 90�ۼ�Ʈ���� �ִ�ġ
        float RedValue = Mathf.Lerp(100f, 255f, normalizedValue);
        float GreenValue = Mathf.Lerp(255f, 0f, normalizedValue);
        Color newColor = new Color(RedValue / 255f, GreenValue / 255f, 0f);
        slider.fillRect.GetComponent<Image>().color = newColor;
    }

}