using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneChanger : MonoBehaviour
{
    [SerializeField] private FadeIn fadein;

    private void OnEnable()
    {
        fadein.FadeOut();
    }
}
