using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 1 << 3)
        {
            SFXManager.Instance.OnPlayerEquipmentFall();
        }
    }
}
