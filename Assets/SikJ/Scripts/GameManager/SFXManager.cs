using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager _instance = null;
    public static SFXManager Instance => _instance;

    [SerializeField] private GameObject AudioSourcePrefab;
    [SerializeField] private int AudioSourceCount = 10;
    private List<AudioSource> AudioSources = new List<AudioSource>();
    [SerializeField] private float TimeToUnmuteAfterPlay = .5f;

    [Header("Common")]
    [SerializeField] private AudioClip timeSlowDown;
    [SerializeField] private SoundEffectSO SFX_timeSlowDown;

    [Header("Player Combat")]
    // Block
    [SerializeField] private SoundEffectSO SFX_playerBlock1;
    [SerializeField] private SoundEffectSO SFX_playerBlock2;
    // Attack Cast
    [SerializeField] private SoundEffectSO SFX_playerWeakAttackCast;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackCast;
    // Attack Hit
    [SerializeField] private SoundEffectSO SFX_playerWeakAttackHit;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackHit1;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackHit2;
    // Dead
    [SerializeField] private SoundEffectSO SFX_playerDead1;
    [SerializeField] private SoundEffectSO SFX_playerDead2;
    [SerializeField] private SoundEffectSO SFX_playerEquipmentFall;

    [Header("Player Locomotion")]
    [SerializeField] private SoundEffectSO SFX_playerRoll;
    [SerializeField] private SoundEffectSO SFX_playerJump;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else if (_instance != null && _instance != this)
        {
            Destroy(this);
        }    
    }

    private void Start()
    {
        CreateAudioSources();
    }

    private void CreateAudioSources()
    {
        for (int i = 0; i < AudioSourceCount; i++)
        {
            GameObject gameObject = Instantiate(AudioSourcePrefab, transform);
            AudioSources.Add(gameObject.GetComponent<AudioSource>());
        }
    }

    public void OnTimeSlowDown(float duration) => SFXPlay(SFX_timeSlowDown, duration);
    public void OnPlayerBlock(float duration)
    {
        SFXPlayOneShot(SFX_playerBlock1);
        SFXPlay(SFX_playerBlock2, duration);
    }
    //public void OnPlayerWeakAttackCast() => PlayOneShot(playerOnWeakAttackCast, 1f);
    public void OnPlayerWeakAttackCast() => SFXPlayOneShot(SFX_playerWeakAttackCast);
    public void OnPlayerStrongAttackCast() => SFXPlayOneShot(SFX_playerStrongAttackCast);
    public void OnPlayerWeakAttackHit() => SFXPlayOneShot(SFX_playerWeakAttackHit);
    public void OnPlayerStrongAttackHit()
    {
        SFXPlayOneShot(SFX_playerStrongAttackHit1);
        SFXPlayOneShot(SFX_playerStrongAttackHit2);
    }
    public void OnPlayerJump() => SFXPlayOneShot(SFX_playerJump);
    public void OnPlayerDead()
    {
        SFXPlayOneShot(SFX_playerDead1);
        SFXPlayOneShot(SFX_playerDead1);
    }
    public void OnPlayerEquipmentFall() => SFXPlayOneShot(SFX_playerEquipmentFall);

    private void SFXPlayOneShot(SoundEffectSO sfx)
    {
        foreach (var audioSource in AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = sfx.startVolume;
                audioSource.PlayOneShot(sfx.clip);
                StartCoroutine(RestoreVolume(audioSource, TimeToUnmuteAfterPlay));
                break;
            }
        }
    }

    private void SFXPlay(SoundEffectSO sfx, float duration)
    {
        foreach (var audioSource in AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = sfx.startVolume;
                audioSource.clip = sfx.clip;
                StartCoroutine(Play(audioSource, sfx.startVolume, duration, sfx.playRateToMute));
                break;
            }
        }
    }

    private IEnumerator Play(AudioSource audioSource, float startVolume, float duration, float rateToMute)
    {
        audioSource.Play();

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Turn down volume to mute.
            if (elapsedTime / duration <= rateToMute)
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            else if (audioSource.volume > 0)
                audioSource.volume = 0;

            yield return null;
        }

        audioSource.Stop();
        StartCoroutine(RestoreVolume(audioSource, TimeToUnmuteAfterPlay));
    }

    private IEnumerator RestoreVolume(AudioSource audioSource, float restoreTime)
    {
        var volumeStart = audioSource.volume;

        float elapsedTime = 0f;
        while (elapsedTime < restoreTime)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volumeStart, 1, elapsedTime / restoreTime);
            yield return null;
        }
        audioSource.volume = 1f;
    }
}
