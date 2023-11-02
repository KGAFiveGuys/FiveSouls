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
    [SerializeField] private float RunSpeed = 5f;

    [SerializeField] private bool nearPattern = false;
    [SerializeField] private bool middlePattern = false;
    [SerializeField] private bool farPattern = false;

    private bool isAttack = false;

    private bool isMotion = false;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out rigidbody);
    }

    private void Start()
    {

        IdleState = new IdleState(animator);
        WalkState = new WalkState(transform, WalkSpeed);
        RunState = new RunState(transform, RunSpeed);
        SideRState = new SideRState(transform, WalkSpeed);
        SideLState = new SideLState(transform, WalkSpeed);

        currentState = State.Idle;

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
        if (Target) // 플레이어만 바라봄
        {
            transform.LookAt(Target.transform);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            isAttack = false;
            if (nearPattern)
            {
                int nearRan = Random.Range(0, 5);
                switch (nearRan)
                {
                    case 0:
                        animator.SetTrigger("Pattern1");
                        break;
                    case 1:
                        animator.SetTrigger("Pattern2");
                        break;
                    case 2:
                        animator.SetTrigger("Pattern3");
                        break;
                    case 3:
                        animator.SetTrigger("Pattern4");
                        break;
                }
                isAttack = true;
            }

            else if (farPattern)
            {
                rigidbody.AddForce(Vector3.forward * 200f * Time.deltaTime);
                animator.SetTrigger("FarPattern");
            }

            else if (middlePattern || farPattern)
            {

                if (!isMotion) //모션 중이 아닐 때만 새로운 모션 시작
                {
                    StartCoroutine(MotionDelay());
                    //RanState();
                }
                else
                {
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
                }
            }
        
        }

        

        
    }

    private void SearchPlayer()
    {
        int layerMask = LayerMask.GetMask("Player");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 50f, layerMask);
        foreach (Collider collider in hitColliders)
        {
            Target = collider.gameObject;
        }
    }

    private void CheckDistance()
    {
        int layerMask = LayerMask.GetMask("Player");

        Collider[] near = Physics.OverlapSphere(transform.position, 5f, layerMask);

        Collider[] middle = Physics.OverlapSphere(transform.position, 20f, layerMask);
        
        Collider[] far = Physics.OverlapSphere(transform.position, 35f, layerMask);
        
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
    }

    //private void RanState()
    //{
    //    int Ran = Random.Range(0, 2);
    //    switch (Ran)
    //    {
    //        case 0:
    //            currentState = State.Idle;
    //            break;
    //        case 1:
    //            currentState = State.Walk;
    //            animator.SetBool("isWalk", true);
    //            break;
    //        case 2:
    //            currentState = State.Run;
    //            animator.SetBool("isRun", true);
    //            break;
    //        case 3:
    //            currentState = State.Side_R;
    //            animator.SetBool("isSide_R", true);
    //            break;
    //        case 4:
    //            currentState = State.Side_L;
    //            animator.SetBool("isSide_L", true);
    //            break;
    //        default:
    //            return;
    //    }

    //}


    private IEnumerator MotionDelay()
    {
        float Motiondelay;
        Motiondelay = Random.Range(2, 5);
        isMotion = true;
        yield return new WaitForSeconds(Motiondelay);
        isMotion = false;

        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);
        animator.SetBool("isSide_R", false);
        animator.SetBool("isSide_L", false);
    }










    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5f);
        Gizmos.DrawWireSphere(transform.position, 20f);
        Gizmos.DrawWireSphere(transform.position, 35f);
    }
}
