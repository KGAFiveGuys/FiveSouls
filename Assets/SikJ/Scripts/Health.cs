using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [field:Header("Value")]
    // Total Max Health = MaxHP * DigitScale
    [field:SerializeField] public float MaxHP { get; private set; } = 1000f;
    // Total Current Health = CurrentHP * DigitScale
    [field: SerializeField] public float CurrentHP { get; private set; }

    [Header("UI")]
    [SerializeField] private Slider HealthSlider;

    private void Start()
    {
        CurrentHP = MaxHP;
    }

    public void GetDamage(float damage)
    {
        if (CurrentHP <= 0)
        {
            Debug.Log($"{gameObject.name} »ç¸ÁÇÑ »óÅÂ!");
            return;
        }

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        Debug.Log($"{gameObject.name} {damage} ÇÇ°Ý! {CurrentHP}/{MaxHP}");
        if (CurrentHP == 0)
        {
            Debug.Log($"{gameObject.name} »ç¸Á!");
        }
    }
}
