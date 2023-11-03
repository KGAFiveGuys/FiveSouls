using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Diagnostics;
using UnityEngine.AI;
public class MutantController : MonoBehaviour
{

    private Health health;

    [field: Header("State")]
    [field: SerializeField] public ControlState ControlState { get; set; } = ControlState.Controllable;

    [Header("Input Actions")]
    public InputAction move;
    public InputAction rotate;
    public InputAction weakAttack;
    public InputAction strongAttack;
    public InputAction run;
    public InputAction jump;
    public InputAction ragdollTest;
    public InputAction knockdown;
    public InputAction standing;

    [Header("Nav")]
    public LayerMask target_layer;
    private NavMeshAgent agent;

    [Header("Movements")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float RunSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float distanceToMove = 20f;

    private float distance;

    private bool isCursor = false;

    private AttackController attackController;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    #region AnimatorParameters
    private readonly int moveX_hash = Animator.StringToHash("moveX");
    private readonly int moveY_hash = Animator.StringToHash("moveY");
    private readonly int isWeakAttack_hash = Animator.StringToHash("isWeakAttack");
    private readonly int isStrongAttack_hash = Animator.StringToHash("isStrongAttack");
    private readonly int isJump_hash = Animator.StringToHash("isJump");
    private readonly int isRun_hash = Animator.StringToHash("isRun");
    private readonly int isKnockDown_hash = Animator.StringToHash("isKnockDown");
    private readonly int isStanding_hash = Animator.StringToHash("isStanding");
    private readonly int isHowling_hash = Animator.StringToHash("isHowling");

    //플레이어와 몬스터의 거리계산용
    private GameObject player;


    //쿨타임용 bool값 
    bool isDash = false;
    bool _isRun = false;
    bool isHammering = false;
    bool isDance = false;
    private bool isTarget
    {
        get
        {
            if (distance >= 30f || distance <= 20f)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
    }



    //스킬쿨타임 관리용
    float cool_Dash;
    #endregion
    #region Cached colliders & rigidbodies
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();
    #endregion

    private void Awake()
    {
        TryGetComponent(out playerRigidbody);
        TryGetComponent(out playerAnimator);
        player = GameObject.FindGameObjectWithTag("Player");
        TryGetComponent(out agent);
        TryGetComponent(out attackController);
        TryGetComponent(out health);
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            health = player.GetComponent<Health>();
            if (player != null)
            {
                // playerHealth 컴포넌트가 찾아진 경우,
                // 이곳에서 playerHealth를 사용하여 값을 가져올 수 있습니다.
                float currentHP = health.CurrentHP;
                print("Player의 현재 HP: " + currentHP);
            }
        }
    }

    private void Update()
    {
        print(distance);
        if(distance >= 3f)
        {
            playerAnimator.SetBool("isTarget", isTarget);
        }
        Move_ToPlayer();
        Move();
        Rotate();
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Togle_Cursor();
        }


        Jugement_MonAction();
        //transform.LookAt(player.transform);
        print("누구보고있니 : " + player.name);


//        Dance();
    }
    
    public void Togle_Cursor()
    {
        isCursor = !isCursor;
        Cursor.visible = isCursor;
    }
    private void Move()
    {
        if (ControlState.Equals(ControlState.Uncontrollable))
            return;

        if (!_isRun)
        {
            transform.Translate(
            desiredMove.x * moveSpeed * Time.deltaTime ,
            0,
            desiredMove.y * moveSpeed * Time.deltaTime
        );
        }
        else
        {

            transform.Translate(
            desiredMove.x * RunSpeed *Time.deltaTime,
            0,
            desiredMove.y * RunSpeed * Time.deltaTime
        );
        }
        
        
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

        run.performed += OnRunPerformed;
        run.canceled += OnRunCanceled;
        run.Enable();

        jump.performed += OnJumpPerformed;
        jump.canceled += OnJumpCanceled;
        jump.Enable();

        standing.performed += onStandingPerformed;
        standing.canceled += onStandingCanceled;
        standing.Enable();

        knockdown.performed += onisKnockDownPerformed;
        knockdown.canceled += onisKnockDownCancled;
        knockdown.Enable();



        //standing.performed +=

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

        weakAttack.Disable();
        weakAttack.performed -= OnWeakAttackPerformed;
        weakAttack.canceled -= OnWeakAttackCanceled;

        strongAttack.Disable();
        strongAttack.performed -= OnStrongAttackPerformed;
        strongAttack.canceled -= OnStrongAttackCanceled;

        run.Disable();
        run.performed -= OnRunPerformed;
        run.canceled -= OnRunCanceled;
        

        jump.performed -= OnJumpPerformed;
        jump.canceled -= OnJumpCanceled;
        jump.Disable();

        standing.performed -= onStandingPerformed;
        standing.canceled -= onStandingCanceled;
        standing.Disable();

        knockdown.Disable();
        knockdown.performed -= onisKnockDownPerformed;
        knockdown.canceled -= onisKnockDownCancled;


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
    //private void OnJumpPerformed(InputAction.CallbackContext context)
    //{
    //    var isJump = context.ReadValueAsButton();
    //    if (isJump)
    //        playerAnimator.SetBool(isJump_hash, true);
    //}

    private bool isJumping = false;

    public void OnJumpPerformed(InputAction.CallbackContext context)
    {
        var isJump = context.ReadValueAsButton();
        if (isJump && !isJumping)
        {
            playerAnimator.SetBool(isJump_hash, true);
            StartCoroutine(MoveForward());
        }
    }

    private IEnumerator MoveForward()
    {
        isJumping = true;

        //이부분 x값으로 변환시키는게 아니라 distance를 가져와서 써야함.
        //float distanceToMove = Skilljudgment.distance - 2f;
        float distanceToMove = distance - 2f;
        float distanceMoved = 0f;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (distanceMoved < distanceToMove)
        {
            float moveDistance = moveSpeed * 1.5f * Time.deltaTime;
            transform.Translate(Vector3.forward * moveDistance);
            distanceMoved += moveDistance;
            // 프레임 단위로 실행
            yield return null;
        }
        stopwatch.Stop();
        long time = stopwatch.ElapsedMilliseconds;
        print("대쉬 동작시간 : "+ time);
        isJumping = false;
        playerAnimator.SetBool(isJump_hash, false);

        // 대쉬 끝
        attackController.TurnOffAttackCollider();
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        playerAnimator.SetBool(isJump_hash, false);
    }
    #endregion
    #region Standingg_Action
    private void onStandingPerformed(InputAction.CallbackContext context)
    {
        var isStanding = context.ReadValueAsButton();
        if (isStanding)
        {
            playerAnimator.SetBool(isStanding_hash, true);
        }
    }

    private void onStandingCanceled(InputAction.CallbackContext context)
    {
        playerAnimator.SetBool(isStanding_hash, false);
    }
    #endregion

    #region KnockDown_Action
    private void onisKnockDownPerformed(InputAction.CallbackContext context)
    {
        var isKnockDown = context.ReadValueAsButton();
        if (isKnockDown)
        {
            playerAnimator.SetBool(isKnockDown_hash, true);
        }
    }

    private void onisKnockDownCancled(InputAction.CallbackContext context)
    {
        playerAnimator.SetBool(isKnockDown_hash, false);
    }
    #endregion

    #region Run_Action
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        
        var isRun = context.ReadValueAsButton();
        if (isRun)
        {
            _isRun = true;

            //ControlState = ControlState.Uncontrollable;
            playerAnimator.SetBool(isRun_hash, true);
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        _isRun = false;
        ControlState = ControlState.Controllable;
        playerAnimator.SetBool(isRun_hash, false);
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
            ToggleRagdoll(true);
            //StartCoroutine(HandleEquipment(true));
        }
    }
    private void OnRagdollCanceled(InputAction.CallbackContext context)
    {
        ToggleRagdoll(false);
        //StartCoroutine(HandleEquipment(false));
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
        playerAnimator.enabled = !isRagdoll;
        ControlState = !isRagdoll ? ControlState.Controllable : ControlState.Uncontrollable;
    }

    public float GetDistance()
    {
        float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
        return distance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 10f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 20f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 30f);
    }

    private IEnumerator Dash_Cool_co()
    {
        while (cool_Dash < 10f)
        {
            cool_Dash += Time.deltaTime;
            yield return null;
        }
        isDash = false;
        cool_Dash = 0;
    }



    private void Jugement_MonAction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1 << 9);

        distance = Vector3.Distance(player.transform.position, transform.position);



        //todo : 거리의 조건에 따라 공격의 패턴이 나오도록 switch 문을 이용해 랜덤으로 발생하게 할것// 
        //todo : 대쉬 공격은 쿨타임을 생성해서 2~30초에 한번만 하게 조정. => done
        //todo : 공격 판정 만드세요 ~~~~~~~~~~~~~~~~~~~
        //todo : 일반공격중 돌던지기 패턴 하나 만들것 => 일단 유도기능x 플레이어의 방향으로 던지는 방식. 
        //       addForce 방식 , velocity 채용 내일 둘다적용해보고 좀더 괜찮은거로  ㄱ (줍는 애니메이션 -> 던지는 애니매이션 필요..)!

        if (distance <= 30f && distance > 20f)
        {
            Dash_Att();
            print("far");
        }
        else if (distance <= 20f && distance > 10f)
        {
            // 대쉬공격 ,  돌던지기 (일반)으로 발동.
            print("middle");
        }
        else if (distance <= 10f)
        {
            // 캐릭터에게 걸어와서 근접공격 (일반 1 강공 1)
            if(distance <= 3f)
            {
                playerAnimator.SetBool(isWeakAttack_hash, true);
            }
            else
            {
                playerAnimator.SetBool(isWeakAttack_hash, false);
            }

            print("close");
        }
/*        else
        {
            if(Time.deltaTime > 30)
            Howling_Att();
        }*/

    }

    //대쉬 공격
    private void Dash_Att()
    {
        if (isDash)
        {
            return;
        }
        else if (!isDash)
        {
            playerAnimator.SetBool(isJump_hash, true);
            isDash = true;
            print(isDash);
            StartCoroutine(MoveForward());
            StartCoroutine(Dash_Cool_co());
        }
    }

    //강한공격 > 주먹질
    private void Hammering_Att()
    {
        if (isHammering)
        {
            return;
        }
        else if (!isHammering)
        {
            playerAnimator.SetBool(isStrongAttack_hash, true);
            isHammering = true;
        }
    }
    float count;
    //네비게이션을 이용하여 플레이어에게 이동
    private void Move_ToPlayer()
    {
        
        if (isDash)
        {

            while (count < 2f)
            {
                count += Time.deltaTime;
            }
            count = 0;
            return;
        }
        else
        {
            agent.SetDestination(player.transform.position);
        }

    }



    //춤추는건 나중에 하자;.
    //private void Dance()
    //{
    //    if (isDance)
    //    {
    //        return;
    //    }
    //    else if (!isDance && health.CurrentHP <= 0)
    //    {
    //        isDance = true;
    //        playerAnimator.SetBool("isDance", true);
    //    }
    //}
    //private void Howling_Att()
    //{
    //    if (isHowling)
    //    {
    //        return;
    //    }
    //    else if (!isHowling)
    //    {
    //        playerAnimator.SetBool(isHowling_hash, true);
    //    }
    //}


}
