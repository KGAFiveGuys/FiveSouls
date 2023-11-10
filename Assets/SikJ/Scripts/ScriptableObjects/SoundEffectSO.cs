using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffectSO_", menuName = "ScriptableObjects/SoundEffectSO")]
public class SoundEffectSO : ScriptableObject
{
    [SerializeField] public AudioClip clip;
    [SerializeField] public float delay = 0f;
    [SerializeField] public AnimationCurve volumeOverTime;
}
