using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnRock : MonoBehaviour
{
    private GameObject rockPooling;
    private void Awake()
    {
        rockPooling = GameObject.Find("RockPoolposition");
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Rock"))
        {
            var offset = new Vector3(0f, 0f, Random.Range(-34f, 36f));
            other.gameObject.SetActive(false);
            RockPooling.Rock_count++;
            other.gameObject.transform.position = rockPooling.transform.position + offset;
        }
    }
}
