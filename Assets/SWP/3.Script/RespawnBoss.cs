using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnBoss : MonoBehaviour
{
    [SerializeField] private AiTest hulkAI;
    private Health playerHealth;

    private void Awake()
    {
        playerHealth = FindObjectOfType<PlayerController>().gameObject.GetComponent<Health>();
    }

    private void OnEnable()
    {
        playerHealth.OnRevive += hulkAI.BossSetting;
        playerHealth.OnRevive += ToggleBossGameobject;
    }

    private void OnDisable()
    {
        playerHealth.OnRevive -= hulkAI.BossSetting;
        playerHealth.OnRevive -= ToggleBossGameobject;
    }

    private void ToggleBossGameobject()
    {
        hulkAI.gameObject.SetActive(false);
        hulkAI.gameObject.SetActive(true);
    }
}
