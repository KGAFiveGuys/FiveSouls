using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [field: Header("Value")]
    [field: SerializeField] public float MaxStamina { get; private set; } = 100f;
    [field: SerializeField] public float CurrentStamina { get; private set; }
    [field: SerializeField] public float RegenerateDelay { get; private set; } = 3f;

    [Header("UI")]
    [SerializeField] private float elapsedTimeAfterConsume = 0f;
    [SerializeField] private float backgroundDelay = 1f;
    [SerializeField] private Slider Foreground;
    [SerializeField] private Slider Background;

    public event Action OnStaminaChanged;

    private void Awake()
    {
        CurrentStamina = MaxStamina;
    }

    public void Consume(float value)
    {
        // Ư�� ����ŭ CurrentStamina �Ҹ�

        // elapsedTimeAfterConsume �ʱ�ȭ
    }

    private void ReGenerate()
    {
        // !PlayerController.IsRun && RegenerateDelay > elapsedTimeAfterConsume �̸� ���

        // Coroutine ���� �ʿ�
    }
}
