using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderBolt : MonoBehaviour
{
    [SerializeField] private float AttackDelay = 0.5f;
    [SerializeField] private GameObject LightningBolt;
    private void OnEnable()
    {
        StartCoroutine(Thunder());
    }
    private IEnumerator Thunder()
    {
        yield return new WaitForSeconds(AttackDelay);
        LightningBolt.SetActive(true);
    }        
}
