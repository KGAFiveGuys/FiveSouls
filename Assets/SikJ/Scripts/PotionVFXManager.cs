using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionVFXManager : MonoBehaviour
{
	[SerializeField] private ParticleSystem healthRegenPotionVFX;
	[SerializeField] private ParticleSystem staminaRegenPotionVFX;
	[SerializeField] private ParticleSystem basicDamagePotionVFX;
	[SerializeField] private ParticleSystem counterDamagePotionVFX;

	private PocketInventory _pocketInventory;

	private void Awake()
	{
		var playerObj = GameObject.FindGameObjectWithTag("Player");
		_pocketInventory = playerObj.GetComponent<PocketInventory>();
	}

	private void OnEnable()
	{
		_pocketInventory.OnItemUsed += PlayPotionParticle;
	}

	private void OnDisable()
	{
		_pocketInventory.OnItemUsed -= PlayPotionParticle;
	}

	private void PlayPotionParticle(ItemSO item)
	{
		switch (item.type)
		{
			case ItemType.HealthRegenBoostPotion:
				healthRegenPotionVFX.Play();
				break;
			case ItemType.StaminaRegenBoostPotion:
				staminaRegenPotionVFX.Play();
				break;
			case ItemType.BaseDamageBoostPotion:
				basicDamagePotionVFX.Play();
				break;
			case ItemType.CounterDamageBoostPotion:
				counterDamagePotionVFX.Play();
				break;
		}
	}
}
