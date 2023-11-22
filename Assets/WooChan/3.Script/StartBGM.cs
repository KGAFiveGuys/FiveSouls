using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBGM : MonoBehaviour
{
    private void Start()
    {
        SFXManager.Instance.OnBossFight4_Started();
    }
}
