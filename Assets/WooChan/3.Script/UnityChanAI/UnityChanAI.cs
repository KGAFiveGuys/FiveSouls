using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanAI : MonoBehaviour
{
    private Animator animator;
    [SerializeField] Rigidbody rigidbody;

    [SerializeField] private State currentState;
    private IdleState IdleState;
    private WalkState WalkState;
    private RunState RunState;
    private SideRState SideRState;
    private SideLState SideLState;

    [SerializeField] private GameObject Target = null;
    [SerializeField] private float WalkSpeed = 2f;
    [SerializeField] private float RunSpeed = 30f;
    [SerializeField] private float MDSpeed = 50f;

    [SerializeField] private bool nearPattern = false;
    [SerializeField] private bool middlePattern = false;
    [SerializeField] private bool farPattern = false;

    private bool farP_Next = false;

    [SerializeField] private bool isMotion = false; //애니메이션이 Idle인지 체크

    private bool isDelay = false;
    private int P_layer;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out rigidbody);
    }

    private void Start()
    {
        P_layer = LayerMask.GetMask("Player");

        IdleState = new IdleState(animator);
        WalkState = new WalkState(transform, WalkSpeed);
        RunState = new RunState(transform, RunSpeed);
        SideRState = new SideRState(transform, WalkSpeed);
        SideLState = new SideLState(transform, WalkSpeed);

        currentState = State.Idle;


        StartCoroutine(DecidePattern());
    }


    private void FixedUpdate()
    {
        if (Target == null)
        {
            SearchPlayer();
        }
        else
        {
            CheckDistance();
        }
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        if (Target && !isMotion) // 플레이어만 바라봄
        {
            LookAt_Rotation_Y(Target.transform);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            isMotion = false;
        } //Idle 인지 체크 후 IsMotion에 반환
        else
        {
            isMotion = true;
        }

        switch (currentState)
        {
            case State.Idle:
                IdleState.Update();
                break;
            case State.Walk:
                WalkState.Update();
                break;
            case State.Run:
                RunState.Update();
                break;
            case State.Side_R:
                SideRState.Update();
                break;
            case State.Side_L:
                SideLState.Update();
                break;
        }

        FarPattern();
        MustDiePattern();


    }

    private void SearchPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 50f, P_layer);
        foreach (Collider collider in hitColliders)
        {
            Target = collider.gameObject;
        }
    }

    private void CheckDistance()
    {

        Collider[] near = Physics.OverlapSphere(transform.position, 5f, P_layer);

        Collider[] middle = Physics.OverlapSphere(transform.position, 20f, P_layer);
        
        Collider[] far = Physics.OverlapSphere(transform.position, 35f, P_layer);
        
        if (near.Length > 0)
        {
            nearPattern = true;
            middlePattern = false;
            farPattern = false;
        }
        else if (middle.Length > 0)
        {
            nearPattern = false;
            middlePattern = true;
            farPattern = false;
        }
        else if (far.Length > 0)
        {
            nearPattern = false;
            middlePattern = false;
            farPattern = true;
        }
        else
        {
            nearPattern = false;
            middlePattern = false;
            farPattern = false;
        }

        Collider[] Check = Physics.OverlapSphere(transform.position, 15f, P_layer);

        if (Check.Length > 0 && (animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern1") || animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie0")))
        {
            farP_Next = true;
        }
        else
        {
            farP_Next = false;
        }
    }

    private void RanState()
    {
        int Ran = Random.Range(0, 4);
        switch (Ran)
        {
            case 0:
                currentState = State.Walk;
                animator.SetBool("isWalk", true);
                break;
            case 1:
                currentState = State.Walk;
                animator.SetBool("isWalk", true);
                break;
            case 2:
                currentState = State.Side_R;
                animator.SetBool("isSide_R", true);
                break;
            case 3:
                currentState = State.Side_L;
                animator.SetBool("isSide_L", true);
                break;
            case 4:
                currentState = State.Idle;
                break;
            case 5:
                currentState = State.Run;
                animator.SetBool("isRun", true);
                break;
            default:
                return;
        }

    }

    private void LookAt_Rotation_Y(Transform targetTransform) //재활용 간웅
    {
        Vector3 lookAtPosition = targetTransform.position;
        lookAtPosition.y = transform.position.y;

        transform.LookAt(lookAtPosition);
    }

    private IEnumerator MotionDelay()
    {
        float Motiondelay;
        Motiondelay = Random.Range(2, 5);
        isDelay = true;
        yield return new WaitForSeconds(Motiondelay);
        isDelay = false;
        animator.SetBool("isIdle", true);
        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);
        animator.SetBool("isSide_R", false);
        animator.SetBool("isSide_L", false);
    }

    private IEnumerator DecidePattern()
    {
        while (true)
        {

            if (!isDelay && !isMotion)
            {

                if (nearPattern)
                {
                    currentState = State.Idle;
                    DecideNearPattern();
                }
                else if (middlePattern || farPattern)
                {
                    RanState();
                    yield return new WaitForSeconds(3f);
                    StartCoroutine(MotionDelay());
                    if (farPattern)
                    {
                        yield return new WaitForSeconds(3f);
                        if (farPattern)
                        {
                            animator.SetTrigger("FarPattern");
                        }

                    }
                }
                else
                {
                    yield return new WaitForSeconds(3f);
                    if (!nearPattern && !middlePattern && !farPattern)
                    {
                        animator.SetTrigger("MustDie");
                        yield break;
                    }
                }

                yield return new WaitForSeconds(2f);


            }
            yield return null;
        }

    }

    private void DecideNearPattern()
    {
        int Ran = Random.Range(0, 5);
        if (Ran == 0)
        {
            return;
        }
        else if (Ran == 1)
        {
            animator.SetTrigger("Pattern1");
        }
        else if (Ran == 2)
        {
            animator.SetTrigger("Pattern2");
        }
        else if (Ran == 3)
        {
            animator.SetTrigger("Pattern3");
        }
        else if (Ran == 4)
        {
            animator.SetTrigger("Pattern4");
        }


    }

    private void FarPattern()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern1"))
        {
            isMotion = true;
            LookAt_Rotation_Y(Target.transform);
            transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern2") && farP_Next)
            {
                animator.SetTrigger("FarPattern2");
            }
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern2"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.4f)
            {
                transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                isMotion = false;
            }
        }
        
    }

    private void MustDiePattern()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie0")) //런
        {
            isMotion = true;
            LookAt_Rotation_Y(Target.transform);
            transform.position += transform.TransformDirection(Vector3.forward) * MDSpeed * Time.deltaTime;

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1") && farP_Next)
            {
                animator.SetTrigger("MD_Slide");
            }
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1")) // 슬라이드
        {
            if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.7f)
            {
                transform.position += transform.TransformDirection(Vector3.forward) * MDSpeed * Time.deltaTime;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                Quaternion Look = Quaternion.LookRotation(Target.transform.position - transform.position);

                transform.rotation = Quaternion.Slerp(transform.rotation, Look, 0.15f);
            }
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1_5") && !nearPattern) // 달리는 중이고 플레이어가 5f안에 없을때
        {
            
            transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
            
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1_5") && nearPattern)
        {
            animator.SetTrigger("MD_Near");
        }
        else if(animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie2")) //백플립
        {
            LookAt_Rotation_Y(Target.transform);
        }
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5f);
        Gizmos.DrawWireSphere(transform.position, 20f);
        Gizmos.DrawWireSphere(transform.position, 35f);
    }
}
