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
[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(BlockController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Stamina))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConstantForce))]
public class PlayerController : MonoBehaviour
{   
    [SerializeField] private PocketInventoryManager _inventoryManager;

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
    public InputAction pickUpItem;
    public InputAction talkToNPC;

    [Header("PlayerMove")]
    #region PlayerMove
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private AnimationCurve jumpAirSpeedOverTime;
    // 달리기로 간주할 이동벡터의 최소 크기
    private const float runThresholdScalar = 0.75f;
    [Tooltip("충분히 달리지 않는 경우 걷기로 전환하기 까지의 대기시간")]
    [SerializeField] private float runThresholdTime = 0.5f;
    [Tooltip("LockOn 시 후방으로 달릴 수 있는 각도")]
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
    [Tooltip("시야각의 절반")]
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
    private readonly int isWeakHit_hash = Animator.StringToHash("isWeakHit");
    private readonly int isStrongHit_hash = Animator.StringToHash("isStrongHit");
    private readonly int isDrinkPotion_hash = Animator.StringToHash("isDrinkPotion");
    private readonly int isCancelPotion_hash = Animator.StringToHash("isCancelPotion");
    #endregion
    private ConstantForce _constantForce;
    private PocketInventory _pocketInventory;

    public event Action OnRoll;
    public event Action OnJump;
    public event Action OnLockOn;
    public event Action OnLockOff;
    public event Action OnPickUpItem;
    public event Action OnTalkToNPC;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _attackController);
        TryGetComponent(out _blockController);
        TryGetComponent(out _health);
        TryGetComponent(out _stamina);
        TryGetComponent(out _animator);
        TryGetComponent(out _constantForce);
        TryGetComponent(out _pocketInventory);
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

        pickUpItem.performed += OnPickUpItemPerformed;
        pickUpItem.Enable();

        talkToNPC.performed += OnTalkToNPCPerformed;
        talkToNPC.Enable();
        #endregion
        _health.OnAttackHit += OnWeakHit;
        _health.OnAttackHit += OnStrongHit;
        _health.OnDead += Die;
        _blockController.OnKnockBackFinished += RecoverAfterKnockBack;
        _blockController.OnBlockFailed += RevertToDefault;
        _inventoryManager.OnUseItem += DrinkPotion;
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

        pickUpItem.performed -= OnPickUpItemPerformed;
        pickUpItem.Disable();

        talkToNPC.performed -= OnTalkToNPCPerformed;
        talkToNPC.Disable();
        #endregion
        _health.OnAttackHit -= OnWeakHit;
        _health.OnAttackHit -= OnStrongHit;
        _health.OnDead -= Die;
        _blockController.OnKnockBackFinished -= RecoverAfterKnockBack;
        _blockController.OnBlockFailed -= RevertToDefault;
        _inventoryManager.OnUseItem -= DrinkPotion;
    }


    public bool IsDrinkPotion { get; set; } = false;
	#region DrinkPotion
	private void DrinkPotion()
    {
        if (IsDead
            || IsDrinkPotion
            || ControlState == ControlState.Uncontrollable)
            return;

        IsRun = false;
        IsDrinkPotion = true;
        _animator.SetTrigger(isDrinkPotion_hash);
		StartCoroutine(CancelDrinkPotion());
	}
    private IEnumerator CancelDrinkPotion()
    {
        // 포션 사용이 중단되었는지 확인
        float elapsedTime = 0f;
		while (elapsedTime < 2.1f)
		{
            elapsedTime += Time.deltaTime;

			if (!IsDrinkPotion)
			{

                yield break;
            }

            yield return null;
		}
        
        _pocketInventory.UseCurrentItem();
        IsDrinkPotion = false;
    }
	#endregion

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

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        SetDefaultCameraPosition();
        CheckLockOnPointDistance();
        LookLockOnPoint();
        Animate();
    }

    private void LateUpdate()
    {
        ShowLockOnPoint();
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
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;

        UI_lockOnPoint.SetActive(false);
        ToggleTargetGroupCamera(false);
        LockOnTargetPoint.IsLockedOn = false;
        LockOnTargetPoint.StopTransitionCheck();
        LockOnTargetPoint = null;
    }
    private void LockOnPoint(LockOnPoint target)
    {
        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;

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
    public Vector3 MoveDirection { get; set; }
    private void Move()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;

        var isBackward = DesiredMove.y < Mathf.Sin(Mathf.PI + runBehindAngle * Mathf.Deg2Rad);
        if ((IsLockOn && isBackward)            // LockOn일 때 후방으로 이동하는 경우
            || _stamina.CurrentStamina == 0)    // 스태미너가 0인 경우
            IsRun = false;

        float currentSpeed = IsRun ? runSpeed : walkSpeed;

        if (IsLockOn)
        {
            MoveDirection = new Vector3(DesiredMove.x, 0, DesiredMove.y);

            if (IsGoingToStair(MoveDirection, out bool isGrounded))
			{
                _constantForce.force = isGrounded ? Physics.gravity * 1000 : Physics.gravity * 4000;

                if (isGrounded)
                    MoveDirection += Vector3.up * defualtUpForce / (currentSpeed / walkSpeed);
            }
			else
			{
                //_constantForce.force = Vector3.zero;
                _constantForce.force = isGrounded ? Physics.gravity * 500 : Physics.gravity * 4000;
            }

            _rigidbody.MovePosition(transform.position + (currentSpeed * MoveDirection.magnitude) * Time.deltaTime * MoveDirection);

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
        // FreeLook이면 desiredMove로 moveDirection을 조정
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
            MoveDirection = forward * DesiredMove.y;
            MoveDirection += right * DesiredMove.x;

            transform.LookAt(transform.position + MoveDirection * currentSpeed);

            if (IsGoingToStair(MoveDirection, out bool isGrounded))
			{
                _constantForce.force = isGrounded ? Physics.gravity * 1000 : Physics.gravity * 4000;

				if (DesiredMove != Vector2.zero && isGrounded)
                    MoveDirection += Vector3.up * defualtUpForce / (currentSpeed / walkSpeed);
            }
            else
            {
                _constantForce.force = isGrounded ? Physics.gravity * 500 : Physics.gravity * 4000;
            }

            _rigidbody.MovePosition(transform.position + currentSpeed * Time.deltaTime * MoveDirection);

            if (IsRun)
                _stamina.Consume(_stamina.RunCostPerSeconds * Time.deltaTime);

            Debug.DrawLine(
                transform.position,                                 // start
                transform.position + MoveDirection * currentSpeed,  // end
                Color.green                                         // color
            );
        }
    }
    [SerializeField] private float defualtUpForce = 6f;
    [SerializeField] private float stairDetectionDistance = 20f;
    [SerializeField] private float stairDetectionOffsetUp = 2f;
    [SerializeField] private float stairDetectionOffsetForward = -.5f;
    private bool IsGoingToStair(Vector3 currentDirection, out bool isGrounded)
    {
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

            isGrounded = hit.distance <= stairDetectionOffsetUp;
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

            isGrounded = hit.distance <= stairDetectionOffsetUp;
        }
        
        return isStairDetected;
    }
    private void Animate()
    {
        // Move
        _animator.SetFloat(moveMagnitude_hash, MoveDirection.magnitude);
        if (IsLockOn)
        {
            _animator.SetFloat(moveX_hash, MoveDirection.x);
            _animator.SetFloat(moveY_hash, MoveDirection.z);
        }
        else
        {
            _animator.SetFloat(moveX_hash, 0);
            _animator.SetFloat(moveY_hash, MoveDirection.magnitude);
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
        if (_stamina.CurrentStamina == 0
            || IsDrinkPotion)
            return;

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
            || IsDrinkPotion
            || isJumping
            || _stamina.CurrentStamina < _stamina.JumpThreshold)
            return;
        
        // 마지막 이동정보 저장
        float lastSpeed = IsRun ? runSpeed : walkSpeed;
        Vector3 lastMovement = MoveDirection * lastSpeed;

        isJumping = true;
        IsRun = false;
        _stamina.Consume(_stamina.JumpCost);
        _animator.SetBool(isJump_hash, true);
        OnJump?.Invoke();
        StartCoroutine(CancelJump(lastMovement));
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
    private bool isBlocking = false;
    #region block_Action
    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        if (ControlState == ControlState.Uncontrollable
            || isBlocking
            || _attackController.IsCounterAttack)
            return;

        IsRun = false;
        ControlState = ControlState.Uncontrollable;
        _stamina.Consume(_stamina.BlockCastCost);
        _animator.SetBool(isBlock_hash, true);
        StartCoroutine(TurnOnBlock(6));
    }
    private IEnumerator TurnOnBlock(int frames)
	{
        int frameCount = 0;
		while (frameCount < frames)
		{
            frameCount++;
            yield return null;
        }
        _blockController.TurnOnBlockCollider();
        isBlocking = true;
	}
    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        if (!isBlocking
            || _attackController.IsCounterAttack)
            return;

        isBlocking = false;
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
            isBlocking = false;
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
            || IsDrinkPotion
            || _stamina.CurrentStamina < _stamina.RollThreshold
            || MoveDirection.magnitude < .25f)
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
        if (isCounterAttacking
            || TryCounterAttack())
            return;
        
        if (ControlState == ControlState.Uncontrollable
            || IsDrinkPotion
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
        yield return new WaitForSeconds(.72f);
        ControlState = ControlState.Controllable;
        _animator.SetBool(isWeakAttack_hash, false);
    }
    #endregion
    #region strongAttack_Action
    private void OnStrongAttackPerformed(InputAction.CallbackContext context)
    {
        if (isCounterAttacking
            || TryCounterAttack())
            return;

        if (ControlState == ControlState.Uncontrollable
            || IsDrinkPotion
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
    private bool isCounterAttacking = false;
    private bool TryCounterAttack()
    {
        if (!_attackController.IsCounterAttack
            || _stamina.CurrentStamina <= _stamina.CounterAttackThreshold)
            return false;

        isCounterAttacking = true;

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

        isCounterAttacking = false;
    }
    #endregion
    #region lockOn_Action
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        if (IsDead)
            return;

        var cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        var isBlending = cinemachineBrain.IsBlending;
        if (isBlending)
            return;

        if (IsLockOn)
        {
            UnlockOnPoint();
            OnLockOff();
            return;
        }
        
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

            // 카메라 시야에 들어왔는지 확인
            Vector3 cameraToPlayer = playerGroundPos - cameraGroundPos;
            Vector3 cameraToEnemy = lockOnPointGroundPos - cameraGroundPos;
            var isBehind = Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized) < 0;
            var isOutOfDetectAngle = Vector3.Angle(cameraToPlayer, cameraToEnemy) > enemyDetectAngle;
            if (isBehind || isOutOfDetectAngle)
                continue;

            // 플레이어 위치 기준 가장 가까운 적인지 확인
            float distance = Vector3.Distance(playerGroundPos, lockOnPointGroundPos);
            if (minDistance > distance)
            {
                minDistance = distance;
                targetPoint = lockOnPoint;
                continue;
            }

            // 카메라 시점 기준 가장 중앙의 적인지 확인
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
        // 더 가까운 방향으로 회전하도록 수정 필요

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
    #region pickUpItem_Action
    private void OnPickUpItemPerformed(InputAction.CallbackContext context)
    {
        if (IsDead
            || IsDrinkPotion
            || ControlState == ControlState.Uncontrollable)
            return;

        OnPickUpItem?.Invoke();
    }
    #endregion
    
    #region talkToNPC_Action
    private void OnTalkToNPCPerformed(InputAction.CallbackContext context)
    {
        if (IsDead
            || IsDrinkPotion
            || ControlState == ControlState.Uncontrollable)
            return;

        OnTalkToNPC?.Invoke();
    }
    #endregion
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

    #region Hit
    public void OnWeakHit(AttackType type)
    {
        if (type != AttackType.Weak)
            return;

        ControlState = ControlState.Uncontrollable;
        
        _attackController.TurnOffAttackCollider();
        InterruptAllActions();

        SFXManager.Instance.OnPlayerWeakHit();
        _animator.SetTrigger(isWeakHit_hash);
        StartCoroutine(CancelWeakHit());
    }
    private IEnumerator CancelWeakHit()
    {
        yield return new WaitForSeconds(.71f);

        _animator.ResetTrigger(isWeakHit_hash);
        ControlState = ControlState.Controllable;
    }
    public void OnStrongHit(AttackType type)
    {
        if (type != AttackType.Strong)
            return;

        ControlState = ControlState.Uncontrollable;

        _attackController.TurnOffAttackCollider();
        InterruptAllActions();

        SFXManager.Instance.OnPlayerStrongHit();
        _animator.SetTrigger(isStrongHit_hash);
        StartCoroutine(CancelStrongHit());
    }
    public IEnumerator CancelStrongHit()
    {
        yield return new WaitForSeconds(1f);

        _animator.ResetTrigger(isStrongHit_hash);
        ControlState = ControlState.Controllable;
    }
    private void InterruptAllActions()
    {
        IsDrinkPotion = false;
        _animator.SetTrigger(isCancelPotion_hash);
        _animator.SetBool(isWeakAttack_hash, false);
        _animator.SetBool(isStrongAttack_hash, false);
        _animator.SetBool(isCounterAttack_hash, false);
        _animator.SetBool(isJump_hash, false);
        _animator.SetBool(isRoll_hash, false);
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

        gameObject.layer = LayerMask.NameToLayer("Ghost");
        _constantForce.force = Vector3.zero;
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
