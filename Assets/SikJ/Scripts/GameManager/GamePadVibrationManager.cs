using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePadVibrationManager : MonoBehaviour
{
    private static IEnumerator currentVibration = null;
    private static int currentPriority = -1;
    
    [Header("Player Locomotion")]
    [SerializeField] private VibrationSO playerRoll;
    [SerializeField] private VibrationSO playerJump;

    [Header("Player Combat")]
    // Block
    [SerializeField] private VibrationSO playerBlockCast;
    [SerializeField] private VibrationSO playerBlockSucceed;
    [SerializeField] private VibrationSO playerBlockFailed;
    // Attack Cast
    [SerializeField] private VibrationSO playerWeakAttackCast;
    [SerializeField] private VibrationSO playerStrongAttackCast;
    [SerializeField] private VibrationSO playerCounterAttackCast;
    // Attack Hit
    [SerializeField] private VibrationSO playerWeakAttackHit;
    [SerializeField] private VibrationSO playerStrongAttackHit;
    [SerializeField] private VibrationSO playerCounterAttackHit;
    // Dead
    [SerializeField] private VibrationSO playerDead;

    public static GamePadVibrationManager _instance = null;
    public static GamePadVibrationManager Instance => _instance;

    private PlayerController _playerController;
    private AttackController _playerAttackController;
    private BlockController _playerBlockController;
    private Health _playerHealth;

    private event Action PlayerWeakAttackCastVibration = null;
    private event Action PlayerStrongAttackCastVibration = null;
    private event Action PlayerCounterAttackCastVibration = null;
    private event Action PlayerBlockCastVibration = null;
    private event Action PlayerBlockSucceedVibration = null;
    private event Action PlayerBlockFailedVibration = null;
    private event Action PlayerRollVibration = null;
    private event Action PlayerJumpVibration = null;
    private event Action PlayerWeakAttackHitVibration = null;
    private event Action PlayerStrongAttackHitVibration = null;
    private event Action PlayerCounterAttackHitVibration = null;
    private event Action PlayerDeadVibration = null;

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
    }

    private void OnEnable()
    {
        PlayerRollVibration = () => { Vibrate(playerRoll); };
        PlayerJumpVibration = () => { Vibrate(playerJump); };
        PlayerWeakAttackCastVibration = () => { Vibrate(playerWeakAttackCast); };
        PlayerWeakAttackHitVibration = () => { Vibrate(playerWeakAttackHit); };
        PlayerStrongAttackCastVibration = () => { Vibrate(playerStrongAttackCast); };
        PlayerStrongAttackHitVibration = () => { Vibrate(playerStrongAttackHit); };
        PlayerCounterAttackCastVibration = () => { Vibrate(playerCounterAttackCast); };
        PlayerCounterAttackHitVibration = () => { Vibrate(playerCounterAttackHit); };
        PlayerBlockCastVibration = () => { Vibrate(playerBlockCast); };
        PlayerBlockSucceedVibration = () => { Vibrate(playerBlockSucceed); };
        PlayerBlockFailedVibration = () => { Vibrate(playerBlockFailed); };
        PlayerDeadVibration = () => { Vibrate(playerDead); };

        SubscribePlayerEvents();
    }
    private void SubscribePlayerEvents()
    {
        _playerController.OnRoll += PlayerRollVibration;
        _playerController.OnJump += PlayerJumpVibration;
        _playerAttackController.OnWeakAttackCast += PlayerWeakAttackCastVibration;
        _playerAttackController.OnWeakAttackHit += PlayerWeakAttackHitVibration;
        _playerAttackController.OnStrongAttackCast += PlayerStrongAttackCastVibration;
        _playerAttackController.OnStrongAttackHit += PlayerStrongAttackHitVibration;
        _playerAttackController.OnCounterAttackCast += PlayerCounterAttackCastVibration;
        _playerAttackController.OnCounterAttackHit += PlayerCounterAttackHitVibration;
        _playerBlockController.OnBlockCast += PlayerBlockCastVibration;
        _playerBlockController.OnBlockSucceed += PlayerBlockSucceedVibration;
        _playerBlockController.OnBlockFailed += PlayerBlockFailedVibration;
        _playerHealth.OnDead += PlayerDeadVibration;
    }

    private void OnDisable()
    {
        UnsubscribePlayerEvents();
    }
    
    private void UnsubscribePlayerEvents()
    {
        if (_playerController != null)
        {
            _playerController.OnRoll -= PlayerRollVibration;
            _playerController.OnJump -= PlayerJumpVibration;
        }

        if (_playerAttackController != null)
        {
            _playerAttackController.OnWeakAttackCast -= PlayerWeakAttackCastVibration;
            _playerAttackController.OnWeakAttackHit -= PlayerWeakAttackHitVibration;
            _playerAttackController.OnStrongAttackCast -= PlayerStrongAttackCastVibration;
            _playerAttackController.OnStrongAttackHit -= PlayerStrongAttackHitVibration;
            _playerAttackController.OnCounterAttackCast -= PlayerCounterAttackCastVibration;
            _playerAttackController.OnCounterAttackHit -= PlayerCounterAttackHitVibration;
        }

        if (_playerBlockController != null)
        {
            _playerBlockController.OnBlockCast -= PlayerBlockCastVibration;
            _playerBlockController.OnBlockSucceed -= PlayerBlockSucceedVibration;
            _playerBlockController.OnBlockFailed -= PlayerBlockFailedVibration;
        }

        if (_playerHealth != null)
        {
            _playerHealth.OnDead -= PlayerDeadVibration;
        }
    }

    private void OnDestroy()
	{
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0, 0);
    }

	public void Vibrate(VibrationSO vibration)
    {
        if (vibration == null                           // 진동정보 없거나 잘못된 경우
            || Gamepad.current == null                  // 인식된 패드 없는 경우
            || currentPriority > vibration.Priority)    // 우선순위가 낮은 경우
            return;

        // 현재의 진동으로 우선순위 변경
        currentPriority = vibration.Priority;

        // 이미 진동 중이면 중단
        if (currentVibration != null)
        {
            StopCoroutine(currentVibration);
            currentVibration = null;
        }

        currentVibration = StartVibrate(vibration);
        StartCoroutine(currentVibration);
    }

    private IEnumerator StartVibrate(VibrationSO vibration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < vibration.Duration)
        {
            elapsedTime += Time.deltaTime;

            var lowMotorSpeed = vibration.LowMoterIntensity.Evaluate(elapsedTime / vibration.Duration);
            var highMotorSpeed = vibration.HighMoterIntensity.Evaluate(elapsedTime / vibration.Duration);
            Gamepad.current.SetMotorSpeeds(lowMotorSpeed, highMotorSpeed);

            yield return null;
        }
        Gamepad.current.SetMotorSpeeds(0, 0);
    }
}
