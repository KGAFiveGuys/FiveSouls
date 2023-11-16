using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSoundTransform : MonoBehaviour
{
    [SerializeField] private Transform Sword;
    [SerializeField] private SwordSpawner swordSpawner;
    private AudioSource audioSource;
    [SerializeField] private AudioClip ExplosionClip;

    private bool ExplosionSFX = false;

    private void Start()
    {
        TryGetComponent(out audioSource);
        audioSource.clip = ExplosionClip;
    }
    private void Update()
    {
        transform.position = Sword.position;
        if (swordSpawner.startExplosion && !ExplosionSFX)
        {
            audioSource.Play();
            ExplosionSFX = true;
        }
        else if (!swordSpawner.startExplosion)
        {
            ExplosionSFX = false;

        }
    }

}
