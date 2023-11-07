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

    public void OnTimeSlowDown(float duration) => SFXPlayPartial(SFX_timeSlowDown, duration);
    public void OnPlayerBlock(float duration)
    {
        SFXPlayWhole(SFX_playerBlock1);
        SFXPlayPartial(SFX_playerBlock2, duration);
    }
    //public void OnPlayerWeakAttackCast() => PlayOneShot(playerOnWeakAttackCast, 1f);
    public void OnPlayerWeakAttackCast() => SFXPlayWhole(SFX_playerWeakAttackCast);
    public void OnPlayerStrongAttackCast() => SFXPlayWhole(SFX_playerStrongAttackCast);
    public void OnPlayerWeakAttackHit() => SFXPlayWhole(SFX_playerWeakAttackHit);
    public void OnPlayerStrongAttackHit()
    {
        SFXPlayWhole(SFX_playerStrongAttackHit1);
        SFXPlayWhole(SFX_playerStrongAttackHit2);
    }
    public void OnPlayerJump() => SFXPlayWhole(SFX_playerJump);
    public void OnPlayerDead()
    {
        SFXPlayWhole(SFX_playerDead1);
        SFXPlayWhole(SFX_playerDead1);
    }
    public void OnPlayerEquipmentFall() => SFXPlayWhole(SFX_playerEquipmentFall);

    private void SFXPlayWhole(SoundEffectSO sfx)
    {
        foreach (var audioSource in AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, sfx.clip.length));
                break;
            }
        }
    }

    private void SFXPlayPartial(SoundEffectSO sfx, float duration)
    {
        foreach (var audioSource in AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, duration));
                break;
            }
        }
    }

    private IEnumerator StartPlay(AudioSource source, SoundEffectSO sfx, float duration)
    {
        // Delay
        float elapsedTime = 0f;
        while (elapsedTime < sfx.delay)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Start Play
        source.clip = sfx.clip;
        source.volume = sfx.volumeOverTime.Evaluate(0);
        source.Play();

        // Lerp Volumne
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = sfx.volumeOverTime.Evaluate(elapsedTime / duration);
            yield return null;
        }
        source.volume = sfx.volumeOverTime.Evaluate(duration);

        // Stop Play
        source.Stop();
        StartCoroutine(RestoreVolume(source));
    }

    private IEnumerator RestoreVolume(AudioSource audioSource)
    {
        // 서서히 복원
        var volumeStart = audioSource.volume;
        float elapsedTime = 0f;
        while (elapsedTime < TimeToUnmuteAfterPlay)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volumeStart, 1, elapsedTime / TimeToUnmuteAfterPlay);
            yield return null;
        }
        audioSource.volume = 1f;
    }
}
