using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Health))]
public class Stamina : MonoBehaviour
{
    [field: Header("Value")]
    [field: SerializeField] public float MaxStamina { get; private set; } = 100f;
    [field: SerializeField] public float CurrentStamina { get; private set; }
    [field: Header("Regenerate")]
    [field: SerializeField] public float RegenDelay { get; private set; } = 1f;
    [field: SerializeField] public float RegenRatePerSeconds { get; private set; } = 10f;
    [field: Header("Locomotion Cost")]
    [field: SerializeField] public float RunCostPerSeconds { get; private set; } = 20f;
    [field: SerializeField] public float JumpCost { get; private set; } = 5f;
    [field: SerializeField] public float RollCost { get; private set; } = 10f;
    [field: Header("Combat Cost")]
    [field: SerializeField] public float WeakAttackCost { get; private set; } = 10f;
    [field: SerializeField] public float StrongAttackCost { get; private set; } = 10f;
    [field: SerializeField] public float BlockCost { get; private set; } = 10f;

    private PlayerController playerController;
    private Health playerHealth;

    public event Action OnStaminaChanged;

    private void Awake()
    {
        TryGetComponent(out playerController);
        TryGetComponent(out playerHealth);

        CurrentStamina = MaxStamina;
    }

	private void OnEnable()
	{
        playerHealth.OnDead += () => {
            Consume(MaxStamina);
        };
	}

	[SerializeField] private float elapsedTimeAfterConsume;
	private void Update()
	{
        if (elapsedTimeAfterConsume < 10f)
            elapsedTimeAfterConsume += Time.deltaTime;
        else
            elapsedTimeAfterConsume = 10f;
        
        if(elapsedTimeAfterConsume > RegenDelay)
            ReGenerate();
	}

	public void Consume(float value)
    {
        elapsedTimeAfterConsume = 0;
        CurrentStamina = Math.Max(0, CurrentStamina - value);
        OnStaminaChanged();
    }

    private void ReGenerate()
    {
		if (!playerController.IsDead    // 생존 상태
            && !playerController.IsRun) // Default Locomotion 상태
            // To-Do : 상태조건 추가
        {
            CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + RegenRatePerSeconds * Time.deltaTime);
            OnStaminaChanged();
        }
    }
}
