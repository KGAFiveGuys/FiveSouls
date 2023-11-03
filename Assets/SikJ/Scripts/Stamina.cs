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
        // 특정 값만큼 CurrentStamina 소모

        // elapsedTimeAfterConsume 초기화
    }

    private void ReGenerate()
    {
        // !PlayerController.IsRun && RegenerateDelay > elapsedTimeAfterConsume 이면 재생

        // Coroutine 저장 필요
    }
}
