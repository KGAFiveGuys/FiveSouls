using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantVibrationManager : MonoBehaviour
{
    // Animation Event
    public void Vibrate(VibrationSO vibration)
    {
        GamePadVibrationManager.Instance.Vibrate(vibration);
    }
}
