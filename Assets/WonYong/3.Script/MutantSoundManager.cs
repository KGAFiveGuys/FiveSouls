using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantSoundManager : MonoBehaviour
{
    // Animation Event
    public void PlaySound(SoundEffectSO sfx)
    {
        SFXManager.Instance.PlayWhole(sfx);
    }
}
