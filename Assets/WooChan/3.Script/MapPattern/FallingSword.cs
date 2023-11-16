using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSword : MonoBehaviour
{
    [SerializeField] private SwordSpawner swordSpawner;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip FallingSound;
    [SerializeField] private AudioClip HitGroundSound;
    [SerializeField] private AudioClip FireSound;

    [SerializeField] private UnityChanSoundManager SwordSFX;
    [SerializeField] private SoundEffectSO SpawnSound;
    [SerializeField] private SoundEffectSO HitSound;

    [SerializeField] Transform Target;
    [SerializeField] Collider AttackCollider;

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
        AttackCollider.enabled = true;
        isStop = false;
        float RanX = Random.Range(500f, -500f);
        float RanY = Random.Range(500f, -500f);
        transform.position = new Vector3(RanX, 1000f, RanY);
        transform.LookAt(Target.position);
        SwordSFX.PlaySound(SpawnSound);
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
            AttackCollider.enabled = false;
            audioSource.PlayOneShot(HitGroundSound);
        }
    }

}
