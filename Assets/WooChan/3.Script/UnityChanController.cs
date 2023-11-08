using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class UnityChanController : MonoBehaviour
{
    [field:Header("State")]
    [field:SerializeField] public ControlState ControlState { get; set; } = ControlState.Controllable;

    [Header("Input Actions")]
    public InputAction move;
    public InputAction rotate;
    public InputAction weakAttack;
    public InputAction strongAttack;
    public InputAction block;
    public InputAction jump;
    public InputAction ragdollTest;
    public InputAction Slide;
    public InputAction Jumping;
    public InputAction Corkscrew;
    public InputAction DropKick;
    public InputAction Kick;
    public InputAction Sweep;
    public InputAction Win;

    [Header("Movements")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float jumpForce;


    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    #region AnimatorParameters
    private readonly int moveX_hash = Animator.StringToHash("moveX");
    private readonly int moveY_hash = Animator.StringToHash("moveY");
    private readonly int isWeakAttack_hash = Animator.StringToHash("isWeakAttack");
    private readonly int isStrongAttack_hash = Animator.StringToHash("isStrongAttack");
    private readonly int isJump_hash = Animator.StringToHash("isJump");
    private readonly int isBlock_hash = Animator.StringToHash("isBlock");

    #endregion
    #region Cached colliders & rigidbodies
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();
    #endregion

    private void Awake()
    {
        TryGetComponent(out playerRigidbody);
        TryGetComponent(out playerAnimator);
    }

    private void Start()
    {

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
        playerAnimator.SetFloat(moveX_hash, desiredMove.x);
        playerAnimator.SetFloat(moveY_hash, desiredMove.y);
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

        ragdollTest.performed += OnRagdollPerformed;
        ragdollTest.canceled += OnRagdollCanceled;
        ragdollTest.Enable();

        Slide.performed += OnSlidePerformed;
        Slide.canceled += OnSlideCanceled;
        Slide.Enable();

        Jumping.performed += OnJumping;
        Jumping.canceled += OnJumpingCanceled;
        Jumping.Enable();

        Corkscrew.performed += OnCorkscrew;
        Corkscrew.canceled += OnCorkscrewCanceled;
        Corkscrew.Enable();

        DropKick.performed += OnDropKick;
        DropKick.canceled += OnDropKickCanceled;
        DropKick.Enable();

        Kick.performed += OnKick;
        Kick.canceled += OnKickCanceled;
        Kick.Enable();

        Sweep.performed += OnSweep;
        Sweep.canceled += OnSweepCanceled;
        Sweep.Enable();
        
        Win.performed += OnWin;
        Win.canceled += OnWinCanceled;
        Win.Enable();


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

        ragdollTest.performed -= OnRagdollPerformed;
        ragdollTest.canceled -= OnRagdollCanceled;
        ragdollTest.Disable();

        Slide.performed -= OnSlidePerformed;
        Slide.canceled -= OnSlideCanceled;
        Slide.Disable();

        Jumping.performed -= OnJumping;
        Jumping.canceled -= OnJumpingCanceled;
        Jumping.Disable();

        Corkscrew.performed -= OnCorkscrew;
        Corkscrew.canceled -= OnCorkscrewCanceled;
        Corkscrew.Disable();

        DropKick.performed -= OnDropKick;
        DropKick.canceled -= OnDropKickCanceled;
        DropKick.Disable();

        Kick.performed -= OnKick;
        Kick.canceled -= OnKickCanceled;
        Kick.Disable();

        Sweep.performed -= OnSweep;
        Sweep.canceled -= OnSweepCanceled;
        Sweep.Disable();
        
        Win.performed -= OnWin;
        Win.canceled -= OnWinCanceled;
        Win.Disable();

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
    #region ragdoll_Action
    private void OnRagdollPerformed(InputAction.CallbackContext context)
    {
        var isRagdoll = context.ReadValueAsButton();
        if (isRagdoll)
        {
            ////ToggleRagdoll(true);
            //StartCoroutine(HandleEquipment(true));
        }
        playerAnimator.SetTrigger("isDie");

    }
    private void OnRagdollCanceled(InputAction.CallbackContext context)
    {
        ////ToggleRagdoll(false);
        //StartCoroutine(HandleEquipment(false));

    }
    #endregion

    private void OnSlidePerformed(InputAction.CallbackContext context)
    {
        var isSlide = context.ReadValueAsButton();
        if (isSlide)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("isSlide");
        }
    }
    private void OnSlideCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }

    private void OnJumping(InputAction.CallbackContext context)
    {
        var isJumping = context.ReadValueAsButton();
        if (isJumping)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("Jumping");
        }
    }
    private void OnJumpingCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }

    private void OnCorkscrew(InputAction.CallbackContext context)
    {
        var isCorkscrew = context.ReadValueAsButton();
        if (isCorkscrew)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("Corkscrew");
        }

    }

    private void OnCorkscrewCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }

    private void OnDropKick(InputAction.CallbackContext context)
    {
        var isDropKick = context.ReadValueAsButton();
        if (isDropKick)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("DropKick");
        }
    }
    private void OnDropKickCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }

    private void OnKick(InputAction.CallbackContext context)
    {
        var isKick = context.ReadValueAsButton();
        if (isKick)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("Kick");
        }

    }
    private void OnKickCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }

    private void OnSweep(InputAction.CallbackContext context)
    {
        var isSweep = context.ReadValueAsButton();
        if (isSweep)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("Sweep");
        }

    }
    private void OnSweepCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }

    private void OnWin(InputAction.CallbackContext context)
    {
        var isWin = context.ReadValueAsButton();
        if (isWin)
        {
            ControlState = ControlState.Uncontrollable;
            playerAnimator.SetTrigger("Win");
        }

    }
    private void OnWinCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
    }



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
        playerAnimator.enabled = !isRagdoll;
        ControlState = !isRagdoll ? ControlState.Controllable : ControlState.Uncontrollable;
    }

}
