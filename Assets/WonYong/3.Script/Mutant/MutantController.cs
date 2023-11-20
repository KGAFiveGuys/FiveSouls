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
    [SerializeField] private float Rock_dmg;
    [SerializeField] private float Smash_dmg;
    [SerializeField] private float Swing_dmg;
    [SerializeField] private float Dash_dmg;
    [SerializeField] private float Howing_dmg;
    //����Ʈ ����� ���� ī�޶�
    [Header("���� ����� ���� door")]
    [SerializeField] private Animator door;
    [SerializeField] private GameObject On_Clear_DollyCamera_;
    //����Ʈ �Ҹ�
    [Header("Sound")]
    [SerializeField] private SoundEffectSO Dash_sound;

    //����Ʈ ������ üũ��
    private GameObject Mutant;
    //�Ͽ︵�� ��� ��ƼŬ ������
    [SerializeField] private GameObject Particale_Position;
    // ����Ÿ�� �˶� ��ƼŬ ���� ��ġ.
    [SerializeField] private GameObject WeakParticle;
    [SerializeField] private GameObject StrongParticle;
    
    //=============
    [SerializeField] private GameObject rockPrefab; // �� ������
    [SerializeField] private Transform handPosition; // �� ��ġ
    [SerializeField] private Transform throwPosition; // ���� ��ġ
    [SerializeField] private float throwForce = 10.0f; // ���� ��
    [SerializeField] private float addForceDuration = .5f; // ������ ���� ������ �ð�



    private GameObject currentRock;
    
    //
    private Health health_p;

    //
    private Health health_m;

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

    [Header("CoolTime")]
    [SerializeField] private float Dash_cool;
    [SerializeField] private float Swing_cool;
    [SerializeField] private float Smash_cool;
    [SerializeField] private float Rock_cool;
    [SerializeField] private float Howing_cool;

    [Header("AttackCollider")]
    [SerializeField] private Collider dashAttackCollider;
    [SerializeField] private Collider smashAttackCollider;
    [SerializeField] private Collider swingAttackCollider;
    [SerializeField] private Collider howlingAttackCollider;

    [Header("Howling Time")]
    [SerializeField] 

    //��ų��Ÿ�� ������
    [Header("�𵹾ư���Ȯ�ο�")]
    public float cool_Dash;
    public float cool_Swing;
    public float cool_Smash;
    public float cool_Rock;
    public float cool_Howling;
    
    //��Ÿ�ӿ� bool�� 
    bool isDash = false;
    bool isSmash = false;
    bool isSwing = false;
    bool _isRun = false;
    bool isRock = false;
    bool isHowling = false;
    //�ൿüũ
    bool isDance = false;
    bool isGroggy = false;
    bool isDie = false;

    //��ü �ൿ üũ
    bool isAction = false;

    //�Ÿ����
    private float distance;
    //���콺 Ŀ�� ������
    private bool isCursor = false;
    //Ÿ�̸�
    private float time = 0f;

    private AttackController attackController;
    private Rigidbody mutantRigidbody;
    private Animator mutantAnimator;
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

    //�÷��̾�� ������ �Ÿ�����
    private GameObject player;



    public bool isTarget
    {
        get
        {
            if (distance >= 30f && distance <=50f)
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
        TryGetComponent(out Mutant);
        StartCoroutine(Swing_Cool_co());
        TryGetComponent(out mutantRigidbody);
        TryGetComponent(out mutantAnimator);
        player = GameObject.FindGameObjectWithTag("Player");
        TryGetComponent(out agent);
        TryGetComponent(out attackController);
        TryGetComponent(out health_p);
        TryGetComponent(out health_m);
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            health_p = player.GetComponent<Health>();
            if (player != null)
            {
                float currentHP = health_p.CurrentHP;
            }
        }
    }

    private void Update()
    {        
        if(health_m.CurrentHP <= 0)
        {
            agent.enabled = false;
        }
        Timer();
        Dance();
        Groggy();
        Die();
        Howilng_att();
        //distance ����
        player.transform.position = player.transform.position;
        Move_ToPlayer();
        Rotate(); 
        if(distance >= 2.5f)
        {
            mutantAnimator.SetBool("isTarget", isTarget);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Togle_Cursor();
        }
        if(health_m.CurrentHP >= 0)
        {
            Judgement_MonAction();
        }


    }
    private void Timer()
    {
        if (distance > 30f && distance < 100f)
        {
            time += Time.deltaTime;
        }
        else
        {
            time = 0;
        }
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
        
        
        mutantAnimator.SetFloat(moveX_hash, desiredMove.x);
        mutantAnimator.SetFloat(moveY_hash, desiredMove.y);
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
        health_m.OnDead += OpenDoor;
    }

    private  void OpenDoor()
    {
        door.enabled = true;

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
        health_m.OnDead -= OpenDoor;
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

    private bool isJumping = false;

    public void OnJumpPerformed(InputAction.CallbackContext context)
    {
        var isJump = context.ReadValueAsButton();
        if (isJump && !isJumping)
        {
            mutantAnimator.SetBool(isJump_hash, true);
            StartCoroutine(MoveForward());
        }
    }

    private IEnumerator MoveForward()
    {
        isJumping = true;
        isAction = true;
        //�̺κ� x������ ��ȯ��Ű�°� �ƴ϶� distance�� �����ͼ� �����.
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
            // ������ ������ ����
            yield return null;
        }
        stopwatch.Stop();
        long time = stopwatch.ElapsedMilliseconds;
        isJumping = false;
        mutantAnimator.SetBool(isJump_hash, false);

        // �뽬 ��
        attackController.TurnOffAttackCollider();
        isAction = false;

    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        mutantAnimator.SetBool(isJump_hash, false);
    }
    #region Standingg_Action
    private void onStandingPerformed(InputAction.CallbackContext context)
    {
        var isStanding = context.ReadValueAsButton();
        if (isStanding)
        {
            mutantAnimator.SetBool(isStanding_hash, true);
        }
    }

    private void onStandingCanceled(InputAction.CallbackContext context)
    {
        mutantAnimator.SetBool(isStanding_hash, false);
    }
    #endregion

    #region KnockDown_Action
    private void onisKnockDownPerformed(InputAction.CallbackContext context)
    {
        var isKnockDown = context.ReadValueAsButton();
        if (isKnockDown)
        {
            mutantAnimator.SetBool(isKnockDown_hash, true);
        }
    }

    private void onisKnockDownCancled(InputAction.CallbackContext context)
    {
        mutantAnimator.SetBool(isKnockDown_hash, false);
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
            mutantAnimator.SetBool(isRun_hash, true);
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        _isRun = false;
        ControlState = ControlState.Controllable;
        mutantAnimator.SetBool(isRun_hash, false);
    }
    #endregion
    #region weakAttack_Action
    private void OnWeakAttackPerformed(InputAction.CallbackContext context)
    {
        var isWeakAttack = context.ReadValueAsButton();
        if (isWeakAttack)
        {
            ControlState = ControlState.Uncontrollable;
            mutantAnimator.SetBool(isWeakAttack_hash, true);
        }
    }
    private void OnWeakAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        mutantAnimator.SetBool(isWeakAttack_hash, false);
    }
    #endregion
    #region strongAttack_Action
    private void OnStrongAttackPerformed(InputAction.CallbackContext context)
    {
        var isStrongAttack = context.ReadValueAsButton();
        if (isStrongAttack)
        {
            ControlState = ControlState.Uncontrollable;
            mutantAnimator.SetTrigger(isStrongAttack_hash);
        }
    }
    private void OnStrongAttackCanceled(InputAction.CallbackContext context)
    {
        ControlState = ControlState.Controllable;
        mutantAnimator.SetBool(isStrongAttack_hash, false);
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
        mutantAnimator.enabled = !isRagdoll;
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 50f);
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

  
    private void Judgement_MonAction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1 << 9);

        distance = Vector3.Distance(player.transform.position, transform.position);
        if(distance > 50f)
        {
            agent.enabled = false;
        }

        if(distance > 30f && distance <=50f)
        {
            if(isAction)
            {
                agent.enabled = false;
            }
            else if(!isAction)
            {
                agent.enabled = true;
            }
        }
        else if (distance <= 30f && distance > 20f)
        {
            if(!isAction)
            {
                agent.enabled = true;
                attackController.ChangeAttackType(AttackType.Strong);
                attackController.AttackCollider = dashAttackCollider;
                attackController.StrongAttackBaseDamage = Dash_dmg;
                AttackAlarm.Instance.RedAlarm();
                if (!isDie)
                {
                   //SFXManager.Instance.PlayWhole(Dash_sound);
                    Dash_Att();
                }

            }
        }
        else if (distance <= 20f && distance > 10f)
        {
            if (!isDash )
            {
                transform.LookAt(player.transform);
                if (!isAction)
                {
                    ThrowRock_anim();   
                }
            }
        }
        else if (distance <= 10f && distance >= 5f)
        {
            if (cool_Swing == 0 || cool_Smash == 0)
            {
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        if(cool_Swing == 0 && !isAction)
                        {
                            //��
                            attackController.ChangeAttackType(AttackType.Weak);
                            attackController.AttackCollider = swingAttackCollider;
                            attackController.WeakAttackBaseDamage = Swing_dmg;
                            AttackAlarm.Instance.YellowAlarm();
                            StartCoroutine(Swing_att());
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case 1:
                        if(cool_Smash == 0 && !isAction)
                        {
                            //��
                            attackController.ChangeAttackType(AttackType.Strong);
                            attackController.AttackCollider = smashAttackCollider;
                            attackController.StrongAttackBaseDamage = Smash_dmg;
                            AttackAlarm.Instance.RedAlarm();
                            StartCoroutine(Smash_Att());
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        else if(distance < 5f)
        {
            transform.LookAt(player.transform);
        }
    }

    //�׺���̼��� �̿��Ͽ� �÷��̾�� �̵�
    private void Move_ToPlayer()
    {
        if(agent.enabled)
        {
            agent.SetDestination(player.transform.position);
        }
    }


    //���Ž� > ������ �ݶ��̴� �÷��� ����
    
    private IEnumerator Up_Smash_Strong_coll()
    {
        AnimationClip SmashAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Smash_Strong");
        Collider attackCollider = attackController.AttackCollider;

        //attackController.transform.position = new Vector3()

        float Smashtime = SmashAnimation.length - 1;
        float elapsedTime = 0f;
        
        var startPos = attackCollider.transform.position;
        var endPos = new Vector3
        (
            attackCollider.transform.position.x,
            attackCollider.transform.position.y - 20f,
            attackCollider.transform.position.z
        );

        while (elapsedTime < Smashtime)
        {
            elapsedTime += Time.deltaTime;
            attackCollider.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / Smashtime)*2f);
            yield return null;
        }
        attackCollider.transform.position = startPos;
        attackController.TurnOffAttackCollider();
    }



    //���� �����
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
    //�Ͽ︵(���)
    private void Howilng_att()
    {
        if (time >= Howing_cool && !isHowling && !isDie)
        {
            isHowling = true;
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = howlingAttackCollider;
            attackController.StrongAttackBaseDamage = Howing_dmg;
            mutantAnimator.SetTrigger("Howling");
            Particale_Position.SetActive(true);
        }
        
    }


    private IEnumerator Swing_att()
    {
        AnimationClip SwingAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Swing_Weak");
        agent.enabled = false;
        isAction = true;
        if (cool_Swing != 0)
        {
            isSwing = false;
        }
        isSwing = true;
        mutantAnimator.SetTrigger("Swing");
        StartCoroutine(Swing_Cool_co());
        yield return new WaitForSeconds(SwingAnimation.length);
        agent.enabled = true;
        isAction = false;
        //mutantAnimator.SetBool(isSwing_hash, isSwing);

    }

    private IEnumerator Smash_Att()
    {
        AnimationClip SmashAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Smash_Strong");
        agent.enabled = false;
        isAction = true;
        if (cool_Smash != 0)
        {
            isSmash = false;
        }
        isSmash = true;
        mutantAnimator.SetTrigger("Smash");
        StartCoroutine(Up_Smash_Strong_coll());
        StartCoroutine(Smash_Cool_co());
        yield return new WaitForSeconds(SmashAnimation.length);
        agent.enabled = true;
        isAction = false;
    }

    //���� ������ ���or ���
    private IEnumerator Smash_Action_Check_co()
    {
        if (!isAction)
        {
            isAction = true;
            agent.enabled = false;
            AnimationClip SmashAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Smash_Strong");
            float SmashAnimationTime = SmashAnimation.length;
            while (SmashAnimationTime < 0)
            {

                SmashAnimationTime -= Time.deltaTime;
                yield return null;
            }
            isAction = false;
            agent.enabled = true;
        }

  

    }

    private IEnumerator Throw_Action_Check_co()
    {
        isAction = true;
        agent.enabled = false;
        AnimationClip PickAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Get_Rock");
        AnimationClip ThrowAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Throw_Rock");
        float Rock_Time = PickAnimation.length + ThrowAnimation.length;
        while (Rock_Time < 0)
        {
            Rock_Time -= Time.deltaTime;
            yield return null;
        }
        isAction = false;
        agent.enabled = true;
    }

    private IEnumerator Swing_Action_Check_co()
    {
        isAction = true;
        agent.enabled =false;
        AnimationClip SwingAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Swing_Weak");
        float SwingAnimationTime = SwingAnimation.length;

        while (SwingAnimationTime < 0)
        {
            SwingAnimationTime -= Time.deltaTime;
            yield return null;
        }
        isAction = false;
        agent.enabled = true;
    }




//�뽬 ����
private void Dash_Att()
    {
        if (isDash)
        {
            return;
        }
        else if (!isDash)
        {
            mutantAnimator.SetBool(isJump_hash, true);
            isDash = true;
            StartCoroutine(MoveForward());
            StartCoroutine(Dash_Cool_co());
            
        }
    }

    //���Ž� �����
    private IEnumerator Smash_Cool_co()
    {
        while (cool_Smash < Smash_cool)
        {
            cool_Smash += Time.deltaTime;
            yield return null;
        }
        isSmash = false;
        cool_Smash = 0;
    }

    //���Ž� 



    //�������� �ִϸ��̼� ���
    private void ThrowRock_anim()
    {
            mutantAnimator.SetTrigger("ThrowRock");
    }

    //�����
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

    //������
    private void PickUpRock()
    {
        agent.enabled = false;
        AttackAlarm.Instance.YellowAlarm();
        if (cool_Rock == 0)
        {
            isRock = true;
            
            if (!isDash && currentRock == null)
            {
                // ���� �����ϰ� �� ��ġ�� ����
                isAction = true;
                Vector3 offset = new Vector3(-1.35f, -0.21f, -0.56f); //��������
                GameObject newRock = Instantiate(rockPrefab, handPosition.position, Quaternion.identity);                
                newRock.transform.parent = handPosition; // ���� �� �Ʒ��� �̵�

                AnimationClip throwAnimation = mutantAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == "Throw_Rock");
                float waitTime = throwAnimation.length * (1 / 1.5f);
                StartCoroutine(DetachRockAfterTime(newRock, waitTime));
                StartCoroutine(Rock_Cool_co());
            }
        }
    }

    private IEnumerator DetachRockAfterTime(GameObject rock, float time)
    {
        isRock = false;
        yield return new WaitForSeconds(time);
        //yield return new WaitForSeconds(throwAnimation.length);
        isAction = false;
        agent.enabled = true;
        rock.transform.parent = null;
        Rigidbody rb = rock.GetComponent<Rigidbody>();
        
        if (rb == null) yield break;

        rb.isKinematic = false; 
        rb.useGravity = true;   
        
        // �÷��̾��� �������� ������
        // �÷��̾��� ���� ����
        var handPos = handPosition.position;
        var playerGroundPos = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        Vector3 throwDirection = playerGroundPos - handPos;

        float elapsedTime = 0f;
        while (elapsedTime < addForceDuration)
        {
            elapsedTime += Time.deltaTime;

            UnityEngine.Debug.DrawLine(handPos, handPos + throwDirection * 100f, Color.magenta);

            var force = throwDirection * Mathf.Lerp(throwForce, 0, (elapsedTime / addForceDuration));
            rb.AddForce(force, ForceMode.Acceleration);

            yield return null;
        }


    }

    // ���߱� 
    private void Dance()
    {
        if(health_p.CurrentHP <= 0 && !isDance)
        {
            isDance = true;
            mutantAnimator.SetTrigger("Dance");
        }
        
    }
    // �׷α�
    private void Groggy()
    {
        if(health_m.CurrentHP <= health_m.MaxHP * 0.5f && !isGroggy)
        {
            isGroggy = true;
            mutantAnimator.SetTrigger("Groggy");
        }
    }
    // �ױ�
    private void Die()
    {
        if(health_m.CurrentHP <= 0 && !isDie)
        {
            isDie = true;
            ToggleRagdoll(true);
            StartCoroutine(Clear_DollyCamera_co());
        }
    }

    //��ƼŬ ���� (�� ��)
    private void WeakParticle_On()
    {
        WeakParticle.SetActive(true);
    }
    private void WeakParticle_Off()
    {
        WeakParticle.SetActive(false);
    }
    private void StrongParticle_On()
    {
        StrongParticle.SetActive(true);
    }
    private void StrongParticle_Off()
    {
        StrongParticle.SetActive(false);
    }
    private IEnumerator Clear_DollyCamera_co()
    {
        On_Clear_DollyCamera_.SetActive(true);
        yield return new WaitForSeconds(3f);
        On_Clear_DollyCamera_.SetActive(false);
    }

}