using Cinemachine;
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
    public InputAction ragdollTest;

    [Header("PlayerMove")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private float rotateSpeed = 60f;
    [SerializeField] private float jumpForce = 20f;
    [Tooltip("LockOn ½Ã ÈÄ¹æÀ¸·Î ´Þ¸± ¼ö ÀÖ´Â °¢µµ")]
    [SerializeField] [Range(0f, 90f)] private float runBehindAngle = 50f;

    [Header("LockOnEnemy")]
    [SerializeField] private GameObject UI_lockOnPoint;
    [SerializeField] private GameObject VC_Default;
    [SerializeField] private GameObject VC_LockOn;
    [SerializeField] private CinemachineTargetGroup TargetGroup;
    [SerializeField] private float enemyDetectDistance = 60f;

    [SerializeField] private float lockOnLimitDistance = 80f;
    [Tooltip("½Ã¾ß°¢ÀÇ Àý¹Ý")]
    [SerializeField] [Range(10f, 90f)] private float enemyDetectAngle = 15f;
    [SerializeField] private float blendTime = 1f;

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

        CheckDead();

        CheckLockOnEnemyDistance();
        LookLockOnEnemy();
        ShowLockOnPoint();
        
        MovePlayer();
        AnimatePlayerMove();
    }
    private void CheckDead()
    {
        if (IsDead)
            return;

        if (_health.CurrentHP == 0)
        {
            IsDead = true;
            Die();
        }
    }
    private void CheckLockOnEnemyDistance()
    {
        // LockOn »óÅÂ¿¡¼­ Á¦ÇÑ¹üÀ§¸¦ ¹þ¾î³ª¸é UnLock
        if (IsLockOn && Vector3.Distance(transform.position, lockOnEnemy.transform.position) > lockOnLimitDistance)
        {
            UI_lockOnPoint.SetActive(false);
            ToggleTargetGroupCamera(false);
            IsLockOn = false;
        }
    }
    private void LookLockOnEnemy()
    {
        if (!IsLockOn)
            return;

        var lookAtPos = new Vector3(lockOnEnemy.transform.position.x, 0, lockOnEnemy.transform.position.z);
        transform.LookAt(lookAtPos);
    }
    private void ShowLockOnPoint()
    {
        if (!IsLockOn)
            return;

        // Highlight locked on target
        var pos = Camera.main.WorldToScreenPoint(lockOnEnemy.transform.position);
        var width = UI_lockOnPoint.GetComponent<RectTransform>().rect.width;
        var height = UI_lockOnPoint.GetComponent<RectTransform>().rect.height;
        UI_lockOnPoint.transform.position = new Vector3(pos.x - width / 2, pos.y + height / 2, pos.z);
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

        // LockOnÀÏ ¶§ ÈÄ¹æÀ¸·Î ÀÌµ¿ÇÏ¸é ´Þ¸± ¼ö ¾øÀ½
        if (IsLockOn && desiredMove.y < Mathf.Sin(Mathf.PI + runBehindAngle * Mathf.Deg2Rad))
            IsRun = false;

        float speed = IsRun ? runSpeed : walkSpeed;
        if (IsLockOn)
        {
            moveDirection = new Vector3(desiredMove.x, 0, desiredMove.y);
            transform.Translate(moveDirection * (speed * moveDirection.magnitude) * Time.deltaTime);
            Debug.DrawLine(transform.position, transform.position + moveDirection * speed, Color.green);
        }
        // FreeLookï¿½Ì¸ï¿½ desiredMoveï¿½ï¿½ moveDirectionï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
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

        // Rotate
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

        ragdollTest.performed += OnRagdollPerformed;
        ragdollTest.canceled += OnRagdollCanceled;
        ragdollTest.Enable();
        #endregion
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

        ragdollTest.performed -= OnRagdollPerformed;
        ragdollTest.canceled -= OnRagdollCanceled;
        ragdollTest.Disable();
        #endregion
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
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        var isRun = context.ReadValueAsButton();
        if (isRun)
            IsRun = !IsRun;
    }
    #endregion
    #region jump_Action
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        var isJump = context.ReadValueAsButton();
        if (isJump)
            _animator.SetBool(isJump_hash, true);
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        _animator.SetBool(isJump_hash, false);
    }
    #endregion
    #region slide_Action
    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        var isBlock = context.ReadValueAsButton();
        if (isBlock)
        {
            ControlState = ControlState.Uncontrollable;
            _animator.SetBool(isBlock_hash, true);
        }
    }
    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        _animator.SetBool(isBlock_hash, false);
        IsRun = false;
    }
    #endregion
    #region roll_Action
    private void OnRollPerformed(InputAction.CallbackContext context)
    {
        var isBlock = context.ReadValueAsButton();
        if (isBlock)
        {
            ControlState = ControlState.Uncontrollable;
            _animator.SetBool(isRoll_hash, true);
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
        var isNormalAttack = context.ReadValueAsButton();
        if (isNormalAttack)
        {
            _attackController.ChangeAttackType(AttackType.Weak);
            ControlState = ControlState.Uncontrollable;
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
        var isStrongAttack = context.ReadValueAsButton();
        if (isStrongAttack)
        {
            _attackController.ChangeAttackType(AttackType.Strong);
            ControlState = ControlState.Uncontrollable;
            _animator.SetTrigger(isStrongAttack_hash);
        }
    }
    private void OnStrongAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        _animator.SetBool(isStrongAttack_hash, false);
        IsRun = false;
    }
    #endregion
    private GameObject lockOnEnemy;
    #region lockOn_Action
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        var isLockOn = context.ReadValueAsButton();
        var isBlending = Camera.main.GetComponent<CinemachineBrain>().IsBlending;
        if (isLockOn && !isBlending && CheckEnemyInRange())
            IsLockOn = !IsLockOn;
    }

    private bool CheckEnemyInRange()
    {
        if (IsLockOn)
        {
            UI_lockOnPoint.SetActive(false);
            ToggleTargetGroupCamera(false);
            return true;
        }

        lockOnEnemy = null;

        float maxDotValue = float.MinValue;
        float minDistance = float.MaxValue;

        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, enemyDetectDistance, 1 << 8);
        foreach (var enemyCollider in enemyColliders)
        {
            var playerGroundPos = new Vector3(transform.position.x, 0, transform.position.z);
            var cameraGroundPos = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
            var enemyGroundPos = new Vector3(enemyCollider.transform.position.x, 0, enemyCollider.transform.position.z);

            // Ä«ï¿½Þ¶ï¿½ ï¿½Ã¾ß¿ï¿½ ï¿½Ö´ï¿½ï¿½ï¿½ È®ï¿½ï¿½
            Vector3 cameraToPlayer = playerGroundPos - cameraGroundPos;
            Vector3 cameraToEnemy = enemyGroundPos - cameraGroundPos;
            if (Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized) < 0    // ï¿½Ä¹ï¿½
                || Vector3.Angle(cameraToPlayer, cameraToEnemy) > enemyDetectAngle)     // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ê°ï¿½
                continue;

            // ï¿½Ã·ï¿½ï¿½Ì¾ï¿½ ï¿½ï¿½Ä¡ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿?ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ È®ï¿½ï¿½
            float distance = Vector3.Distance(playerGroundPos, enemyGroundPos);
            if (minDistance > distance)
            {
                minDistance = distance;
                lockOnEnemy = enemyCollider.gameObject;
            }
            
            // Ä«ï¿½Þ¶ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ß¾ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ È®ï¿½ï¿½
            float dot = Vector3.Dot(cameraToPlayer.normalized, cameraToEnemy.normalized);
            if (maxDotValue < dot)
            {
                maxDotValue = dot;
                lockOnEnemy = enemyCollider.gameObject;
                continue;
            }
        }

        UI_lockOnPoint.SetActive(lockOnEnemy != null);
        ToggleTargetGroupCamera(lockOnEnemy != null, lockOnEnemy);
        return lockOnEnemy != null;
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
            if (lockOnEnemy != null)
            {
                TargetGroup.RemoveMember(lockOnEnemy.transform);
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
        while (elapsedTime < blendTime)
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
        while (elapsedTime < blendTime)
        {
            elapsedTime += Time.deltaTime;

            var lastEnemyPos = lockOnEnemy.transform.position;
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
    #region ragdoll_Action
    private void OnRagdollPerformed(InputAction.CallbackContext context)
    {
        var isRagdoll = context.ReadValueAsButton();
        if (isRagdoll)
        {
            Die();
        }
    }

    public void Die()
    {
        IsLockOn = false;
        UI_lockOnPoint.SetActive(false);
        ToggleTargetGroupCamera(false);

        ToggleRagdoll(true);
        StartCoroutine(HandleEquipment(true));
    }

    private void OnRagdollCanceled(InputAction.CallbackContext context)
    {
        Revive();
    }

    public void Revive()
    {
        ToggleRagdoll(false);
        StartCoroutine(HandleEquipment(false));
        IsRun = false;
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
    private IEnumerator HandleEquipment(bool isDrop)
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
                lockOnEnemy.transform.position.x,
                0,
                lockOnEnemy.transform.position.z
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
