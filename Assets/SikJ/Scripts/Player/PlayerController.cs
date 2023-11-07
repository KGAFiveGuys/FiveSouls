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
public class PlayerController : MonoBehaviour
{
    [field:Header("State")]
    [field:SerializeField] public ControlState ControlState { get; set; } = ControlState.Controllable;
    [field: SerializeField] public bool IsLockOn { get; set; } = false;
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

    public event Action OnRoll;
    public event Action OnJump;
    public event Action OnLockOn;
    public event Action OnLockOff;

    [Header("PlayerMove")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    // 달리기로 간주할 이동벡터의 최소 크기
    private const float runThresholdScalar = 0.75f;
    [Tooltip("충분히 달리지 않는 경우 걷기로 전환하기 까지의 대기시간")]
    [SerializeField] private float runThresholdTime = 0.5f;
    [SerializeField] private float rotateSpeed = 60f;
    [SerializeField] private float jumpForce = 20f;
    [Tooltip("LockOn 시 후방으로 달릴 수 있는 각도")]
    [SerializeField] [Range(0f, 90f)] private float runBehindAngle = 50f;

    [Header("LockOnEnemy")]
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
    [field:SerializeField] public GameObject LockedOnEnemy { get; private set; }

    [Header("Ragdoll")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("Drop Equiment")]
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

    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private AttackController _attackController;
    private BlockController _blockController;
    private Health _health;
    private Stamina _stamina;
    private Animator _animator;
    #region AnimatorParameters
    private readonly int moveX_hash = Animator.StringToHash("moveX");
    private readonly int moveY_hash = Animator.StringToHash("moveY");
    private readonly int isSprint_hash = Animator.StringToHash("isSprint");
    private readonly int isWeakAttack_hash = Animator.StringToHash("isWeakAttack");
    private readonly int isStrongAttack_hash = Animator.StringToHash("isStrongAttack");
    private readonly int isJump_hash = Animator.StringToHash("isJump");
    private readonly int isBlock_hash = Animator.StringToHash("isBlock");
    private readonly int isRoll_hash = Animator.StringToHash("isRoll");
    #endregion

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _collider);
        TryGetComponent(out _attackController);
        TryGetComponent(out _blockController);
        TryGetComponent(out _health);
        TryGetComponent(out _stamina);
        TryGetComponent(out _animator);

        swordOriginPos = sword.localPosition;
        swordOriginRot = sword.localRotation;
        shieldOriginPos = shield.localPosition;
        shieldOriginRot = shield.localRotation;
    }

    private void Start()
    {
        _health.OnDead += () => { IsDead = true; };

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

        CheckLockOnEnemyDistance();
        LookLockOnEnemy();
        ShowLockOnPoint();
        
        MovePlayer();
        AnimatePlayerMove();
    }
    
    private void CheckLockOnEnemyDistance()
    {
        // LockOn 상태에서 제한범위를 벗어나면 UnLock
        if (IsLockOn && Vector3.Distance(transform.position, LockedOnEnemy.transform.position) > lockOnLimitDistance)
        {
            UnlockOnEnemy();
        }
    }
    public void UnlockOnEnemy()
	{
        UI_lockOnPoint.SetActive(false);
        ToggleTargetGroupCamera(false);
        IsLockOn = false;
        VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value = Camera.main.transform.localEulerAngles.y;
    }
    private void LookLockOnEnemy()
    {
        if (!IsLockOn)
            return;

        var lookAtPos = new Vector3(LockedOnEnemy.transform.position.x, 0, LockedOnEnemy.transform.position.z);
        transform.LookAt(lookAtPos);
    }
    private void ShowLockOnPoint()
    {
        if (!IsLockOn)
            return;

        var pos = Camera.main.WorldToScreenPoint(LockedOnEnemy.transform.position);
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
        if (IsLockOn && !Camera.main.GetComponent<CinemachineBrain>().IsBlending)
            VC_Default.GetComponent<CinemachineFreeLook>().Follow.position = Camera.main.transform.position;
    }
    
    Vector3 moveDirection;
    private void MovePlayer()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;

        // LockOn일 때 후방으로 이동하면 달릴 수 없음
        if (IsLockOn && desiredMove.y < Mathf.Sin(Mathf.PI + runBehindAngle * Mathf.Deg2Rad))
            IsRun = false;

        // 스태미너가 0이면 달릴 수 없음
        if (_stamina.CurrentStamina == 0)
            IsRun = false;

        float speed = IsRun ? runSpeed : walkSpeed;
        if (IsLockOn)
        {
            moveDirection = new Vector3(desiredMove.x, 0, desiredMove.y);
            transform.Translate(moveDirection * (speed * moveDirection.magnitude) * Time.deltaTime);

            // To-Do : Collision Check

            if (IsRun)
                _stamina.Consume(_stamina.RunCostPerSeconds * Time.deltaTime);

            Debug.DrawLine(transform.position, transform.position + moveDirection * speed, Color.green);
        }
        // FreeLook이면 desiredMove로 moveDirection을 조정
        else
        {
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
            Vector3 cameraToPlayer = (playerGroundPos - cameraGroundPos);
            var forward = cameraToPlayer.normalized;
            var right = Vector3.Cross(Vector3.up, forward);
            moveDirection = forward * desiredMove.y;
            moveDirection += right * desiredMove.x;

            transform.LookAt(transform.position + moveDirection * speed);
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

            // To-Do : Collision Check

            if (IsRun)
                _stamina.Consume(_stamina.RunCostPerSeconds * Time.deltaTime);
        }
        Debug.DrawLine(transform.position, transform.position + moveDirection * speed, Color.green);
    }
    private void AnimatePlayerMove()
    {
        // Move
        if (IsLockOn)
        {
            _animator.SetFloat(moveX_hash, moveDirection.x);
            _animator.SetFloat(moveY_hash, moveDirection.z);    
        }
        else
        {
            var value = Mathf.Sqrt(moveDirection.x * moveDirection.x + moveDirection.z * moveDirection.z);
            _animator.SetFloat(moveX_hash, 0);
            _animator.SetFloat(moveY_hash, value);
        }

        // Run
        _animator.SetBool(isSprint_hash, IsRun);

        // To-Do : Rotate
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
        weakAttack.canceled += OnWeakAttackCanceled;
        weakAttack.Enable();

        strongAttack.performed += OnStrongAttackPerformed;
        strongAttack.canceled += OnStrongAttackCanceled;
        strongAttack.Enable();

        block.performed += OnBlockPerformed;
        block.canceled += OnBlockCanceled;
        block.Enable();

        roll.performed += OnRollPerformed;
        roll.canceled += OnRollCanceled;
        roll.Enable();

        jump.performed += OnJumpPerformed;
        jump.canceled += OnJumpCanceled;
        jump.Enable();

        lockOn.performed += OnLockOnPerformed;
        lockOn.Enable();
        #endregion
        _health.OnDead += Die;
        _attackController.OnWeakAttackCast += SFXManager.Instance.OnPlayerWeakAttackCast;
        _attackController.OnWeakAttackHit += SFXManager.Instance.OnPlayerWeakAttackHit;
        _attackController.OnStrongAttackCast += SFXManager.Instance.OnPlayerStrongAttackCast;
        _attackController.OnStrongAttackHit += SFXManager.Instance.OnPlayerStrongAttackHit;
    }

    private void OnDisable()
    {
        #region Disable InputActions
        move.Disable();
        move.performed -= OnMovePerformed;
        move.performed -= OnMoveCanceled;

        rotate.Disable();
        rotate.performed -= OnRotatePerformed;
        rotate.performed -= OnRotateCanceled;

        run.performed -= OnRunPerformed;
        run.Disable();

        weakAttack.Disable();
        weakAttack.performed -= OnWeakAttackPerformed;
        weakAttack.canceled -= OnWeakAttackCanceled;

        strongAttack.Disable();
        strongAttack.performed -= OnStrongAttackPerformed;
        strongAttack.canceled -= OnStrongAttackCanceled;

        block.performed -= OnBlockPerformed;
        block.canceled -= OnBlockCanceled;
        block.Disable();

        roll.performed -= OnRollPerformed;
        roll.canceled -= OnRollCanceled;
        roll.Disable();

        jump.performed -= OnJumpPerformed;
        jump.canceled -= OnJumpCanceled;
        jump.Disable();

        lockOn.performed -= OnLockOnPerformed;
        lockOn.Disable();
        #endregion
        _health.OnDead -= Die;
        _attackController.OnWeakAttackCast -= SFXManager.Instance.OnPlayerWeakAttackCast;
        _attackController.OnWeakAttackHit -= SFXManager.Instance.OnPlayerWeakAttackHit;
        _attackController.OnStrongAttackCast -= SFXManager.Instance.OnPlayerStrongAttackCast;
        _attackController.OnStrongAttackHit -= SFXManager.Instance.OnPlayerStrongAttackHit;
    }

    private Vector2 desiredMove;
    #region move_Action
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        desiredMove = context.ReadValue<Vector2>();
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        desiredMove = Vector2.zero;
    }
    #endregion
    private Vector2 desiredRotate;
    #region rotate_Action
    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        desiredRotate = context.ReadValue<Vector2>().normalized;
    }
    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        desiredRotate = Vector2.zero;
    }
    #endregion
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

            if (IsRun && desiredMove.magnitude < runThresholdScalar)
			{
                elapsedTimeAfterStopRunning += Time.deltaTime;
			}
			else if (IsRun && desiredMove.magnitude >= runThresholdScalar)
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
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_stamina.CurrentStamina < _stamina.JumpThreshold)
            return;

        var isJump = context.ReadValueAsButton();
        if (isJump)
		{
            _stamina.Consume(_stamina.JumpCost);
            _animator.SetBool(isJump_hash, true);
            OnJump?.Invoke();
        }
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        _animator.SetBool(isJump_hash, false);
    }
    #endregion
    #region block_Action
    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        var isBlock = context.ReadValueAsButton();
        if (isBlock)
        {
            ControlState = ControlState.Uncontrollable;
            _stamina.Consume(_stamina.BlockCost);
            _animator.SetBool(isBlock_hash, true);
        }
    }
    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        _animator.SetBool(isBlock_hash, false);
        IsRun = false;
        _blockController.TurnOffBlockCollider();
    }
    #endregion
    #region roll_Action
    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        if (_stamina.CurrentStamina < _stamina.RollThreshold)
            return;

        var isBlock = context.ReadValueAsButton();
        if (isBlock)
        {
            ControlState = ControlState.Uncontrollable;
            _stamina.Consume(_stamina.RollCost);
            _animator.SetBool(isRoll_hash, true);
            OnRoll?.Invoke();
        }
    }
    private void OnRollCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        _animator.SetBool(isRoll_hash, false);
        IsRun = false;
    }
    #endregion
    #region weakAttack_Action
    private void OnWeakAttackPerformed(InputAction.CallbackContext context)
    {
        if (_stamina.CurrentStamina < _stamina.WeakAttackThreshold)
            return;

        var isWeakAttack = context.ReadValueAsButton();
        if (isWeakAttack)
        {
            ControlState = ControlState.Uncontrollable;
            _attackController.ChangeAttackType(AttackType.Weak);
            _stamina.Consume(_stamina.WeakAttackCost);
            _animator.SetBool(isWeakAttack_hash, true);
        }
    }
    private void OnWeakAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        _animator.SetBool(isWeakAttack_hash, false);
        IsRun = false;
    }
    #endregion
    #region strongAttack_Action
    private void OnStrongAttackPerformed(InputAction.CallbackContext context)
    {
        if (_stamina.CurrentStamina < _stamina.StrongAttackThreshold)
            return;

        var isStrongAttack = context.ReadValueAsButton();
        if (isStrongAttack)
        {
            ControlState = ControlState.Uncontrollable;
            _attackController.ChangeAttackType(AttackType.Strong);
            _stamina.Consume(_stamina.StrongAttackCost);
            _animator.SetBool(isStrongAttack_hash, true);
        }
    }
    private void OnStrongAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        _animator.SetBool(isStrongAttack_hash, false);
        IsRun = false;
    }
    #endregion
	#region lockOn_Action
	private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        if (IsDead)
            return;

        var isLockOn = context.ReadValueAsButton();
        var isBlending = Camera.main.GetComponent<CinemachineBrain>().IsBlending;
        var isEnemyDetected = CheckEnemyInRange();
        if (isLockOn && !isBlending)
		{
			if (isEnemyDetected)
			{
                IsLockOn = !IsLockOn;

                if (IsLockOn == true)
                {
                    OnLockOn();
                }
                else
                {
                    OnLockOff();
                    UnlockOnEnemy();
                }
            }
            // 적이 없는 경우 카메라를 플레이어 정면방향으로 회전
			else
			{
                StartCoroutine(ResetDefaultVCRotation());
            }
        }
    }

    private IEnumerator ResetDefaultVCRotation()
    {
        // 더 가까운 방향으로 회전하도록 수정 필요

        var startRotation = VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
        var endRotation = transform.localEulerAngles.y;

        float elapsedTime = 0f;
        var progress = 0f;
        while (elapsedTime < resetRotationTime)
        {
            elapsedTime += Time.deltaTime;
            progress = resetRotationIntensity.Evaluate(elapsedTime / resetRotationTime);
            VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value = Mathf.Lerp(startRotation, endRotation, progress);
            yield return null;
        }
        VC_Default.GetComponent<CinemachineFreeLook>().m_XAxis.Value = endRotation;
    }

    private bool CheckEnemyInRange()
    {
        if (IsLockOn)
            return true;

        LockedOnEnemy = null;

        float maxDotValue = float.MinValue;
        float minDistance = float.MaxValue;

        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, enemyDetectDistance, 1 << 8);
        foreach (var enemyCollider in enemyColliders)
        {
            var playerGroundPos = new Vector3(transform.position.x, 0, transform.position.z);
            var cameraGroundPos = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
            var enemyGroundPos = new Vector3(enemyCollider.transform.position.x, 0, enemyCollider.transform.position.z);

            // 카메라 시야에 있는지 확인
            Vector3 cameraToPlayer = playerGroundPos - cameraGroundPos;
            Vector3 cameraToEnemy = enemyGroundPos - cameraGroundPos;
            if (Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized) < 0    // 후방
                || Vector3.Angle(cameraToPlayer, cameraToEnemy) > enemyDetectAngle)     // 감지 범위 초과
                continue;

            // 플레이어 위치 기준 가장 가까운 적인지 확인
            float distance = Vector3.Distance(playerGroundPos, enemyGroundPos);
            if (minDistance > distance)
            {
                minDistance = distance;
                LockedOnEnemy = enemyCollider.gameObject;
            }
            
            // 카메라 시점 기준 가장 중앙의 적인지 확인
            float dot = Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized);
            if (maxDotValue < dot)
            {
                maxDotValue = dot;
                LockedOnEnemy = enemyCollider.gameObject;
                continue;
            }
        }

        UI_lockOnPoint.SetActive(LockedOnEnemy != null);
        ToggleTargetGroupCamera(LockedOnEnemy != null, LockedOnEnemy);
        return LockedOnEnemy != null;
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
            if (LockedOnEnemy != null)
            {
                TargetGroup.RemoveMember(LockedOnEnemy.transform);
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
        while (LockedOnEnemy != null && elapsedTime < blendTime)
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
        while (LockedOnEnemy != null && elapsedTime < blendTime)
        {
            elapsedTime += Time.deltaTime;

            var lastEnemyPos = LockedOnEnemy.transform.position;
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
    
	#region Die
	public void Die()
    {
        if (IsDead)
            return;

        IsLockOn = false;
        UI_lockOnPoint.SetActive(false);
        ToggleTargetGroupCamera(false);

        SFXManager.Instance.OnPlayerDead();
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
