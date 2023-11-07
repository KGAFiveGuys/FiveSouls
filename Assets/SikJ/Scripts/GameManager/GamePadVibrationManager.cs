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
    [SerializeField] private VibrationSO playerEquipmentFall;

    public static GamePadVibrationManager _instance = null;
    public static GamePadVibrationManager Instance => _instance;

    private PlayerController _playerController;
    private AttackController _playerAttackController;
    private BlockController _playerBlockController;

    private event Action playerWeakAttackCastAction = null;
    private event Action playerStrongAttackCastAction = null;
    private event Action playerBlockCastAction = null;
    private event Action playerBlockSucceedAction = null;
    private event Action playerRollAction = null;
    private event Action playerJumpAction = null;
    private event Action playerWeakAttackHitAction = null;
    private event Action playerStrongAttackHitAction = null;

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
    }

    private void OnEnable()
    {
        playerRollAction = () => { Vibrate(playerRoll); };
        playerJumpAction = () => { Vibrate(playerJump); };
        playerWeakAttackCastAction = () => { Vibrate(playerWeakAttackCast); };
        playerWeakAttackHitAction = () => { Vibrate(playerWeakAttackHit); };
        playerStrongAttackCastAction = () => { Vibrate(playerStrongAttackCast); };
        playerStrongAttackHitAction = () => { Vibrate(playerStrongAttackHit); };
        playerBlockCastAction = () => { Vibrate(playerBlockCast); };
        playerBlockSucceedAction = () => { Vibrate(playerBlockSucceed); };

        _playerController.OnRoll += playerRollAction;
        _playerController.OnJump += playerJumpAction;
        _playerAttackController.OnWeakAttackCast += playerWeakAttackCastAction;
        _playerAttackController.OnWeakAttackHit += playerWeakAttackHitAction;
        _playerAttackController.OnStrongAttackCast += playerStrongAttackCastAction;
        _playerAttackController.OnStrongAttackHit += playerStrongAttackHitAction;
        _playerBlockController.OnBlockCast += playerBlockCastAction;
        _playerBlockController.OnBlockSucceed += playerBlockSucceedAction;
    }

    private void OnDisable()
    {
        _playerController.OnRoll -= playerRollAction;
        _playerController.OnJump -= playerJumpAction;
        _playerAttackController.OnWeakAttackCast -= playerWeakAttackCastAction;
        _playerAttackController.OnWeakAttackHit -= playerWeakAttackHitAction;
        _playerAttackController.OnStrongAttackCast -= playerStrongAttackCastAction;
        _playerAttackController.OnStrongAttackHit -= playerStrongAttackHitAction;
        _playerBlockController.OnBlockCast -= playerBlockCastAction;
        _playerBlockController.OnBlockSucceed -= playerBlockSucceedAction;
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
