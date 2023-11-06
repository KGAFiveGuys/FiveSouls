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

    [Header("Common")]
    [SerializeField] private AudioClip timeSlowDown;

    [Header("Player Combat")]
    // Block
    [SerializeField] private AudioClip playerOnBlock1;
    [SerializeField] private AudioClip playerOnBlock2;
    // Attack Cast
    [SerializeField] private AudioClip playerOnWeakAttackCast;
    [SerializeField] private AudioClip playerOnStrongAttackCast;
    // Attack Hit
    [SerializeField] private AudioClip playerOnWeakAttackHit;
    [SerializeField] private AudioClip playerOnStrongAttackHit1;
    [SerializeField] private AudioClip playerOnStrongAttackHit2;
    // Dead
    [SerializeField] private AudioClip playerOnDead1;
    [SerializeField] private AudioClip playerOnDead2;
    [SerializeField] private AudioClip playerOnEquipmentFall;

    [Header("Player Locomotion")]
    [SerializeField] private AudioClip playerOnRoll;
    [SerializeField] private AudioClip playerOnJump;

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

    public void OnTimeSlowDown(float duration) => Play(timeSlowDown, duration, .75f);
    public void OnPlayerBlock(float duration)
    {
        PlayOneShot(playerOnBlock1);
        Play(playerOnBlock2, duration, 1.1f);
    }
    public void OnPlayerWeakAttackCast() => PlayOneShot(playerOnWeakAttackCast);
    public void OnPlayerStrongAttackCast() => PlayOneShot(playerOnWeakAttackCast);
    public void OnPlayerWeakAttackHit() => PlayOneShot(playerOnWeakAttackHit);
    public void OnPlayerStrongAttackHit()
    {
        PlayOneShot(playerOnStrongAttackHit1);
        PlayOneShot(playerOnStrongAttackHit2);
    }
    public void OnPlayerRoll() => PlayOneShot(playerOnRoll);
    public void OnPlayerJump() => PlayOneShot(playerOnJump);
    public void OnPlayerDead()
    {
        PlayOneShot(playerOnDead1);
        PlayOneShot(playerOnDead2);
    }
    public void OnPlayerEquipmentFall() => PlayOneShot(playerOnEquipmentFall);

    /// <summary>
    /// Find playable audioSource, then PlayOneShot.
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    private void PlayOneShot(AudioClip clip)
    {
        foreach (var audioSource in AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(clip);
                break;
            }
        }
    }

    /// <summary>
    /// Find playable audioSource, then play until time to mute.
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="duration">Time to keep play</param>
    /// <param name="rateToMute">Rate in duration to mute and stop audio clip</param>
    private void Play(AudioClip clip, float duration, float rateToMute)
    {
        foreach (var audioSource in AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clip;
                StartCoroutine(Play(audioSource, duration, rateToMute));
                break;
            }
        }
    }

    private IEnumerator Play(AudioSource audioSource, float duration, float rateToMute)
    {
        audioSource.Play();

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Turn down volume to mute.
            if (elapsedTime / duration <= rateToMute)
                audioSource.volume = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            else if (audioSource.volume > 0)
                audioSource.volume = 0;

            yield return null;
        }
        
        StartCoroutine(StopNRestoreVolume(audioSource, .5f));
    }

    private IEnumerator StopNRestoreVolume(AudioSource audioSource, float restoreTime)
    {
        audioSource.Stop();
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
