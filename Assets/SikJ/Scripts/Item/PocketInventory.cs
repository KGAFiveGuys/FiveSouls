using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Pocket
{
    public ItemSO itemInfo;
    public int count;

	public Pocket(ItemSO itemInfo, int count)
	{
        this.itemInfo = itemInfo;
        this.count = count;
	}
}

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Stamina))]
[RequireComponent(typeof(AttackController))]
public class PocketInventory : MonoBehaviour
{
    [SerializeField] private PocketInventoryManager pocketInventoryControlManager;
    [SerializeField] ItemSO healthRegenBoostPotion;
    [SerializeField] ItemSO staminaRegenBoostPotion;
    [SerializeField] ItemSO baseDamageBoostPotion;
    [SerializeField] ItemSO counterDamageBoostPotion;

    private List<Pocket> pocketList = new List<Pocket>();

    private int currentIndex = 0;
    public Pocket CurrentPocket => pocketList[currentIndex];

    public event Action<ItemSO> OnItemUsed;

    private Health playerHealth;
    private Stamina playerStamina;
    private AttackController playerAttackController;

    private void Awake()
    {
        TryGetComponent(out playerHealth);
        TryGetComponent(out playerStamina);
        TryGetComponent(out playerAttackController);

        Pocket pocket1 = new Pocket(healthRegenBoostPotion, 1);
        Pocket pocket2 = new Pocket(staminaRegenBoostPotion, 1);
        Pocket pocket3 = new Pocket(baseDamageBoostPotion, 1);
        Pocket pocket4 = new Pocket(counterDamageBoostPotion, 1);
        pocketList.Add(pocket1);
        pocketList.Add(pocket2);
        pocketList.Add(pocket3);
        pocketList.Add(pocket4);
    }

	public void StoreItem(ItemSO itemSO)
	{
		for (int i = 0; i < pocketList.Count; i++)
		{
            if (pocketList[i].itemInfo == itemSO)
			{
                var targetPocket = pocketList[i];
                targetPocket.count++;
                pocketList[i] = targetPocket;
                break;
            }
        }

        ChangeSelection(0);
    }

	public void ChangeSelection(int direction)
	{
        currentIndex += direction;

        // Prevent index out of range exception
        if (currentIndex > 0)
            currentIndex %= pocketList.Count;
        else if (currentIndex < 0)
            currentIndex += pocketList.Count;

        var currentPocket = pocketList[currentIndex];
        pocketInventoryControlManager.ChangePocketInfo(currentPocket.itemInfo, currentPocket.count);
    }

    public void UseCurrentItem()
	{
		if (CurrentPocket.count == 0)
            return;
		
        if(TryUse(pocketList[currentIndex]))
		{
            var temp = pocketList[currentIndex];
            temp.count--;
            pocketList[currentIndex] = temp;

            OnItemUsed?.Invoke(pocketList[currentIndex].itemInfo);
        }
        else
        {
            Debug.Log($"{CurrentPocket.itemInfo.Name}의 효과가 아직 남아있습니다");
		}

        ChangeSelection(0);
    }

	private bool TryUse(Pocket pocket)
    {
        // type에 따른 효과 부여
        switch (pocket.itemInfo.type)
        {
            case ItemType.HealthRegenBoostPotion:
				if (currentHealthRegenBoost == null)
				{
                    currentHealthRegenBoost = BoostHealthRegen(pocket.itemInfo.duration);
                    StartCoroutine(currentHealthRegenBoost);
                    return true;
                }
                break;
            case ItemType.StaminaRegenBoostPotion:
                if (currentStaminaRegenBoost == null)
                {
                    currentStaminaRegenBoost = BoostStaminaRegen(pocket.itemInfo.duration);
                    StartCoroutine(currentStaminaRegenBoost);
                    return true;
                }
                break;
            case ItemType.BaseDamageBoostPotion:
                if (currentBaseDamageBoost == null)
                {
                    currentBaseDamageBoost = BoostBaseDamage(pocket.itemInfo.duration);
                    StartCoroutine(currentBaseDamageBoost);
                    return true;
                }
                break;
            case ItemType.CounterDamageBoostPotion:
                if (currentCounterDamageBoost == null)
                {
                    currentCounterDamageBoost = BoostCounterDamage(pocket.itemInfo.duration);
                    StartCoroutine(currentCounterDamageBoost);
                    return true;
                }
                break;
        }

        return false;
    }

    private IEnumerator currentHealthRegenBoost = null;
    private IEnumerator BoostHealthRegen(float duration)
    {
        float targetHeal = 50f;
        float totalHeal = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var currentHeal = targetHeal / duration * Time.deltaTime;
            totalHeal += currentHeal;

            // 목표 회복량을 초과하는 경우 보정
			if (totalHeal > targetHeal)
                currentHeal -= totalHeal - targetHeal;

            playerHealth.GetHeal(currentHeal);
            yield return null;
        }

        currentHealthRegenBoost = null;
    }

    private IEnumerator currentStaminaRegenBoost = null;
    private IEnumerator BoostStaminaRegen(float duration)
    {
        var originDelay = playerStamina.RegenDelay;
        var originMaxRegen = playerStamina.MaxRegenPerSeconds;
        playerStamina.RegenDelay /= 2;
        playerStamina.MaxRegenPerSeconds *= 2;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerStamina.RegenDelay = originDelay;
        playerStamina.MaxRegenPerSeconds = originMaxRegen;

        currentStaminaRegenBoost = null;
    }

    private IEnumerator currentBaseDamageBoost = null;
    private IEnumerator BoostBaseDamage(float duration)
    {
        var originWeakAttackDamage = playerAttackController.WeakAttackBaseDamage;
        var originStrongAttackDamage = playerAttackController.StrongAttackBaseDamage;
        playerAttackController.WeakAttackBaseDamage *= 2;
        playerAttackController.StrongAttackBaseDamage *= 2;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerAttackController.WeakAttackBaseDamage = originWeakAttackDamage;
        playerAttackController.StrongAttackBaseDamage = originStrongAttackDamage;

        currentBaseDamageBoost = null;
    }

    private IEnumerator currentCounterDamageBoost = null;
    private IEnumerator BoostCounterDamage(float duration)
    {
        var originCounterAttackDamage = playerAttackController.CounterAttackBaseDamage;
        playerAttackController.CounterAttackBaseDamage *= 2;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerAttackController.CounterAttackBaseDamage = originCounterAttackDamage;

        currentCounterDamageBoost = null;
    }
}
