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
    [field: SerializeField] public float MaxRegenPerSeconds { get; private set; } = 30f;
    [SerializeField] private float MaxRegenTimeThreshold = 2f;
    [SerializeField] private float elapsedTimeAfterConsume;
    [SerializeField] private AnimationCurve RegenLerpIntensity;

    [field: Header("Locomotion Cost")]
    [field: SerializeField] public float RunCostPerSeconds { get; private set; } = 20f;
    [field: SerializeField] public float JumpCost { get; private set; } = 5f;
    [field: SerializeField] public float JumpThreshold { get; private set; } = 2.5f;
    [field: SerializeField] public float RollCost { get; private set; } = 10f;
    [field: SerializeField] public float RollThreshold { get; private set; } = 5f;
    [field: Header("Attack Cost")]
    [field: SerializeField] public float WeakAttackCost { get; private set; } = 10f;
    [field: SerializeField] public float WeakAttackThreshold { get; private set; } = 5f;
    [field: SerializeField] public float StrongAttackCost { get; private set; } = 20f;
    [field: SerializeField] public float StrongAttackThreshold { get; private set; } = 10f;
    [field: Header("Block Cost")]
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

	private void Start()
	{
        playerHealth.OnDead += () =>
        {
            Consume(MaxStamina);
        };
	}

	private void Update()
	{
        if (elapsedTimeAfterConsume < MaxRegenTimeThreshold)
            elapsedTimeAfterConsume += Time.deltaTime;
        else
            elapsedTimeAfterConsume = MaxRegenTimeThreshold;
        
        if(!playerController.IsDead && elapsedTimeAfterConsume > RegenDelay)
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
            var intensity = RegenLerpIntensity.Evaluate(elapsedTimeAfterConsume / MaxRegenTimeThreshold);
            var targetStamina = CurrentStamina + intensity * MaxRegenPerSeconds * Time.deltaTime;
            CurrentStamina = Mathf.Min(MaxStamina, targetStamina);
            OnStaminaChanged();
        }
    }
}
