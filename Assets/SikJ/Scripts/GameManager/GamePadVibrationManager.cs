using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePadVibrationManager : MonoBehaviour
{
    [SerializeField] private static IEnumerator currentVibration = null;
    [SerializeField] private static int currentPriority = -1;
    
    [Header("Player Locomotion")]
    [SerializeField] private VibrationSO playerRoll;
    [SerializeField] private VibrationSO playerJump;

    [Header("Player Combat")]
    // Block
    [SerializeField] private VibrationSO playerBlockCast;
    [SerializeField] private VibrationSO playerBlockSucceed;
    // Attack Cast
    [SerializeField] private VibrationSO playerWeakAttackCast;
    [SerializeField] private VibrationSO playerStrongAttackCast;
    // Attack Hit
    [SerializeField] private VibrationSO playerWeakAttackHit;
    [SerializeField] private VibrationSO playerStrongAttackHit;
    // Dead
    [SerializeField] private VibrationSO playerDead;

    public static GamePadVibrationManager _instance = null;
    public static GamePadVibrationManager Instance => _instance;

    private PlayerController _playerController;
    private AttackController _playerAttackController;
    private BlockController _playerBlockController;
    private Health _playerHealth;

    private event Action playerWeakAttackCastVibration = null;
    private event Action playerStrongAttackCastVibration = null;
    private event Action playerBlockCastVibration = null;
    private event Action playerBlockSucceedVibration = null;
    private event Action playerRollVibration = null;
    private event Action playerJumpVibration = null;
    private event Action playerWeakAttackHitVibration = null;
    private event Action playerStrongAttackHitVibration = null;
    private event Action playerDeadVibration = null;

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
        playerRollVibration = () => { Vibrate(playerRoll); };
        playerJumpVibration = () => { Vibrate(playerJump); };
        playerWeakAttackCastVibration = () => { Vibrate(playerWeakAttackCast); };
        playerWeakAttackHitVibration = () => { Vibrate(playerWeakAttackHit); };
        playerStrongAttackCastVibration = () => { Vibrate(playerStrongAttackCast); };
        playerStrongAttackHitVibration = () => { Vibrate(playerStrongAttackHit); };
        playerBlockCastVibration = () => { Vibrate(playerBlockCast); };
        playerBlockSucceedVibration = () => { Vibrate(playerBlockSucceed); };
        playerDeadVibration = () => { Vibrate(playerDead); };

        _playerController.OnRoll += playerRollVibration;
        _playerController.OnJump += playerJumpVibration;
        _playerAttackController.OnWeakAttackCast += playerWeakAttackCastVibration;
        _playerAttackController.OnWeakAttackHit += playerWeakAttackHitVibration;
        _playerAttackController.OnStrongAttackCast += playerStrongAttackCastVibration;
        _playerAttackController.OnStrongAttackHit += playerStrongAttackHitVibration;
        _playerBlockController.OnBlockCast += playerBlockCastVibration;
        _playerBlockController.OnBlockSucceed += playerBlockSucceedVibration;
        _playerHealth.OnDead += playerDeadVibration;
    }

    private void OnDisable()
    {
        _playerController.OnRoll -= playerRollVibration;
        _playerController.OnJump -= playerJumpVibration;
        _playerAttackController.OnWeakAttackCast -= playerWeakAttackCastVibration;
        _playerAttackController.OnWeakAttackHit -= playerWeakAttackHitVibration;
        _playerAttackController.OnStrongAttackCast -= playerStrongAttackCastVibration;
        _playerAttackController.OnStrongAttackHit -= playerStrongAttackHitVibration;
        _playerBlockController.OnBlockCast -= playerBlockCastVibration;
        _playerBlockController.OnBlockSucceed -= playerBlockSucceedVibration;
        _playerHealth.OnDead -= playerDeadVibration;
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
