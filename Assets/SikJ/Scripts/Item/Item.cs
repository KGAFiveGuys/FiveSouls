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

public abstract class Item : MonoBehaviour
{
    public ItemType type;
    public string Name;
    [TextArea(3, 6)] public string Spec;
    public string Effect;
}
