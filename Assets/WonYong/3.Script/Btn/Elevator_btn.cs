using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator_btn : MonoBehaviour
{
    public Material objectMaterial;

    private void Awake()
    {
        objectMaterial = TryGetComponent(out Renderer renderer) ? renderer.material : null;
    }

    public static bool eleva_btn = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            eleva_btn = !eleva_btn;
            if (eleva_btn)
            {
                objectMaterial.color = Color.green;
            }
        } 
    }

}
