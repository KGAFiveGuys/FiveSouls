using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator_btn : MonoBehaviour
{
    [SerializeField] private MeshRenderer Aura1;
    [SerializeField] private MeshRenderer Aura2;
    [SerializeField] private MeshRenderer Aura3;
    [SerializeField] private Light topLight;
    [SerializeField] private Light bottomLight;

    [SerializeField] private Material AuraMat1_Inactive;
    [SerializeField] private Material AuraMat2_Inactive;
    [SerializeField] private Material AuraMat3_Inactive;

    [SerializeField] private Material AuraMat1_Active;
    [SerializeField] private Material AuraMat2_Active;
    [SerializeField] private Material AuraMat3_Active;

    public static bool eleva_btn = false;

    private void Start()
    {
        ToggleButtonLight();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            eleva_btn = !eleva_btn;
            ToggleButtonLight();
        }
    }

    private void ToggleButtonLight()
    {
        Aura1.material = eleva_btn ? AuraMat1_Active : AuraMat1_Inactive;
        Aura2.material = eleva_btn ? AuraMat2_Active : AuraMat2_Inactive;
        Aura3.material = eleva_btn ? AuraMat3_Active : AuraMat3_Inactive;
        var topColor = topLight.color;
        var bottomColor = bottomLight.color;
        if (eleva_btn)
        {
            topColor.r = 1;
            bottomColor.r = 1;
        }
        else
        {
            topColor.r = 0;
            bottomColor.r = 0;
        }
        topLight.color = topColor;
        bottomLight.color = bottomColor;
    }
}
