using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlState
{
    Controllable,
    Uncontrollable,
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(BlockController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Stamina))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConstantForce))]
public class PlayerController : MonoBehaviour
{
    [field:Header("State")]
    [field:SerializeField] public ControlState ControlState { get; set; } = ControlState.Controllable;
    public bool IsLockOn => LockOnTargetPoint != null;
    [field: SerializeField] public LockOnPoint LockOnTargetPoint { get; set; }
    [field: SerializeField] public bool IsRun { get; set; } = false;
    [field: SerializeField] public bool IsDead { get; set; } = false;

    [Header("Input Actions")]
    public InputAction move;
    public InputAction rotate;
    public InputAction run;
    public InputAction weakAttack;
    public InputAction strongAttack;
    public InputAction block;
    public InputAction roll;
    public InputAction jump;
    public InputAction lockOn;

    [Header("PlayerMove")]
    #region PlayerMove
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private AnimationCurve jumpAirSpeedOverTime;
    // �޸���� ������ �̵������� �ּ� ũ��
    private const float runThresholdScalar = 0.75f;
    [Tooltip("����� �޸��� �ʴ� ��� �ȱ�� ��ȯ�ϱ� ������ ���ð�")]
    [SerializeField] private float runThresholdTime = 0.5f;
    [Tooltip("LockOn �� �Ĺ����� �޸� �� �ִ� ����")]
    [SerializeField] [Range(0f, 90f)] private float runBehindAngle = 50f;
    #endregion

    [Header("LockOnEnemy")]
    #region LockOnEnemy
    [SerializeField] private GameObject UI_lockOnPoint;
    [SerializeField] private GameObject VC_Default;
    [SerializeField] private GameObject VC_LockOn;
    [SerializeField] private CinemachineTargetGroup TargetGroup;
    [SerializeField] private AnimationCurve resetRotationIntensity;
    [SerializeField] private float resetRotationTime = .5f;
    [SerializeField] private float enemyDetectDistance = 60f;
    [SerializeField] private float lockOnLimitDistance = 80f;
    [Tooltip("�þ߰��� ����")]
    [SerializeField] [Range(10f, 90f)] private float enemyDetectAngle = 15f;
    [SerializeField] private float blendTime = 1f;
    public GameObject LockedOnEnemy
    {
        get
        {
            if (LockOnTargetPoint != null)
                return LockOnTargetPoint.EnemyObject;

            return null;
        }
    }
    #endregion

    [Header("Ragdoll")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("Drop Equiments")]
    #region Drop Equiments
    [SerializeField] private float dropDelay = .5f;

    [SerializeField] private Transform swordParent;
    [SerializeField] private Transform sword;
    private Vector3 swordOriginPos;
    private Quaternion swordOriginRot;

    [SerializeField] private Transform shieldParent;
    [SerializeField] private Transform shield;
    private Vector3 shieldOriginPos;
    private Quaternion shieldOriginRot;

    [SerializeField] private List<Collider> weaponColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> weaponRigidbodies = new List<Rigidbody>();
    [SerializeField] private List<Collider> shieldColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> shieldRigidbodies = new List<Rigidbody>();
    #endregion

    private Rigidbody _rigidbody;
    private AttackController _attackController;
    private BlockController _blockController;
    private Health _health;
    private Stamina _stamina;
    private Animator _animator;
    #region AnimatorParameters
    private readonly int moveMagnitude_hash = Animator.StringToHash("moveMagnitude");
    private readonly int moveX_hash = Animator.StringToHash("moveX");
    private readonly int moveY_hash = Animator.StringToHash("moveY");
    private readonly int isSprint_hash = Animator.StringToHash("isSprint");
    private readonly int isWeakAttack_hash = Animator.StringToHash("isWeakAttack");
    private readonly int isStrongAttack_hash = Animator.StringToHash("isStrongAttack");
    private readonly int isCounterAttack_hash = Animator.StringToHash("isCounterAttack");
    private readonly int isJump_hash = Animator.StringToHash("isJump");
    private readonly int isBlock_hash = Animator.StringToHash("isBlock");
    private readonly int isRoll_hash = Animator.StringToHash("isRoll");
    #endregion
    private ConstantForce _constantForce;

    public event Action OnRoll;
    public event Action OnJump;
    public event Action OnLockOn;
    public event Action OnLockOff;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _attackController);
        TryGetComponent(out _blockController);
        TryGetComponent(out _health);
        TryGetComponent(out _stamina);
        TryGetComponent(out _animator);
        TryGetComponent(out _constantForce);
    }

    private void OnEnable()
    {
        #region Enable InputActions
        move.performed += OnMovePerformed;
        move.canceled += OnMoveCanceled;
        move.Enable();

        rotate.performed += OnRotatePerformed;
        rotate.canceled += OnRotateCanceled;
        rotate.Enable();

        run.performed += OnRunPerformed;
        run.Enable();

        weakAttack.performed += OnWeakAttackPerformed;
        weakAttack.Enable();

        strongAttack.performed += OnStrongAttackPerformed;
        strongAttack.Enable();

        block.performed += OnBlockPerformed;
        block.canceled += OnBlockCanceled;
        block.Enable();

        roll.performed += OnRollPerformed;
        roll.Enable();

        jump.performed += OnJumpPerformed;
        jump.Enable();

        lockOn.performed += OnLockOnPerformed;
        lockOn.Enable();
        #endregion
        _health.OnDead += Die;
        _blockController.OnKnockBackFinished += RecoverAfterKnockBack;
        _blockController.OnBlockFailed += RevertToDefault;
    }

    private void OnDisable()
    {
        #region Disable InputActions
        move.performed -= OnMovePerformed;
        move.performed -= OnMoveCanceled;
        move.Disable();

        rotate.performed -= OnRotatePerformed;
        rotate.canceled -= OnRotateCanceled;
        rotate.Disable();

        run.performed -= OnRunPerformed;
        run.Disable();

        weakAttack.performed -= OnWeakAttackPerformed;
        weakAttack.Disable();

        strongAttack.performed -= OnStrongAttackPerformed;
        strongAttack.Disable();

        block.performed -= OnBlockPerformed;
        block.canceled -= OnBlockCanceled;
        block.Disable();

        roll.performed -= OnRollPerformed;
        roll.Disable();

        jump.performed -= OnJumpPerformed;
        jump.Disable();

        lockOn.performed -= OnLockOnPerformed;
        lockOn.Disable();
        #endregion
        _health.OnDead -= Die;
        _blockController.OnKnockBackFinished -= RecoverAfterKnockBack;
        _blockController.OnBlockFailed -= RevertToDefault;
    }

    private void Start()
    {
        swordOriginPos = sword.localPosition;
        swordOriginRot = sword.localRotation;
        #region Set weapon colliders & rigidbodies
        foreach (Collider c in weaponColliders)
        {
            c.enabled = false;
        }
        foreach (Rigidbody rb in weaponRigidbodies)
        {
            rb.isKinematic = true;
        }
        #endregion
        shieldOriginPos = shield.localPosition;
        shieldOriginRot = shield.localRotation;
        #region Set shield colliders & rigidbodies
        foreach (Collider c in shieldColliders)
        {
            c.enabled = false;
        }
        foreach (Rigidbody rb in shieldRigidbodies)
        {
            rb.isKinematic = true;
        }
        #endregion
    }

    private void Update()
    {
        SetDefaultCameraPosition();
        CheckLockOnPointDistance();
        ShowLockOnPoint();
        LookLockOnPoint();
        
        Move();
        Animate();
    }
    
    private void CheckLockOnPointDistance()
    {
        if (!IsLockOn)
            return;

        var distance = Vector3.Distance(transform.position, LockOnTargetPoint.transform.position);
        if (distance > lockOnLimitDistance)
            UnlockOnPoint();
    }
    public void UnlockOnPoint()
    {
        UI_lockOnPoint.SetActive(false);
        ToggleTargetGroupCamera(false);
        LockOnTargetPoint.IsLockedOn = false;
        LockOnTargetPoint.StopTransitionCheck();
        LockOnTargetPoint = null;
    }
    private void LockOnPoint(LockOnPoint target)
    {
        UI_lockOnPoint.SetActive(true);
        ToggleTargetGroupCamera(true, target.gameObject);
        LockOnTargetPoint = target;
        LockOnTargetPoint.IsLockedOn = true;
        target.StartTransitionCheck();
    }
    [SerializeField] private bool isLockOnPointChangable = false;
    public bool TryChangeLockOnPoint(LockOnPoint from, LockOnPoint to)
    {
        if (!isLockOnPointChangable)
            return false;

        isLockOnPointChangable = false;

        TargetGroup.RemoveMember(LockOnTargetPoint.transform);
        LockOnTargetPoint = null;

        from.StopTransitionCheck();
        from.IsLockedOn = false;
        
        to.StartTransitionCheck();
        to.IsLockedOn = true;

        LockOnTargetPoint = to;
        TargetGroup.AddMember(LockOnTargetPoint.transform, 1, 50f);

        return true;
    }
    private void LookLockOnPoint()
    {
        if (!IsLockOn)
            return;

        var lockOnPointGroundPos = new Vector3(LockOnTargetPoint.transform.position.x, transform.position.y, LockedOnEnemy.transform.position.z);
        transform.LookAt(lockOnPointGroundPos);
    }
    private void ShowLockOnPoint()
    {
        if (!IsLockOn)
            return;

        var pos = Camera.main.WorldToScreenPoint(LockOnTargetPoint.transform.position);
        var rectTransform = UI_lockOnPoint.GetComponent<RectTransform>();
        var scale = rectTransform.localScale;
        var width = rectTransform.rect.width;
        var height = rectTransform.rect.height;

        var xOffset = scale.x * (width / 2);
        var yOffset = scale.y * (height / 2);

        UI_lockOnPoint.transform.position = new Vector3(pos.x - xOffset, pos.y + yOffset, pos.z);
    }
    private void SetDefaultCameraPosition()
    {
        if (!IsLockOn)
            return;

        if (!Camera.main.GetComponent<CinemachineBrain>().IsBlending)
            VC_Default.GetComponent<CinemachineFreeLook>().Follow.position = Camera.main.transform.position;
    }
    Vector3 moveDirection;
    private void Move()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;

        var isBackward = DesiredMove.y < Mathf.Sin(Mathf.PI + runBehindAngle * Mathf.Deg2Rad);
        if ((IsLockOn && isBackward)            // LockOn�� �� �Ĺ����� �̵��ϴ� ���
            || _stamina.CurrentStamina == 0)    // ���¹̳ʰ� 0�� ���
            IsRun = false;

        float currentSpeed = IsRun ? runSpeed : walkSpeed;

        if (IsLockOn)
        {
            moveDirection = new Vector3(DesiredMove.x, 0, DesiredMove.y);

            if (IsGoingToStair(moveDirection))
			{
                _constantForce.force = Physics.gravity * 1000;
                moveDirection += Vector3.up * defualtUpForce / (currentSpeed / walkSpeed);
            }
			else
			{
                _constantForce.force = Vector3.zero;
            }

            _rigidbody.MovePosition(transform.position + (currentSpeed * moveDirection.magnitude) * Time.deltaTime * moveDirection);

            if (IsRun)
                _stamina.Consume(_stamina.RunCostPerSeconds * Time.deltaTime);

            var forward = transform.forward * DesiredMove.y;
            var right = transform.right * DesiredMove.x;
            var desiredMove = forward + right;

            Debug.DrawLine(
                transform.position,                                                             // start
                transform.position + desiredMove * (currentSpeed * desiredMove.magnitude),      // end
                Color.green                                                                     // color
            );
        }
        // FreeLook�̸� desiredMove�� moveDirection�� ����
        else
        {
            var playerGroundPos = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z
            );
            var cameraGroundPos = new Vector3(
                Camera.main.transform.position.x,
                transform.position.y,
                Camera.main.transform.position.z
            );
            Vector3 cameraToPlayer = (playerGroundPos - cameraGroundPos);
            var forward = cameraToPlayer.normalized;
            var right = Vector3.Cross(Vector3.up, forward);
            moveDirection = forward * DesiredMove.y;
            moveDirection += right * DesiredMove.x;

            transform.LookAt(transform.position + moveDirection * currentSpeed);

            if (IsGoingToStair(moveDirection))
			{
                _constantForce.force = Physics.gravity * 1000;
                moveDirection += Vector3.up * defualtUpForce / (currentSpeed / walkSpeed);
            }
            else
            {
                _constantForce.force = Vector3.zero;
            }

            _rigidbody.MovePosition(transform.position + currentSpeed * Time.deltaTime * moveDirection);

            if (IsRun)
                _stamina.Consume(_stamina.RunCostPerSeconds * Time.deltaTime);

            Debug.DrawLine(
                transform.position,                                 // start
                transform.position + moveDirection * currentSpeed,  // end
                Color.green                                         // color
            );
        }
    }
    [SerializeField] private float defualtUpForce = 6f;
    [SerializeField] private float stairDetectionDistance = 20f;
    [SerializeField] private float stairDetectionOffsetUp = 2f;
    [SerializeField] private float stairDetectionOffsetForward = -.5f;
    private bool IsGoingToStair(Vector3 currentDirection)
    {
        if (DesiredMove == Vector2.zero)
            return false;

		bool isStairDetected = false;
		if (IsLockOn)
        {
            var forward = transform.forward * currentDirection.z;
            var right = transform.right * currentDirection.x;
            var moveDirection = forward + right;
            var offset = Vector3.up * stairDetectionOffsetUp + forward * stairDetectionOffsetForward;

            Debug.DrawLine(
                transform.position + offset + moveDirection * moveDirection.magnitude,
                transform.position + offset + moveDirection * moveDirection.magnitude + Vector3.down * stairDetectionDistance,
                Color.red);

            isStairDetected = Physics.Raycast(
                transform.position + offset + moveDirection * moveDirection.magnitude,  // Origin
                Vector3.down,                                                                   // Direction
                out RaycastHit hit,                                                             // HitInfo
                stairDetectionDistance,                                                         // MaxDistance
                1 << 14                                                                         // Layer (Stair = 14)
            );
        }
        else
        {
            var offset = Vector3.up * stairDetectionOffsetUp + transform.forward * stairDetectionOffsetForward;

            Debug.DrawLine(
                transform.position + offset + currentDirection,
                transform.position + offset + currentDirection + Vector3.down * stairDetectionDistance,
                Color.red);

            isStairDetected = Physics.Raycast(
                transform.position + offset + currentDirection,  // Origin
                Vector3.down,                                       // Direction
                out RaycastHit hit,                                 // HitInfo
                stairDetectionDistance,                             // MaxDistance
                1 << 14                                             // Layer (Stair = 14)
            );
        }
        
        return isStairDetected;
    }
    private void Animate()
    {
        // Move
        _animator.SetFloat(moveMagnitude_hash, moveDirection.magnitude);
        if (IsLockOn)
        {
            _animator.SetFloat(moveX_hash, moveDirection.x);
            _animator.SetFloat(moveY_hash, moveDirection.z);
        }
        else
        {
            _animator.SetFloat(moveX_hash, 0);
            _animator.SetFloat(moveY_hash, moveDirection.magnitude);
        }

        // Run
        _animator.SetBool(isSprint_hash, IsRun);

        // To-Do : Rotate
    }
    public Vector2 DesiredMove { get; private set; }
    #region move_Action
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        DesiredMove = context.ReadValue<Vector2>();
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        DesiredMove = Vector2.zero;
    }
    #endregion
    public Vector2 DesiredRotate { get; private set; }
    #region run_Action
    private IEnumerator currentCheckMovingEnoughToRun = null;
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (_stamina.CurrentStamina == 0)
            return;

        var isRun = context.ReadValueAsButton();
        if (isRun)
		{
            if (currentCheckMovingEnoughToRun != null)
            {
                StopCoroutine(currentCheckMovingEnoughToRun);
                currentCheckMovingEnoughToRun = null;
            }

            // Walk -> Run
			if (!IsRun)
			{
                currentCheckMovingEnoughToRun = CheckMovingEnoughToRun();
                StartCoroutine(currentCheckMovingEnoughToRun);
            }

			IsRun = !IsRun;
        }
    }
    private IEnumerator CheckMovingEnoughToRun()
	{
        float elapsedTimeAfterStopRunning = 0f;
		while (true)
		{
            if (elapsedTimeAfterStopRunning > runThresholdTime)
                break;

            if (IsRun && DesiredMove.magnitude < runThresholdScalar)
			{
                elapsedTimeAfterStopRunning += Time.deltaTime;
			}
			else if (IsRun && DesiredMove.magnitude >= runThresholdScalar)
			{
                elapsedTimeAfterStopRunning = 0f;
			}

            yield return null;
		}
        IsRun = false;
        currentCheckMovingEnoughToRun = null;
	}
    #endregion
    #region jump_Action
    private bool isJumping = false;
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (ControlState == ControlState.Uncontrollable
            || isJumping
            || _stamina.CurrentStamina < _stamina.JumpThreshold)
            return;

        var isJump = context.ReadValueAsButton();
        if (isJump)
		{
            // ������ �̵����� ����
            float lastSpeed = IsRun ? runSpeed : walkSpeed;
            Vector3 lastMovement = moveDirection * lastSpeed;

            isJumping = true;
            IsRun = false;
            _stamina.Consume(_stamina.JumpCost);
            _animator.SetBool(isJump_hash, true);
            OnJump?.Invoke();
            StartCoroutine(CancelJump(lastMovement));
        }
    }
    private IEnumerator CancelJump(Vector3 lastMovement)
    {
        var startSpeed = lastMovement * 5;
        var endSpeed = startSpeed / 2;

        float duration = .9f;
        float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
            elapsedTime += Time.deltaTime;
            var rateOverTime = jumpAirSpeedOverTime.Evaluate(elapsedTime / duration);
            var currentMovement = Vector3.Lerp(startSpeed, endSpeed, rateOverTime);
            _rigidbody.MovePosition(transform.position + currentMovement * Time.deltaTime);
            yield return null;
		}
        isJumping = false;
        _animator.SetBool(isJump_hash, false);
    }
    #endregion
    #region block_Action
    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        if (_attackController.IsCounterAttack)
            return;
        
        IsRun = false;
        ControlState = ControlState.Uncontrollable;
        _animator.SetBool(isBlock_hash, true);
        _stamina.Consume(_stamina.BlockCastCost);
    }
    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        if (_attackController.IsCounterAttack)
            return;
        
        ControlState = ControlState.Controllable;
        _animator.SetBool(isBlock_hash, false);
        _blockController.TurnOffBlockCollider();
    }
    #endregion
    private void RecoverAfterKnockBack()
    {
        if (!_attackController.IsCounterAttack)
        {
            _animator.SetBool(isBlock_hash, false);
            ControlState = ControlState.Controllable;
        }
    }
    private void RevertToDefault()
    {
        _animator.SetBool(isBlock_hash, false);
        ControlState = ControlState.Controllable;
    }
    #region roll_Action
    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        if (ControlState == ControlState.Uncontrollable
            || _stamina.CurrentStamina < _stamina.RollThreshold
            || moveDirection.magnitude < .25f)
            return;

        var isRoll = context.ReadValueAsButton();
        if (isRoll)
        {
            ControlState = ControlState.Uncontrollable;
            _animator.SetBool(isRoll_hash, true);

            OnRoll?.Invoke();
            _stamina.Consume(_stamina.RollCost);
            StartCoroutine(CancelRoll());
        }
    }
    // Animation Event
    public void ChangePlayerLayer(CombatLayerMask layer)
    {
        gameObject.layer = (int)layer;
    }
    private IEnumerator CancelRoll()
    {
        yield return new WaitForSeconds(.9f);
        _animator.SetBool(isRoll_hash, false);
        ControlState = ControlState.Controllable;
        IsRun = false;
    }
    #endregion
    #region weakAttack_Action
    private void OnWeakAttackPerformed(InputAction.CallbackContext context)
    {
        var isPressed = context.ReadValueAsButton();
        if (!isPressed)
            return;

        if (TryCounterAttack())
            return;
        
        if (ControlState == ControlState.Uncontrollable
            || _stamina.CurrentStamina < _stamina.WeakAttackThreshold)
            return;

        IsRun = false;
        ControlState = ControlState.Uncontrollable;
        _animator.SetBool(isWeakAttack_hash, true);
        _attackController.ChangeAttackType(AttackType.Weak);
        _stamina.Consume(_stamina.WeakAttackCost);
        StartCoroutine(CancelWeakAttack());
    }
    private IEnumerator CancelWeakAttack()
    {
        yield return new WaitForSeconds(1f);
        ControlState = ControlState.Controllable;
        _animator.SetBool(isWeakAttack_hash, false);
    }
    #endregion
    #region strongAttack_Action
    private void OnStrongAttackPerformed(InputAction.CallbackContext context)
    {
        var isPressed = context.ReadValueAsButton();
        if (!isPressed)
            return;

        if (TryCounterAttack())
            return;

        if (ControlState == ControlState.Uncontrollable
            || _stamina.CurrentStamina < _stamina.StrongAttackThreshold)
            return;

        IsRun = false;
        ControlState = ControlState.Uncontrollable;
        _animator.SetBool(isStrongAttack_hash, true);
        _attackController.ChangeAttackType(AttackType.Strong);
        _stamina.Consume(_stamina.StrongAttackCost);
        StartCoroutine(CancelStrongAttack());
        
    }
    private IEnumerator CancelStrongAttack()
    {
        yield return new WaitForSeconds(.9f);
        ControlState = ControlState.Controllable;
        _animator.SetBool(isStrongAttack_hash, false);
    }
    #endregion
    #region Counter Attack
    private bool TryCounterAttack()
    {
        if (!_attackController.IsCounterAttack
            || _stamina.CurrentStamina <= _stamina.CounterAttackThreshold)
            return false;

        ControlState = ControlState.Uncontrollable;
        _animator.SetBool(isCounterAttack_hash, true);
        _attackController.ChangeAttackType(AttackType.Counter);
        _stamina.Consume(_stamina.CounterAttackCost);
        _blockController.StopKnockBack();
        StartCoroutine(CancelCounterAttack());
        return true;
    }

    private IEnumerator CancelCounterAttack()
    {
        yield return new WaitForSeconds(1f);
        ControlState = ControlState.Controllable;
        _animator.SetBool(isBlock_hash, false);
        _animator.SetBool(isCounterAttack_hash, false);
        IsRun = false;

        _attackController.StopCounterAttackTime();
    }
    #endregion
    #region lockOn_Action
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        if (IsDead)
            return;

        if (IsLockOn)
        {
            UnlockOnPoint();
            OnLockOff();
            return;
        }

        var isPressed = context.ReadValueAsButton();
        var isBlending = Camera.main.GetComponent<CinemachineBrain>().IsBlending;
        if (!(isPressed && !isBlending))
            return;
        
        if (TryFindLockOnPointInRange(out LockOnPoint target))
		{
            LockOnPoint(target);
            OnLockOn();
        }
        else
        {
            StartCoroutine(ResetDefaultVCRotation());
        }
    }

    private bool TryFindLockOnPointInRange(out LockOnPoint targetPoint)
    {
        targetPoint = null;

        float maxDotValue = float.MinValue;
        float minDistance = float.MaxValue;

        Collider[] lockOnPointColliders = Physics.OverlapSphere(transform.position, enemyDetectDistance, 1 << 12);
        foreach (var lockOnPointCollider in lockOnPointColliders)
        {
            if (!lockOnPointCollider.TryGetComponent(out LockOnPoint lockOnPoint))
                continue;

            var playerGroundPos = new Vector3(transform.position.x, 0, transform.position.z);
            var cameraGroundPos = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
            var lockOnPointGroundPos = new Vector3(lockOnPoint.transform.position.x, 0, lockOnPoint.transform.position.z);

            // ī�޶� �þ߿� ���Դ��� Ȯ��
            Vector3 cameraToPlayer = playerGroundPos - cameraGroundPos;
            Vector3 cameraToEnemy = lockOnPointGroundPos - cameraGroundPos;
            var isBehind = Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized) < 0;
            var isOutOfDetectAngle = Vector3.Angle(cameraToPlayer, cameraToEnemy) > enemyDetectAngle;
            if (isBehind || isOutOfDetectAngle)
                continue;

            // �÷��̾� ��ġ ���� ���� ����� ������ Ȯ��
            float distance = Vector3.Distance(playerGroundPos, lockOnPointGroundPos);
            if (minDistance > distance)
            {
                minDistance = distance;
                targetPoint = lockOnPoint;
                continue;
            }

            // ī�޶� ���� ���� ���� �߾��� ������ Ȯ��
            float dot = Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized);
            if (maxDotValue < dot)
            {
                maxDotValue = dot;
                targetPoint = lockOnPoint;
            }
        }
        return targetPoint != null;
    }

    private void ToggleTargetGroupCamera(bool isTurnOn, GameObject target = null)
    {
        if (isTurnOn)
        {
            if (target != null)
                TargetGroup.AddMember(target.transform, 1, 50f);

            VC_LockOn.SetActive(true);
        }
        else
        {
            if (IsLockOn)
            {
                TargetGroup.RemoveMember(LockOnTargetPoint.transform);
				StartCoroutine(LerpDefaultCameraFollowPosition());
				StartCoroutine(LerpDefaultCameraLookAtPosition());
			}

            VC_LockOn.SetActive(false);
        }
    }

    private IEnumerator LerpDefaultCameraFollowPosition()
    {
        var lastCameraPos = Camera.main.transform.position;
        
        float elapsedTime = 0f;
        while (IsLockOn && elapsedTime < blendTime)
        {
            elapsedTime += Time.deltaTime;

            VC_Default.GetComponent<CinemachineFreeLook>().Follow.position = Vector3.Lerp(
                lastCameraPos,
                transform.position,
                elapsedTime / blendTime
            );

            yield return null;
        }

        VC_Default.GetComponent<CinemachineFreeLook>().Follow.position = transform.position;
    }

    private IEnumerator LerpDefaultCameraLookAtPosition()
    {
        float elapsedTime = 0f;
        while (IsLockOn && elapsedTime < blendTime)
        {
            elapsedTime += Time.deltaTime;

            var lastEnemyPos = LockOnTargetPoint.transform.position;
            var playerPos = transform.position;

            VC_Default.GetComponent<CinemachineFreeLook>().LookAt.position = Vector3.Lerp(
                lastEnemyPos,
                playerPos,
                elapsedTime / blendTime
            );
            
            yield return null;
        }

        VC_Default.GetComponent<CinemachineFreeLook>().LookAt.position = transform.position;
    }

    #endregion
    private IEnumerator ResetDefaultVCRotation()
    {
        // �� ����� �������� ȸ���ϵ��� ���� �ʿ�

        var startRotation = VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
        var endRotation = transform.localEulerAngles.y;

        float elapsedTime = 0f;
        while (elapsedTime < resetRotationTime)
        {
            elapsedTime += Time.deltaTime;
            var progress = resetRotationIntensity.Evaluate(elapsedTime / resetRotationTime);
            VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value = Mathf.Lerp(startRotation, endRotation, progress);
            yield return null;
        }
        VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value = endRotation;
    }
    #region rotate_Action
    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        DesiredRotate = context.ReadValue<Vector2>();
    }
    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        DesiredRotate = Vector2.zero;
        isLockOnPointChangable = true;
    }
    #endregion

    #region Die
    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        LockOnTargetPoint = null;
        UI_lockOnPoint.SetActive(false);
        ToggleTargetGroupCamera(false);

        ToggleRagdoll(true);
        StartCoroutine(DropEquipments(true));
    }
    public void ToggleRagdoll(bool isRagdoll)
    {
        #region Toggle ragdoll colliders & rigidbodies
        foreach (var c in ragdollColliders)
        {
            c.enabled = isRagdoll;
        }
        foreach (var rb in ragdollRigidbodies)
        {
            rb.useGravity = isRagdoll;
            rb.isKinematic = !isRagdoll;
        }
        #endregion

        // Toggle animation & control
        _animator.enabled = !isRagdoll;
        _rigidbody.velocity = Vector3.zero;
        ControlState = !isRagdoll ? ControlState.Controllable : ControlState.Uncontrollable;
    }
    /// <summary>
    /// Drop or pick-up equipments (sword and shield)
    /// </summary>
    /// <param name="isDrop">
    /// <para>true if drop</para>
    /// <para>false if pick-up</para>
    /// </param>
    private IEnumerator DropEquipments(bool isDrop)
    {
        if (isDrop)
            yield return new WaitForSeconds(dropDelay);

        #region Toggle weapon colliders & rigidbodies
        foreach (var c in weaponColliders)
        {
            c.enabled = isDrop;
        }
        foreach (var rb in weaponRigidbodies)
        {
            rb.useGravity = isDrop;
            rb.isKinematic = !isDrop;
            if (isDrop)
            {
                rb.transform.SetParent(null);
            }
            else
            {
                rb.transform.SetParent(swordParent);
                sword.localPosition = swordOriginPos;
                sword.localRotation = swordOriginRot;
            }
        }
        #endregion
        #region Toggle shield colliders & rigidbodies
        foreach (var c in shieldColliders)
        {
            c.enabled = isDrop;
        }
        foreach (var rb in shieldRigidbodies)
        {
            rb.useGravity = isDrop;
            rb.isKinematic = !isDrop;
            if (isDrop)
            {
                rb.transform.SetParent(null);
            }
            else
            {
                rb.transform.SetParent(shieldParent);
                shield.localPosition = shieldOriginPos;
                shield.localRotation = shieldOriginRot;
            }
        }
        #endregion
    }
    #endregion
    public void Revive()
    {
        ToggleRagdoll(false);
        StartCoroutine(DropEquipments(false));
        IsRun = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectDistance);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, lockOnLimitDistance);

        Gizmos.color = Color.magenta;
        if (IsLockOn)
        {
            // Debug detect enemy line of sight
            Gizmos.DrawLine(transform.position, new Vector3(
                LockedOnEnemy.transform.position.x,
                0,
                LockedOnEnemy.transform.position.z
            ));
        }
        else
        {
            // Debug detect enemy line of sight
            var playerGroundPos = new Vector3(
                transform.position.x,
                0,
                transform.position.z
            );
            var cameraGroundPos = new Vector3(
                Camera.main.transform.position.x,
                0,
                Camera.main.transform.position.z
            );
            Vector3 cameraToPlayer = playerGroundPos - cameraGroundPos;
            Gizmos.DrawLine(cameraGroundPos, cameraToPlayer.normalized * enemyDetectDistance);
        }
    }
}
