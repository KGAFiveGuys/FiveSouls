using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockStart_Sound : MonoBehaviour
{
    [SerializeField] private SoundEffectSO sound;

    private void OnEnable()
    {
        SFXManager.Instance.PlayWhole(sound);
    }
}
