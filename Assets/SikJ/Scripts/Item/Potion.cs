using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : Item, IConsumable
{
    [SerializeField] private float effectDuration = 30f;

    private Health playerHealth;
    private Stamina playerStamina;
    private AttackController playerAttackController;

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<Health>();
        playerStamina = player.GetComponent<Stamina>();
        playerAttackController = player.GetComponent<AttackController>();
    }

    public void Consume()
    {
        // type에 따른 효과 부여
        switch (type)
        {
            case ItemType.HealthRegenBoostPotion:
                break;
            case ItemType.StaminaRegenBoostPotion:
                break;
            case ItemType.BaseDamageBoostPotion:
                break;
            case ItemType.CounterDamageBoostPotion:
                break;
            default:
                break;
        }
    }

    private IEnumerator currentHealthRegenBoost = null;
    private IEnumerator BoostHealthRegen()
    {
        float totalHeal = 50f;

        float elapsedTime = 0f;
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;

            playerHealth.GetHeal(totalHeal/effectDuration * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator currentStaminaRegenBoost = null;
    private IEnumerator BoostStaminaRegen()
    {


        float elapsedTime = 0f;
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator currentBaseDamageBoost = null;
    private IEnumerator BoostBaseDamage()
    {
        float elapsedTime = 0f;
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator currentCounterDamageBoost = null;
    private IEnumerator BoostCounterDamage()
    {
        float elapsedTime = 0f;
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }
}
