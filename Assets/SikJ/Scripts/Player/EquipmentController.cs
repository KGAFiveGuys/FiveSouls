using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    private PlayerController player;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (player.IsDead && collision.gameObject.layer == 1 << 3)
        {
            SFXManager.Instance.OnPlayerEquipmentFall();
        }
    }
}
