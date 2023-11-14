using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    HealthRegenBoostPotion,
    StaminaRegenBoostPotion,
    BaseDamageBoostPotion,
    CounterDamageBoostPotion,
}

[CreateAssetMenu(fileName = "ItemSO_", menuName = "ScriptableObjects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [SerializeField] public ItemType type;
    [SerializeField] public float duration;
    [SerializeField] public string Name;
    [SerializeField] [TextArea(3, 10)] public string Spec;
}
