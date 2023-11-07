using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VibrationSO_", menuName = "ScriptableObjects/VibrationSO")]
public class VibrationSO : ScriptableObject
{
    [field: Tooltip("Higher vale for more imporant. (Default = 10)")]
    [field: SerializeField] [field: Range(0, 50)] public int Priority { get; set; } = 10;
    [field: SerializeField] [field: Range(0, 10)] public float Duration { get; set; }
    [field: SerializeField] public AnimationCurve LowMoterIntensity { get; set; }
    [field: SerializeField] public AnimationCurve HighMoterIntensity { get; set; }
}
