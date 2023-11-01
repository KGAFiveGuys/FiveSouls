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
[RequireComponent(typeof(Animator))]
public class playerController : MonoBehaviour
{
    [field:Header("State")]
    [field:SerializeField] public ControlState ControlState { get; set; } = ControlState.Controllable;
    [field: SerializeField] public bool IsLockOn { get; set; } = false;
    [field: SerializeField] public bool IsRun { get; set; } = false;

    [Header("Input Actions")]
    public InputAction move;
    public InputAction rotate;
    public InputAction run;
    public InputAction weakAttack;
    public InputAction strongAttack;
    public InputAction block;
    public InputAction jump;
    public InputAction lockOn;
    public InputAction ragdollTest;

    [Header("Controls")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private float rotateSpeed = 60f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float enemyDetectDistance = 60f;
    [Tooltip("시야각의 절반")]
    [SerializeField] [Range(10f, 90f)] private float enemyDetectAngle = 15f;

    [Header("LockOnEnemy")]
    [SerializeField] GameObject UI_lockOnPoint;
    [SerializeField] GameObject VC_TargetGroup;
    [SerializeField] CinemachineTargetGroup TargetGroup;

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

    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCollider;
    private Animator playerAnimator;
    #region AnimatorParameters
    private readonly int moveX_hash = Animator.StringToHash("moveX");
    private readonly int moveY_hash = Animator.StringToHash("moveY");
    private readonly int isRun_hash = Animator.StringToHash("isRun");
    private readonly int isWeakAttack_hash = Animator.StringToHash("isWeakAttack");
    private readonly int isStrongAttack_hash = Animator.StringToHash("isStrongAttack");
    private readonly int isJump_hash = Animator.StringToHash("isJump");
    private readonly int isBlock_hash = Animator.StringToHash("isBlock");
    #endregion

    private void Awake()
    {
        TryGetComponent(out playerRigidbody);
        TryGetComponent(out playerCollider);
        TryGetComponent(out playerAnimator);

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
        LookLockOnEnemy();
        ShowLockOnPoint();
        Move();
        Rotate();
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
    private void Move()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;

        var speed = IsRun ? runSpeed : walkSpeed;
        transform.Translate(
            desiredMove.x * speed * Time.deltaTime,
            0,
            desiredMove.y * speed * Time.deltaTime
        );
    }
    private void Rotate()
    {
        if (ControlState.Equals(ControlState.Uncontrollable) || IsLockOn)
            return;

        transform.Rotate(Vector3.up * desiredRotate.x * rotateSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        Animate();
    }

    private void Animate()
    {
        // Move
        playerAnimator.SetFloat(moveX_hash, desiredMove.x);
        playerAnimator.SetFloat(moveY_hash, desiredMove.y);
        playerAnimator.SetBool(isRun_hash, IsRun);

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
            playerAnimator.SetBool(isJump_hash, true);
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        playerAnimator.SetBool(isJump_hash, false);
    }
    #endregion
    #region block_Action
    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        var isBlock = context.ReadValueAsButton();
        if (isBlock)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetBool(isBlock_hash, true);
        }
    }
    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        playerAnimator.SetBool(isBlock_hash, false);
    }
    #endregion
    #region weakAttack_Action
    private void OnWeakAttackPerformed(InputAction.CallbackContext context)
    {
        var isWeakAttack = context.ReadValueAsButton();
        if (isWeakAttack)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetBool(isWeakAttack_hash, true);
        }
    }
    private void OnWeakAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        playerAnimator.SetBool(isWeakAttack_hash, false);
    }
    #endregion
    #region strongAttack_Action
    private void OnStrongAttackPerformed(InputAction.CallbackContext context)
    {
        var isStrongAttack = context.ReadValueAsButton();
        if (isStrongAttack)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger(isStrongAttack_hash);
        }
    }
    private void OnStrongAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        playerAnimator.SetBool(isStrongAttack_hash, false);
    }
    #endregion
    private GameObject lockOnEnemy;
    #region lockOn_Action
    private void OnLockOnPerformed(InputAction.CallbackContext context)
    {
        var isLockOn = context.ReadValueAsButton();
        if (isLockOn && CheckEnemyInRange())
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
                lockOnEnemy = enemyCollider.gameObject;
            }
            
            // 카메라 시점 기준 가장 중앙의 적인지 확인
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

            VC_TargetGroup.SetActive(true);
        }
        else
        {
            if (lockOnEnemy != null)
                TargetGroup.RemoveMember(lockOnEnemy.transform);

            VC_TargetGroup.SetActive(false);
        }
    }

    #endregion
    #region ragdoll_Action
    private void OnRagdollPerformed(InputAction.CallbackContext context)
    {
        var isRagdoll = context.ReadValueAsButton();
        if (isRagdoll)
        {
            ToggleRagdoll(true);
            StartCoroutine(HandleEquipment(true));
        }
    }
    private void OnRagdollCanceled(InputAction.CallbackContext context)
    {
        ToggleRagdoll(false);
        StartCoroutine(HandleEquipment(false));
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
        playerAnimator.enabled = !isRagdoll;
        playerRigidbody.velocity = Vector3.zero;
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

        if (IsLockOn)
        {
            // Debug detect enemy line of sight
            //Debug.DrawLine(
            //    transform.position,
            //    new Vector3(lockOnEnemy.transform.position.x,
            //                0,
            //                lockOnEnemy.transform.position.z),
            //    Color.red
            //);
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
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(cameraGroundPos, cameraToPlayer.normalized * enemyDetectDistance);
        }
    }
}
