using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        //삭제할 돌땡이

        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject, 0.5f);
        }
    }
}
