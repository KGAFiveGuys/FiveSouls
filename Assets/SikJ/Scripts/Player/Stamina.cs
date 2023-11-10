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
    [field: SerializeField] public float CounterAttackCost { get; private set; } = 30f;
    [field: SerializeField] public float CounterAttackThreshold { get; private set; } = 15f;
    [field: Header("Block Cost")]
    [field: SerializeField] public float BlockCastCost { get; private set; } = 1f;
    [field: SerializeField] public float BlockSuccessCost { get; private set; } = 10f;


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
            if (currentRegen != null)
            {
                StopCoroutine(currentRegen);
                currentRegen = null;
            }
        };
	}

	private void OnEnable()
	{
		if (currentRegen == null)
		{
            currentRegen = ReGenerateStamina();
            StartCoroutine(currentRegen);
        }
    }

	private void OnDisable()
	{
        if (currentRegen != null)
        {
            StopCoroutine(currentRegen);
            currentRegen = null;
        }
    }

	private void Update()
	{
        if (elapsedTimeAfterConsume < MaxRegenTimeThreshold)
            elapsedTimeAfterConsume = Mathf.Min(elapsedTimeAfterConsume + Time.deltaTime, MaxRegenTimeThreshold);
	}

	public void Consume(float value)
    {
        if (playerController.IsDead)
            return;

        elapsedTimeAfterConsume = 0;
        CurrentStamina = Math.Max(0, CurrentStamina - value);
        OnStaminaChanged();
    }

    private IEnumerator currentRegen;
    private IEnumerator ReGenerateStamina()
    {
		while (true)
		{
            yield return null;

            if (playerController.IsDead     // 사망한 상태
                || playerController.IsRun)  // 스태미너 회복 불가한 상태
                continue;

            if(elapsedTimeAfterConsume > RegenDelay)
			{
                var intensity = RegenLerpIntensity.Evaluate(elapsedTimeAfterConsume / MaxRegenTimeThreshold);
                var targetStamina = CurrentStamina + intensity * MaxRegenPerSeconds * Time.deltaTime;
                CurrentStamina = Mathf.Min(MaxStamina, targetStamina);
                OnStaminaChanged();
            }
		}
    }
}
