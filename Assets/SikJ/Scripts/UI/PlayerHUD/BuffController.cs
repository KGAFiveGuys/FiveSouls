using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffController : MonoBehaviour
{
    [SerializeField] private GameObject healthRegenBoostPrefab;
    [SerializeField] private GameObject staminaRegenBoostPrefab;
    [SerializeField] private GameObject baseAttackDamageBoostPrefab;
    [SerializeField] private GameObject counterAttackDamageBoostPrefab;
    [SerializeField] private GameObject armorBoostPrefab;

    private PocketInventory pocketInventory;

    private void Awake()
    {
        pocketInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PocketInventory>();
    }

    private void OnEnable()
    {
        pocketInventory.OnItemUsed += CreateBuff;
    }

    private void OnDisable()
    {
        pocketInventory.OnItemUsed -= CreateBuff;
    }

    private void CreateBuff(ItemSO itemInfo)
    {
        GameObject newBuff = null;
        switch (itemInfo.type)
        {
            case ItemType.HealthRegenBoostPotion:
                newBuff = healthRegenBoostPrefab;
                break;
            case ItemType.StaminaRegenBoostPotion:
                newBuff = staminaRegenBoostPrefab;
                break;
            case ItemType.BaseDamageBoostPotion:
                newBuff = baseAttackDamageBoostPrefab;
                break;
            case ItemType.CounterDamageBoostPotion:
                newBuff = counterAttackDamageBoostPrefab;
                break;
        }

        var buff = Instantiate(newBuff);
        buff.transform.parent = transform;
        buff.transform.localScale = Vector3.one;
        StartCoroutine(DisappearGradually(buff, itemInfo.duration));
    }

    private IEnumerator DisappearGradually(GameObject buff, float duration)
    {
        Color originImageColor = buff.transform.GetChild(1).GetChild(0).GetComponent<Image>().color;
        Color targetImageColor = new Color(originImageColor.r, originImageColor.g, originImageColor.b, 0);

        Color originBackgroundColor = buff.transform.GetChild(0).GetComponent<Image>().color;
        Color targetBackgroundColor = new Color(originBackgroundColor.r, originBackgroundColor.g, originBackgroundColor.b, 0);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp Buff Image
            buff.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.Lerp(originImageColor, targetImageColor, elapsedTime / duration);
            // Lerp Buff Background
            buff.transform.GetChild(0).GetComponent<Image>().color = Color.Lerp(originBackgroundColor, targetBackgroundColor, elapsedTime / duration);
            yield return null;
        }
        buff.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = targetImageColor;

        Destroy(buff);
    }
}
