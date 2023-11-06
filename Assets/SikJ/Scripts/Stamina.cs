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
    [SerializeField] private Slider StaminaSlider;

    private float elapsedTimeAfterConsume = 0f;

    private void Start()
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
