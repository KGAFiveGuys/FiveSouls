using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnRock : MonoBehaviour
{
    [SerializeField] private GameObject rockPooling;
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Rock"))
        {
            var offset = new Vector3(Random.Range(-50f, 50f),0f, 0f );
            other.gameObject.SetActive(false);
            RockPooling.Rock_count++;
            other.gameObject.transform.position = rockPooling.transform.position + offset;
        }
    }
}