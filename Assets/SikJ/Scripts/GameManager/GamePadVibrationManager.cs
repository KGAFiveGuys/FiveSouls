using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePadVibrationManager : MonoBehaviour
{
    [SerializeField] private static IEnumerator currentVibration = null;
    [SerializeField] private static int currentPriority = -1;

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

    [Header("Player Locomotion")]
    [SerializeField] private VibrationSO playerRoll;
    [SerializeField] private VibrationSO playerJump;

    public static GamePadVibrationManager _instance = null;
    public static GamePadVibrationManager Instance => _instance;

    private PlayerController _playerController;

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

        _playerController = FindObjectOfType<PlayerController>();
    }

    private event Action playerWeakAttackCastAction = null;
    private event Action playerStrongAttackCastAction = null;
    private event Action playerBlockCastAction = null;
    private event Action playerRollAction = null;
    private event Action playerJumpAction = null;
    private event Action playerWeakAttackHitAction = null;
    private event Action playerStrongAttackHitAction = null;

    private void OnEnable()
    {
        playerWeakAttackCastAction = () => { Vibrate(playerWeakAttackCast); };
        playerStrongAttackCastAction = () => { Vibrate(playerStrongAttackCast); };
        playerBlockCastAction = () => { Vibrate(playerBlockCast); };
        playerRollAction = () => { Vibrate(playerRoll); };
        playerJumpAction = () => { Vibrate(playerJump); };

        _playerController.OnWeakAttack += playerWeakAttackCastAction;
        _playerController.OnStrongAttack += playerStrongAttackCastAction;
        _playerController.OnBlock += playerBlockCastAction;
        _playerController.OnRoll += playerRollAction;
        _playerController.OnJump += playerJumpAction;
    }

	private void OnDisable()
	{
        _playerController.OnWeakAttack -= playerWeakAttackCastAction;
        _playerController.OnStrongAttack -= playerStrongAttackCastAction;
        _playerController.OnBlock -= playerBlockCastAction;
        _playerController.OnRoll -= playerRollAction;
        _playerController.OnJump -= playerJumpAction;
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
