using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Diagnostics;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(AttackController))]
public class MutantController : MonoBehaviour
{
    public GameObject rockPrefab; // 돌 프리팹
    public Transform handPosition; // 손 위치
    public Transform throwPosition; // 던질 위치
    public float throwForce = 10.0f; // 던질 힘
    public float addForceDuration = .5f; // 던지는 힘을 누적할 시간

    private GameObject currentRock;
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

    [Header("CoolTime")]
    [SerializeField] private float Dash_cool;
    [SerializeField] private float Swing_cool;
    [SerializeField] private float Smash_cool;
    [SerializeField] private float Rock_cool;

    [Header("AttackCollider")]
    [SerializeField] private Collider dashAttackCollider;
    [SerializeField] private Collider smashAttackCollider;
    [SerializeField] private Collider swingAttackCollider;
    [SerializeField] private Collider throwRockAttackCollider;

    //스킬쿨타임 관리용
    [Header("쿨돌아가나확인용")]
    public float cool_Dash;
    public float cool_Swing;
    public float cool_Smash;
    public float cool_Rock;

    //쿨타임용 bool값 
    bool isDash = false;
    bool isSmash = false;
    bool isSwing = false;
    bool _isRun = false;
    bool isRock = false;

    //거리계산
    private float distance;
    //마우스 커서 가리기
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
    
    //========================================================================
    private readonly int isKnockDown_hash = Animator.StringToHash("isKnockDown");
    private readonly int isStanding_hash = Animator.StringToHash("isStanding");
    private readonly int isHowling_hash = Animator.StringToHash("isHowling");
    private readonly int isSwing_hash = Animator.StringToHash("Swing");
    private readonly int isSmash_hash = Animator.StringToHash("Smash");

    //플레이어와 몬스터의 거리계산용
    private GameObject player;



    public bool isTarget
    {
        get
        {
            if (distance >= 30f || (distance <= 5f && distance >=2.3f))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }



    #endregion
    #region Cached colliders & rigidbodies
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();
    #endregion

    private void Awake()
    {
        StartCoroutine(Swing_Cool_co());
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
                float currentHP = health.CurrentHP;
                print("Player의 현재 HP: " + currentHP);
            }
        }
    }
    private void Update()
    {
        player.transform.position = player.transform.position;
        print("isTarget : " + isTarget);
        Move_ToPlayer();
        Move();
        Rotate(); 
        Jugement_MonAction();
        if(distance >= 0.5f)
        {
            playerAnimator.SetBool("isTarget", isTarget);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Togle_Cursor();
        }
        print("isSwing : " + isSwing);
        
        print("누구보고있니 : " + player.name);
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
        while (cool_Dash < Dash_cool)
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
        if (distance <= 30f && distance > 20f)
        {
            //강
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = dashAttackCollider;
            Dash_Att();
            print("far");
        }
        else if (distance <= 20f && distance > 10f)
        {
            if (!isDash)
            {
                transform.LookAt(player.transform);
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }
            print("middle");
        }
        else if (distance <= 10f)
        {
            if (cool_Swing == 0 || cool_Smash == 0)
            {
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        if(cool_Swing == 0)
                        {
                            //약
                            attackController.ChangeAttackType(AttackType.Weak);
                            attackController.AttackCollider = swingAttackCollider;
                            Swing_att();
                        }
                        else
                        {
                            return;
                        }
                        
                        break;
                    case 1:
                        if(cool_Smash == 0)
                        {
                            //강
                            attackController.ChangeAttackType(AttackType.Strong);
                            attackController.AttackCollider = smashAttackCollider;
                            Smash_Att();
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        print("둘다쿨타임");
                        break;
                }
            }
            print("close");
        }
    }

    //네비게이션을 이용하여 플레이어에게 이동
    private void Move_ToPlayer()
    {

        if (isDash)
        {
            while (count < 5f)
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

    //그로기상태 후 하울링 공격 > 광역기

    private IEnumerator Swing_Cool_co()
    {
        while (cool_Swing < Swing_cool)
        {
            cool_Swing += Time.deltaTime;
            yield return null;
        }
        isSwing = false;
        cool_Swing = 0;
    }
    private void Swing_att()
    {
        if(cool_Swing != 0)
        {
            isSwing = false;
        }
        isSwing = true;
        //playerAnimator.SetBool(isSwing_hash, isSwing);
        playerAnimator.SetTrigger("Swing");
        StartCoroutine(Swing_Cool_co());
    }
   

    //대쉬 공격 (강공1)
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

    //강한공격 2.주먹질
    private IEnumerator Smash_Cool_co()
    {
        while(cool_Smash < Smash_cool)
        {
            cool_Smash += Time.deltaTime;
            yield return null;
        }
        isSmash = false;
        cool_Smash = 0;
    }
    private void Smash_Att()
    {
        if (cool_Smash != 0)
        {
            isSmash = false;
        }
        isSmash = true;
        playerAnimator.SetTrigger("Smash");
        StartCoroutine(Smash_Cool_co());
        //playerAnimator.SetBool(isStrongAttack_hash, true);
    }


    float count;


    //돌주워 던지기 약한공격 2.
    private IEnumerator Rock_Cool_co()
    {
        while (cool_Rock < Rock_cool)
        {
            cool_Rock += Time.deltaTime;
            yield return null;
        }
        isRock = false;
        cool_Rock = 0;

    }
    private void PickUpRock()
    {
        if(cool_Rock == 0)
        {
            if (!isDash && currentRock == null)
            {
                // 돌을 생성하고 손 위치에 놓기
                Vector3 offset = new Vector3(1f, -1f, -.91f);
                GameObject newRock = Instantiate(rockPrefab, handPosition.position+offset, Quaternion.identity);                
                newRock.transform.parent = handPosition; // 돌을 손 아래로 이동

                AnimationClip throwAnimation = playerAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Throw_Rock");
                float waitTime = throwAnimation.length;

                attackController.ChangeAttackType(AttackType.Weak);
                attackController.AttackCollider = throwRockAttackCollider;
                // 일정 시간 대기 후 돌을 부모에서 분리
                StartCoroutine(DetachRockAfterTime(newRock, waitTime));

                // 쿨다운 코루틴 시작
                StartCoroutine(Rock_Cool_co());
            }
        }
    }

    private IEnumerator DetachRockAfterTime(GameObject rock, float time)
    {
        yield return new WaitForSeconds(time);
        // 돌을 부모에서 분리
        rock.transform.parent = null;
        // 돌의 Rigidbody 컴포넌트 가져오기
        Rigidbody rb = rock.GetComponent<Rigidbody>();
        
        if (rb == null) yield break;

        rb.isKinematic = false; // isKinematic을 비활성화
        rb.useGravity = true;   // 중력 사용을 활성화
        
        // 플레이어의 방향으로 던지기
        // 플레이어의 전방 방향
        var handPos = handPosition.position;
        var playerGroundPos = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        Vector3 throwDirection = playerGroundPos - handPos;

        //rb.AddForce(throwDirection * throwForce, ForceMode.Acceleration);

        float elapsedTime = 0f;
        while (elapsedTime < addForceDuration)
        {
            elapsedTime += Time.deltaTime;

            UnityEngine.Debug.DrawLine(handPos, handPos + throwDirection * 100f, Color.magenta);

            var force = throwDirection * Mathf.Lerp(throwForce, 0, (elapsedTime / addForceDuration));
            rb.AddForce(force, ForceMode.Acceleration);

            yield return null;
        }

        //Vector3 throwDirection = (player.transform.position - transform.position).normalized;
        //Vector3 throwVelocity = throwDirection * throwForce * Time.deltaTime;
        //transform.Translate(throwVelocity, Space.World);


    }
    //주운돌 던지기.


}