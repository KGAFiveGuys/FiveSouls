using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanAI : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject Target = null;
    [SerializeField] private float WalkSpeed = 2f;
    [SerializeField] private float RunSpeed = 5f;

    //private int MovementRan;
    //private int SideMovementRan;
    private bool isMotion = false;

    private void Awake()
    {
        TryGetComponent(out animator);
    }


    private void FixedUpdate()
    {
        if (Target == null)
        {
            SearchPlayer();
        }
        else if (Target) // 플레이어만 바라봄
        {
            transform.LookAt(Target.transform);
        }
    }

    private void Update()
    {

        if (!isMotion) //모션 중이 아닐 때만 새로운 모션 시작
        {
            RanMovement();
            StartCoroutine(MotionDelay());
        }
        else
        {
            return;
        }

        #region 움직임 관련
        //if (MovementRan == 0)
        //{
        //    return;
        //}
        //else if (MovementRan == 1)
        //{
        //    WalkMovement();
        //}
        //else if (MovementRan == 2)
        //{
        //    RunMovement();
        //}
        //else if (MovementRan == 3)
        //{
        //    SideMovement(SideMovementRan);
        //}
        #endregion

        Movement();

    }



    private void SearchPlayer()
    {
        int layerMask = LayerMask.GetMask("Player");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f, layerMask);
        foreach (Collider collider in hitColliders)
        {
            Target = collider.gameObject;
        }
    }

    private void RanMovement()
    {
        int Ran = Random.Range(0, 4);
        switch (Ran)
        {
            case 0:
                break;
            case 1:
                WalkMovement();
                break;
            case 2:
                RunMovement();
                break;
            case 3:
                SideMovement();
                break;
            default:
                return;
        }

    }

    private void WalkMovement()
    {
        animator.SetBool("isWalk", true);
    }

    private void RunMovement()
    {
        animator.SetBool("isRun", true);
    }
    
    private void SideMovement()
    {
        int RL = Random.Range(0, 2);

        if (RL == 0)
        {
            animator.SetBool("isSide_R", true);
        }
        else if (RL == 1)
        {
            animator.SetBool("isSide_L", true);
        }
    }

    private IEnumerator MotionDelay() //모든 불값은 여기서 끄기
    {
        float Motiondelay;
        Motiondelay = Random.Range(0, 3);
        isMotion = true;
        yield return new WaitForSeconds(Motiondelay);
        isMotion = false;

        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);
        animator.SetBool("isSide_R", false);
        animator.SetBool("isSide_L", false);

    }

    private void Movement()
    {
        if (animator.GetBool("isWalk"))
        {
            transform.position += transform.TransformDirection(Vector3.forward) * WalkSpeed * Time.deltaTime;
        }
        else if (animator.GetBool("isRun"))
        {
            transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
        }
        else if (animator.GetBool("isSide_R"))
        {
            transform.position += transform.TransformDirection(Vector3.right) * WalkSpeed * Time.deltaTime;
        }
        else if (animator.GetBool("isSide_L"))
        {
            transform.position += transform.TransformDirection(Vector3.left) * WalkSpeed * Time.deltaTime;
        }

    }














    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 20f);
    }
}
