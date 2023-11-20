using System;
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
    private readonly List<AudioSource> BGM_AudioSources = new List<AudioSource>();
    private readonly List<AudioSource> SFX_AudioSources = new List<AudioSource>();
    [SerializeField] private float TimeToUnmuteAfterPlay = .5f;

    #region BGM List
    [Header("Back Ground Music")]
    [SerializeField] private SoundEffectSO BGM_bossFight1;
    [SerializeField] private SoundEffectSO BGM_bossFight2;
    [SerializeField] private SoundEffectSO BGM_bossFight3;
    [SerializeField] private SoundEffectSO BGM_bossFight4;
    #endregion
    #region SFX List
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
    // Attack Success
    [SerializeField] private SoundEffectSO SFX_playerWeakAttackSuccess;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackSuccess1;
    [SerializeField] private SoundEffectSO SFX_playerStrongAttackSuccess2;
    [SerializeField] private SoundEffectSO SFX_playerCounterAttackSuccess;
    // Hit
    [SerializeField] private SoundEffectSO SFX_playerWeakHit;
    [SerializeField] private SoundEffectSO SFX_playerStrongHit;
    // Dead
    [SerializeField] private SoundEffectSO SFX_playerDead1;
    [SerializeField] private SoundEffectSO SFX_playerDead2;
    [SerializeField] private SoundEffectSO SFX_playerEquipmentFall;
    // Drink Potion
    [SerializeField] private SoundEffectSO SFX_playerDrinkPotionCast;
    [SerializeField] private SoundEffectSO SFX_playerDrinkPotionSuccess;

    [Header("Player Locomotion")]
    [SerializeField] private SoundEffectSO SFX_playerRoll;
    [SerializeField] private SoundEffectSO SFX_playerJump;

    [Header("Prop Breaks")]
    [SerializeField] private SoundEffectSO SFX_woodenPropBreaked;
    [SerializeField] private SoundEffectSO SFX_barrelBreaked;
    #endregion

    private PlayerController _playerController;
    private AttackController _playerAttackController;
    private BlockController _playerBlockController;
    private Health _playerHealth;
    private PocketInventory _pocketInventory;

    // Player Locomotion
    private event Action PlayerRollSFX = null;
    private event Action PlayerJumpSFX = null;
    // Player Combat
    private event Action PlayerBlockCastSFX = null;
    private event Action PlayerBlockFailedSFX = null;
    public void OnPlayerWeakHit()
    {
        PlayWhole(SFX_playerWeakHit);
    }
    public void OnPlayerStrongHit()
    {
        PlayWhole(SFX_playerStrongHit);
    }
    public void OnPlayerBlockSucceed(float duration)
    {
        PlayWhole(SFX_playerBlockSucceed1);
        PlayPartial(SFX_playerBlockSucceed2, duration);
    }
    public void OnTimeSlowDown(float duration) => PlayPartial(SFX_timeSlowDown, duration);
    private event Action PlayerWeakAttackCastSFX = null;
    private event Action PlayerWeakAttackHitSFX = null;
    private event Action PlayerStrongAttackCastSFX = null;
    private event Action PlayerStrongAttackHitSFX = null;
    private event Action PlayerCounterAttackCastSFX = null;
    private event Action PlayerCounterAttackHitSFX = null;
    private event Action PlayerDeadSFX = null;
    public void OnPlayerEquipmentFall() => PlayWhole(SFX_playerEquipmentFall);
    public void OnWoodenCrateBreaked() => PlayWhole(SFX_woodenPropBreaked);
    public void OnPlayerDrinkPotionCast() => PlayWhole(SFX_playerDrinkPotionCast);
    public void OnPlayerDrinkPotionSuccess() => PlayWhole(SFX_playerDrinkPotionSuccess);

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

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        _playerController = playerObj.GetComponent<PlayerController>();
        _playerAttackController = playerObj.GetComponent<AttackController>();
        _playerBlockController = playerObj.GetComponent<BlockController>();
        _playerHealth = playerObj.GetComponent<Health>();
        _pocketInventory = playerObj.GetComponent<PocketInventory>();
    }

    #region BGM
    public void OnBossFight1_Started() => PlayLoop(BGM_bossFight1);
    public void OnBossFight2_Started() => PlayLoop(BGM_bossFight2);
    public void OnBossFight3_Started() => PlayLoop(BGM_bossFight3);
    public void OnBossFight4_Started() => PlayLoop(BGM_bossFight4);
    #endregion

    private void OnEnable()
    {
        PlayerRollSFX = () => { PlayWhole(SFX_playerRoll); };
        PlayerJumpSFX = () => { PlayWhole(SFX_playerJump); };
        PlayerBlockCastSFX = () => { PlayWhole(SFX_playerBlockCast); };
        PlayerBlockFailedSFX = () => { PlayWhole(SFX_playerBlockFailed); };
        PlayerWeakAttackCastSFX = () => { PlayWhole(SFX_playerWeakAttackCast); };
        PlayerWeakAttackHitSFX = () => { PlayWhole(SFX_playerWeakAttackSuccess); };
        PlayerStrongAttackCastSFX = () => { PlayWhole(SFX_playerStrongAttackCast); };
        PlayerStrongAttackHitSFX = () =>
        {
            PlayWhole(SFX_playerStrongAttackSuccess1);
            PlayWhole(SFX_playerStrongAttackSuccess2);
        };
        PlayerCounterAttackCastSFX = () => { PlayWhole(SFX_playerCounterAttackCast); };
        PlayerCounterAttackHitSFX = () => { PlayWhole(SFX_playerCounterAttackSuccess); };
        PlayerDeadSFX = () =>
        {
            PlayWhole(SFX_playerDead1);
            PlayWhole(SFX_playerDead1);
        };

        SubscribePlayerEvents();
    }
    private void SubscribePlayerEvents()
    {
        _playerController.OnRoll += PlayerRollSFX;
        _playerController.OnJump += PlayerJumpSFX;
        _playerBlockController.OnBlockCast += PlayerBlockCastSFX;
        _playerBlockController.OnBlockFailed += PlayerBlockFailedSFX;
        _playerAttackController.OnWeakAttackCast += PlayerWeakAttackCastSFX;
        _playerAttackController.OnWeakAttackHit += PlayerWeakAttackHitSFX;
        _playerAttackController.OnStrongAttackCast += PlayerStrongAttackCastSFX;
        _playerAttackController.OnStrongAttackHit += PlayerStrongAttackHitSFX;
        _playerAttackController.OnCounterAttackCast += PlayerCounterAttackCastSFX;
        _playerAttackController.OnCounterAttackHit += PlayerCounterAttackHitSFX;
        _playerHealth.OnDead += PlayerDeadSFX;
    }

	private void OnDisable()
	{
        UnsubscribePlayerEvents();
    }
    private void UnsubscribePlayerEvents()
    {
        _playerController.OnRoll -= PlayerRollSFX;
        _playerController.OnJump -= PlayerJumpSFX;
        _playerBlockController.OnBlockCast -= PlayerBlockCastSFX;
        _playerBlockController.OnBlockFailed -= PlayerBlockFailedSFX;
        _playerAttackController.OnWeakAttackCast -= PlayerWeakAttackCastSFX;
        _playerAttackController.OnWeakAttackHit -= PlayerWeakAttackHitSFX;
        _playerAttackController.OnStrongAttackCast -= PlayerStrongAttackCastSFX;
        _playerAttackController.OnStrongAttackHit -= PlayerStrongAttackHitSFX;
        _playerAttackController.OnCounterAttackCast -= PlayerCounterAttackCastSFX;
        _playerAttackController.OnCounterAttackHit -= PlayerCounterAttackHitSFX;
        _playerHealth.OnDead += PlayerDeadSFX;
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

    private static int BGMTurnCounter = 0;
    private static int SFXTurnCounter = 0;
    public void PlayLoop(SoundEffectSO bgm)
    {
        if (bgm == null)
            return;

        for (int i = 0; i < BGM_AudioSourceCount; i++)
        {
            var audioSource = BGM_AudioSources[BGMTurnCounter];
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, bgm, bgm.clip.length, true));
                BGMTurnCounter = (BGMTurnCounter + 1) % BGM_AudioSourceCount;
                break;
            }
        }
    }

    public void PlayWhole(SoundEffectSO sfx)
    {
        if (sfx == null)
            return;

        for (int i = 0; i < SFX_AudioSourceCount; i++)
        {
            var audioSource = SFX_AudioSources[SFXTurnCounter];
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, sfx.clip.length));
                SFXTurnCounter = (SFXTurnCounter + 1) % SFX_AudioSourceCount;
                break;
            }
        }
    }

    public void PlayPartial(SoundEffectSO sfx, float duration)
    {
        if (sfx == null)
            return;

        for (int i = 0; i < SFX_AudioSourceCount; i++)
        {
            var audioSource = SFX_AudioSources[SFXTurnCounter];
            if (!audioSource.isPlaying)
            {
                StartCoroutine(StartPlay(audioSource, sfx, duration));
                SFXTurnCounter = (SFXTurnCounter + 1) % SFX_AudioSourceCount;
                break;
            }
        }
    }

    private IEnumerator StartPlay(AudioSource source, SoundEffectSO sfx, float duration, bool isLoop = false)
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
        source.loop = isLoop;
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

    public void StopAllBGM(float delay)
    {
        StartCoroutine(Delay(delay));

        foreach (var audioSource in BGM_AudioSources)
            audioSource.Stop();
    }

    private IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
}
