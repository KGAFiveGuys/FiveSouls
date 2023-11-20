using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeFadeIn : MonoBehaviour
{
    [SerializeField] private Material material;
    private float FadeSpeed = 0.2f;
    private Color BaseColor;
    private Color BlueColor;
    private Color WhiteColor;
    private void Start()
    {
        BaseColor = material.GetColor("_EmissionColor");
        BlueColor = Color.clear;
        WhiteColor = Color.clear;
    }

    private void OnEnable()
    {
        material.SetColor("_EmissionColor", new Color(0,0,0));
        StartCoroutine(EyeFade());
    }
    
    private IEnumerator EyeFade()
    {
        while (material.color.b < 1)
        {
            if (gameObject.name == "eye_L_old")
            {
                material.SetColor("_EmissionColor", BlueColor);
                BlueColor.b += Time.deltaTime * FadeSpeed;
            }
            if (gameObject.name == "eye_R_old")
            {
                material.SetColor("_EmissionColor", WhiteColor);
                WhiteColor.r += Time.deltaTime * FadeSpeed;
                WhiteColor.g += Time.deltaTime * FadeSpeed;
                WhiteColor.b += Time.deltaTime * FadeSpeed;
            }

            yield return null;
        }
        if (material.color.b >= 1)
        {
            material.SetColor("_EmissionColor", BaseColor);
            yield break;
        }
    }
}
