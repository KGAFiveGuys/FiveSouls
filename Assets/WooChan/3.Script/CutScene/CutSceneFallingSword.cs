using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneFallingSword : MonoBehaviour
{
    [SerializeField] private SwordSpawner swordSpawner;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip FallingSound;
    [SerializeField] private AudioClip HitGroundSound;
    [SerializeField] private AudioClip FireSound;

    [SerializeField] private UnityChanSoundManager SwordSFX;
    [SerializeField] private SoundEffectSO HitSound;

    [SerializeField] private Transform Target;
    private bool isStop = false;

    private bool fireSFX = false;

    void Update()
    {
        if (!isStop)
        {
            transform.position += transform.TransformDirection(Vector3.forward) * 400 * Time.deltaTime;
        }
        if (swordSpawner.startFire & !fireSFX)
        {
            audioSource.clip = FireSound;
            audioSource.Play();
            fireSFX = true;
        }
    }

    private void OnEnable()
    {
        TryGetComponent(out audioSource);
        audioSource.clip = FallingSound;
        isStop = false;
        transform.position = new Vector3(transform.position.x, 1000f, transform.position.y);
        transform.LookAt(Target);
        audioSource.Play();

        fireSFX = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        int Ground = LayerMask.NameToLayer("Ground");
        if (other.gameObject.layer == Ground)
        {
            audioSource.Stop();
            isStop = true;
            audioSource.PlayOneShot(HitGroundSound);
        }
    }

}
