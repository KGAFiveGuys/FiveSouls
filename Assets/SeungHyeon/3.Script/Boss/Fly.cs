using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
    [SerializeField]private Rigidbody rb;

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.layer.Equals(3))
        {
            rb.AddForce(Vector3.up * 15f, ForceMode.Acceleration);
        }
    }


}
