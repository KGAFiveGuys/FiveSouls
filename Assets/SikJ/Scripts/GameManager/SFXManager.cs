using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager _instance = null;
    public static SFXManager Instance => _instance;

    [SerializeField] private GameObject AudioSourcePrefab;
    [SerializeField] private int BGM_AudioSourceCount = 3;
    [SerializeField] private int SFX_AudioSourceCount = 10;
    private List<AudioSource> BGM_AudioSources = new List<AudioSource>();
    private List<AudioSource> SFX_AudioSources = new List<AudioSource>();
    [SerializeField] private float TimeToUnmuteAfterPlay = .5f;

    [Header("Back Ground Music")]
    #region BGM
    [SerializeField] private SoundEffectSO BGM_bossFight1;
    [SerializeField] private SoundEffectSO BGM_bossFight2;
    [SerializeField] private SoundEffectSO BGM_bossFight3;
    [SerializeField] private SoundEffectSO BGM_bossFight4;
    #endregion

    #region SFX
    [Header("Common")]
    [SerializeField] private SoundEffectSO SFX_timeSlowDown;

    [Header("Player Combat")]
    // Block
    [SerializeField] private SoundEffectSO SFX_playerBlockCast;
    [SerializeField] private SoundEffectSO SFX_playerBlockSucceed1;
    [SerializeField] private SoundEffectSO SFX_playerBlockSucceed2;
    [SerializeField] private SoundEffectSO SFX_playerBlockFailed;
    // Attack Cast
    [SerializeField] private SoundEffectSO SFX_playerWeakAttackCast;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackCast;
    [SerializeField] private SoundEffectSO SFX_playerCounterAttackCast;
    // Attack Hit
    [SerializeField] private SoundEffectSO SFX_playerWeakAttackHit;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackHit1;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackHit2;
    [SerializeField] private SoundEffectSO SFX_playerCounterAttackHit;
    // Dead
    [SerializeField] private SoundEffectSO SFX_playerDead1;
    [SerializeField] private SoundEffectSO SFX_playerDead2;
    [SerializeField] private SoundEffectSO SFX_playerEquipmentFall;

    [Header("Player Locomotion")]
    [SerializeField] private SoundEffectSO SFX_playerRoll;
    [SerializeField] private SoundEffectSO SFX_playerJump;

    [Header("Prop Breaks")]
    [SerializeField] private SoundEffectSO SFX_woodenPropBreaked;
    [SerializeField] private SoundEffectSO SFX_barrelBreaked;
    #endregion

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
        for (int i = 0; i < BGM_AudioSourceCount; i++)
        {
            GameObject gameObject = Instantiate(AudioSourcePrefab, transform);
            BGM_AudioSources.Add(gameObject.GetComponent<AudioSource>());
        }

        for (int i = 0; i < SFX_AudioSourceCount; i++)
        {
            GameObject gameObject = Instantiate(AudioSourcePrefab, transform);
            SFX_AudioSources.Add(gameObject.GetComponent<AudioSource>());
        }
    }

    #region BGM
    public void OnBossFight1_Started() => PlayLoop(BGM_bossFight1);
    public void OnBossFight2_Started() => PlayLoop(BGM_bossFight2);
    public void OnBossFight3_Started() => PlayLoop(BGM_bossFight3);
    public void OnBossFight4_Started() => PlayLoop(BGM_bossFight4);
    #endregion

    #region SFX
    public void OnTimeSlowDown(float duration) => PlayPartial(SFX_timeSlowDown, duration);
    public void OnPlayerBlockCast()
    {
        Debug.Log("SFX_OnPlayerBlockCast");
    }
    public void OnPlayerBlockSucceed(float duration)
    {
        PlayWhole(SFX_playerBlockSucceed1);
        PlayPartial(SFX_playerBlockSucceed2, duration);
    }
    public void OnPlayerBlockFailed()
	{
        Debug.Log("SFX_OnPlayerBlockFailed");
	}
    public void OnPlayerWeakAttackCast() => PlayWhole(SFX_playerWeakAttackCast);
    public void OnPlayerStrongAttackCast() => PlayWhole(SFX_playerStrongAttackCast);
    public void OnPlayerCounterAttackCast() => PlayInMiddle(SFX_playerCounterAttackCast, SFX_playerCounterAttackCast.startInMiddle);
    public void OnPlayerWeakAttackHit() => PlayWhole(SFX_playerWeakAttackHit);
    public void OnPlayerStrongAttackHit()
    {
        PlayWhole(SFX_playerStrongAttackHit1);
        PlayWhole(SFX_playerStrongAttackHit2);
    }
    public void OnPlayerCounterAttackHit() => PlayWhole(SFX_playerCounterAttackHit);
    public void OnPlayerRoll() => PlayWhole(SFX_playerRoll);
    public void OnPlayerJump() => PlayWhole(SFX_playerJump);
    public void OnPlayerDead()
    {
        PlayWhole(SFX_playerDead1);
        PlayWhole(SFX_playerDead1);
    }
    public void OnPlayerEquipmentFall() => PlayWhole(SFX_playerEquipmentFall);

    public void OnWoodenCrateBreaked() => PlayWhole(SFX_woodenPropBreaked);
    #endregion

    private void PlayLoop(SoundEffectSO bgm)
    {
        if (bgm == null)
            return;

        foreach (var audioSource in BGM_AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, bgm, bgm.clip.length, true));
                break;
            }
        }
    }

    private void PlayWhole(SoundEffectSO sfx)
    {
        if (sfx == null)
            return;

        foreach (var audioSource in SFX_AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, sfx.clip.length));
                break;
            }
        }
    }

    private void PlayInMiddle(SoundEffectSO sfx, float time)
    {
        if (sfx == null)
            return;

        foreach (var audioSource in SFX_AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, sfx.clip.length, false, time));
                break;
            }
        }
    }

    private void PlayPartial(SoundEffectSO sfx, float duration)
    {
        if (sfx == null)
            return;

        foreach (var audioSource in SFX_AudioSources)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, duration));
                break;
            }
        }
    }

    private IEnumerator StartPlay(AudioSource source, SoundEffectSO sfx, float duration, bool isLoop = false, float time = 0f)
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
        source.loop = isLoop ? true : false;
        source.time = time;
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
