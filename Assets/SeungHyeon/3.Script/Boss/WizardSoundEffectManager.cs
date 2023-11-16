using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardSoundEffectManager : MonoBehaviour
{
    public void PlaySound(SoundEffectSO sfx)
    {
        SFXManager.Instance.PlayWhole(sfx);
    }
}
