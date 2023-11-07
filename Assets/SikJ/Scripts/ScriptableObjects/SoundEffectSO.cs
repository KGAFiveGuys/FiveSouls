using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffectSO_", menuName = "ScriptableObjects/SoundEffectSO")]
public class SoundEffectSO : ScriptableObject
{
    [SerializeField] public bool isFullPlay = true;
    [SerializeField] public AudioClip clip;
    [SerializeField] [Range(0f, 1f)] public float startVolume = 1f;
    [SerializeField] [Range(0f, 1f)] public float playRateToMute = 1f;
}
