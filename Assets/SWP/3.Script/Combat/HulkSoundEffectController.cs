using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HulkSoundEffectController : MonoBehaviour
{
    public void PlaySound(SoundEffectSO sfx)
    {
        SFXManager.Instance.PlayWhole(sfx);
    }
}
