using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HulkVibrationController : MonoBehaviour
{
    public void Vibrate(VibrationSO vibration)
    {
        GamePadVibrationManager.Instance.Vibrate(vibration);
    }
}
