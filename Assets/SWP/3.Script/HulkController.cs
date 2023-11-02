using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Animator))]
public class HulkController : MonoBehaviour
{
    [field:Header("State")]
    [field:SerializeField] public ControlState ControlState { get; set; } = ControlState.Controllable;

    [Header("Input Actions")]
    public InputAction move;
    public InputAction rotate;
    public InputAction normalAttack;
    public InputAction strongAttack;
    public InputAction slide;
    public InputAction jump;
    public InputAction ragdollTest;

    [Header("Movements")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float jumpForce;

    private Rigidbody HulkRigidbody;
    private Animator HulkAnimator;
    #region AnimatorParameters
    private readonly int moveX_hash = Animator.StringToHash("moveX");
    private readonly int moveY_hash = Animator.StringToHash("moveY");
    private readonly int isNormalAttack_hash = Animator.StringToHash("isNormalAttack");
    private readonly int NormalNum_hash = Animator.StringToHash("NormalNum");
    private readonly int isStrongAttack_hash = Animator.StringToHash("isStrongAttack");
    private readonly int StrongNum_hash = Animator.StringToHash("StrongNum");
    private readonly int isJump_hash = Animator.StringToHash("isJump");
    private readonly int isSlide_hash = Animator.StringToHash("isSlide");
    #endregion
    #region Cached colliders & rigidbodies
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();
    #endregion

    private void Awake()
    {
        TryGetComponent(out HulkRigidbody);
        TryGetComponent(out HulkAnimator);
    }

    private void Update()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;
        
        transform.Translate(
            desiredMove.x * moveSpeed * Time.deltaTime,
            0,
            desiredMove.y * moveSpeed * Time.deltaTime
        );
        HulkAnimator.SetFloat(moveX_hash, desiredMove.x);
        HulkAnimator.SetFloat(moveY_hash, desiredMove.y);
    }

    private void Rotate()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;

        transform.Rotate(Vector3.up * desiredRotate.x * rotateSpeed * Time.deltaTime);
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

        normalAttack.performed += OnNormalAttackPerformed;
        normalAttack.canceled += OnNormalAttackCanceled;
        normalAttack.Enable();

        strongAttack.performed += OnStrongAttackPerformed;
        strongAttack.canceled += OnStrongAttackCanceled;
        strongAttack.Enable();

        slide.performed += OnSlidePerformed;
        slide.canceled += OnSlideCanceled;
        slide.Enable();

        jump.performed += OnJumpPerformed;
        jump.canceled += OnJumpCanceled;
        jump.Enable();

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

        normalAttack.Disable();
        normalAttack.performed -= OnNormalAttackPerformed;
        normalAttack.canceled -= OnNormalAttackCanceled;

        strongAttack.Disable();
        strongAttack.performed -= OnStrongAttackPerformed;
        strongAttack.canceled -= OnStrongAttackCanceled;

        slide.performed -= OnSlidePerformed;
        slide.canceled -= OnSlideCanceled;
        slide.Disable();

        jump.performed -= OnJumpPerformed;
        jump.canceled -= OnJumpCanceled;
        jump.Disable();

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
    #region jump_Action
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        var isJump = context.ReadValueAsButton();
        if (isJump)
            HulkAnimator.SetBool(isJump_hash, true);
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        HulkAnimator.SetBool(isJump_hash, false);
    }
    #endregion
    #region slide_Action
    private void OnSlidePerformed(InputAction.CallbackContext context)
    {
        var isSlide = context.ReadValueAsButton();
        if (isSlide)
        {
            ControlState = ControlState.Uncontrollable;
            HulkAnimator.SetBool(isSlide_hash, true);
        }
    }
    private void OnSlideCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        HulkAnimator.SetBool(isSlide_hash, false);
    }
    #endregion
    #region nomarlAttack_Action
    private void OnNormalAttackPerformed(InputAction.CallbackContext context)
    {
        var isNormalAttack = context.ReadValueAsButton();
        if (isNormalAttack)
        {
            ControlState = ControlState.Uncontrollable;
            HulkAnimator.SetBool(isNormalAttack_hash, true);
            HulkAnimator.SetInteger(NormalNum_hash, Random.Range(0,2));
        }
    }
    private void OnNormalAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        HulkAnimator.SetBool(isNormalAttack_hash, false);
    }
    #endregion
    #region strongAttack_Action
    private void OnStrongAttackPerformed(InputAction.CallbackContext context)
    {
        var isStrongAttack = context.ReadValueAsButton();
        if (isStrongAttack)
        {
            ControlState = ControlState.Uncontrollable;
            HulkAnimator.SetBool(isStrongAttack_hash, true);
            HulkAnimator.SetInteger(StrongNum_hash, Random.Range(0, 2));
        }
    }
    private void OnStrongAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        HulkAnimator.SetBool(isStrongAttack_hash, false);
    }
    #endregion
    #region ragdoll_Action
    private void OnRagdollPerformed(InputAction.CallbackContext context)
    {
        var isRagdoll = context.ReadValueAsButton();
        if (isRagdoll)
        {
            ToggleRagdoll(true);
        }
    }
    private void OnRagdollCanceled(InputAction.CallbackContext context)
    {
        ToggleRagdoll(false);
    }
    #endregion

    public void ToggleRagdoll(bool isRagdoll)
    {
        #region Toggle colliders & rigidbodies
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
        HulkAnimator.enabled = !isRagdoll;
        ControlState = !isRagdoll ? ControlState.Controllable : ControlState.Uncontrollable;
    }
    
}
