using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanAI : MonoBehaviour
{
    private Animator animator;
    [SerializeField] Rigidbody rigidbody;
    
    [SerializeField] private bool isIdle = false;
    [SerializeField] private bool isWalk = false;
    [SerializeField] private bool isWalk_R = false;
    [SerializeField] private bool isWalk_L = false;



    [SerializeField] private GameObject Target = null;
    [SerializeField] private float TargetDistance = 0f;
    [SerializeField] private float WalkSpeed = 2f;
    [SerializeField] private float RunSpeed = 30f;
    [SerializeField] private float MDSpeed = 50f;

    [SerializeField] private bool nearPattern = false;
    [SerializeField] private bool middlePattern = false;
    [SerializeField] private bool farPattern = false;


    [SerializeField] private float MiddlePatternGraceTime = 0f;
    [SerializeField] private float MiddlePatternTime = 0f;
    [SerializeField] private float farPatternGraceTime = 0f;


    [SerializeField] private float MustDieGraceTime = 0f;

    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform ProjectileParent;
    [SerializeField] private float ProjectileSpeed;
    private bool middleP1 = false;
    private bool middleP2 = false;

    [SerializeField] private bool farP_Next = false; // farpattern 다음으로 넘어가기 위해서 한번더 체크하는용도
    [SerializeField] private bool farP_trigger = false;

    [SerializeField] private bool isMotion = false;

    private bool isMove = false;
    private int P_layer;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out rigidbody);
    }

    private void Start()
    {
        P_layer = LayerMask.GetMask("Player");
        MiddlePatternTime = Random.Range(3, 10);

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
            MoveUpdate();
        }
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        if (Target && !isMotion) // 플레이어만 바라봄
        {
            LookAt_Rotation_Y(Target.transform);
        }

        //if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        //{
        //    isMotion = false;
        //} //Idle 인지 체크 후 IsMotion에 반환
        //else
        //{
        //    isMotion = true;
        //}

        

        MiddlePattern();
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
        
        Collider[] far = Physics.OverlapSphere(transform.position, 40f, P_layer);
        
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

        TargetDistance = Vector3.Distance(transform.position, Target.transform.position);

        if (TargetDistance < 10f)
        {
            farP_Next = true;
        }
        else
        {
            farP_Next = false;
        }
    }


    private void LookAt_Rotation_Y(Transform targetTransform) 
    {
        Vector3 lookAtPosition = targetTransform.position;
        lookAtPosition.y = transform.position.y;

        transform.LookAt(lookAtPosition);
    }


    private IEnumerator DecidePattern()
    {
        while (true)
        {

            if (!isMotion)
            {

                if (nearPattern)
                {
                    DecideNearPattern();
                }
                else
                {
                    RandomMovement();
                }

                if (middlePattern || nearPattern)
                {
                    farPatternGraceTime = 0f;
                    MustDieGraceTime = 0f;
                }
                if (middlePattern)
                {
                    while (MiddlePatternGraceTime < 10f && middlePattern)
                    {
                        if (!isMotion)
                        {
                            MiddlePatternGraceTime += Time.deltaTime;
                        }
                        yield return null;
                    }
                    if(middlePattern && MiddlePatternGraceTime > MiddlePatternTime)
                    {
                        int Ran = Random.Range(0, 3);
                        if(Ran == 0 || Ran == 1)
                        {
                            ResetPos();
                            isIdle = true;
                            animator.SetTrigger("Roll");
                        }
                        else
                        {
                            ResetPos();
                            isIdle = true;
                            StartCoroutine(BackBackPattern());
                        }
                        MiddlePatternGraceTime = 0f;
                        MiddlePatternTime = Random.Range(3, 10);

                    }
                }
                if (farPattern)
                {
                    MustDieGraceTime = 0f;
                    while (farPatternGraceTime < 6f && farPattern)
                    {
                        if (!isMotion)
                        {
                            farPatternGraceTime += Time.deltaTime;
                        }
                        yield return null;
                    }
                    if (farPattern && farPatternGraceTime > 5f)
                    {
                        int Ran = Random.Range(0, 2);
                        
                        if(Ran == 0)
                        {
                            animator.SetTrigger("FarPattern");
                            ResetPos();
                            isIdle = true;
                        }
                        else if(Ran == 1)
                        {
                            animator.SetTrigger("MiddlePattern");
                            ResetPos();
                            isIdle = true;
                        }
                        farPatternGraceTime = 0f;
                    }
                }
                else
                {
                    while (!nearPattern && !middlePattern && !farPattern && MustDieGraceTime < 10f)
                    {
                        yield return null;
                        MustDieGraceTime += Time.deltaTime;
                    }
                    if (!nearPattern && !middlePattern && !farPattern && MustDieGraceTime > 10f)
                    {
                        animator.SetTrigger("MustDie");
                        ResetPos();
                        isIdle = true;
                        yield break;
                    }
                }

                yield return null;


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


    private void ResetPos()
    {
        isIdle = false;
        isWalk = false;
        isWalk_R = false;
        isWalk_L = false;
        animator.SetBool("isIdle",false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isSide_R", false);
        animator.SetBool("isSide_L", false);
    }
    private IEnumerator MoveDelay()
    {
        float Motiondelay;
        Motiondelay = Random.Range(3, 5);
        isMove = true;
        yield return new WaitForSeconds(Motiondelay);
        isMove = false;
        yield break;
    }

    private void RandomMovement()
    {
        if (!isMove)
        {
            ResetPos();
            int Ran = Random.Range(0, 5);
            if (Ran == 0 || Ran == 1)
            {
                isWalk = true;
            }
            else if (Ran == 2)
            {
                isWalk_R = true;
            }
            else if (Ran == 3)
            {
                isWalk_L = true;
            }
            else if (Ran == 4)
            {
                isIdle = true;
            }
            StartCoroutine(MoveDelay());
        }
    }
    private void MoveUpdate()
    {
        if (!isMotion)
        {

            if (isIdle)
            {
                animator.SetBool("isIdle", true);
                animator.SetBool("isWalk", false);
                animator.SetBool("isSide_R", false);
                animator.SetBool("isSide_L", false);

            }
            else if (isWalk)
            {
                transform.position += transform.TransformDirection(Vector3.forward) * WalkSpeed * Time.deltaTime;
                animator.SetBool("isWalk", true);
                animator.SetBool("isIdle", false);
                animator.SetBool("isSide_R", false);
                animator.SetBool("isSide_L", false);
            }
            else if (isWalk_R)
            {
                transform.position += transform.TransformDirection(Vector3.right) * WalkSpeed * Time.deltaTime;
                animator.SetBool("isSide_R", true);
                animator.SetBool("isWalk", false);
                animator.SetBool("isIdle", false);
                animator.SetBool("isSide_L", false);
            }
            else if (isWalk_L)
            {
                transform.position += transform.TransformDirection(Vector3.left) * WalkSpeed * Time.deltaTime;
                animator.SetBool("isSide_L", true);
                animator.SetBool("isWalk", false);
                animator.SetBool("isSide_R", false);
                animator.SetBool("isIdle", false);
            }

        }
        Roll();
        RollBack();
        
    }

    private void FarPattern()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern1"))
        {
            isMotion = true;
            transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f && !animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern2"))
            {
                LookAt_Rotation_Y(Target.transform);
            }

            if (farP_Next && !farP_trigger)
            {
                animator.SetTrigger("FarPattern2");
                farP_trigger = true;
            }
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern2"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.4f)
            {
                transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                isMotion = false;
                farP_trigger = false;
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

    //--------------------------------------------middlepattern
    private void MiddlePattern()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MiddlePattern1"))
        {
            isMotion = true;
            LookAt_Rotation_Y(Target.transform);
            if (!middleP1)
            {
                StartCoroutine(MiddleP_Delay()); // 1~4초 후 패턴2로 넘어감
            } 
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MiddlePattern3"))
        {
            if (!middleP2)
            {
                StartCoroutine(MiddleP_Delay2()); // 1초 후 패턴 넘어감
            }
            LookAt_Rotation_Y(Target.transform);
        }
        else
        {
            middleP1 = false;
            middleP2 = false;
        }
    }
    private IEnumerator MiddleP_Delay()
    {
        middleP1 = true;
        float Ran = Random.Range(1, 4);
        yield return new WaitForSeconds(Ran);
        animator.SetTrigger("MiddlePattern2");
        yield break;
    }
    private IEnumerator MiddleP_Delay2()
    {
        middleP2 = true;
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("MiddlePattern3");
        isMotion = false;
        yield break;
    }

    private void Onprojectile()
    {
        projectile.SetActive(true);
        StartCoroutine(LookAtProjectile_co(Target.transform));
    }
    private void MoveProjectile(float duration)
    {
        StopCoroutine(LookAtProjectile_co(Target.transform));
        StartCoroutine(MoveProjectile_co(duration));
    }
    private IEnumerator MoveProjectile_co(float duration)
    {
        float elapsedTime = 0f;
        ProjectileParent = projectile.transform.parent;
        projectile.transform.parent = null;
        while (elapsedTime < duration)
        {
            projectile.transform.position += transform.TransformDirection(Vector3.forward) * ProjectileSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        projectile.transform.parent = ProjectileParent;
        projectile.transform.position = projectile.transform.parent.position;
        projectile.SetActive(false);
        yield break;
    }

    private IEnumerator LookAtProjectile_co(Transform targetTransform)
    {
        while (true)
        {
            Vector3 lookAtPosition = targetTransform.position;
            lookAtPosition.y = transform.position.y;

            transform.LookAt(lookAtPosition);
            yield return null;
        }
    }
    //--------------------------------------------middlepattern

    private void Roll()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Roll"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f)
            {
                LookAt_Rotation_Y(Target.transform);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f)
            {
                isMotion = true;
                transform.position += transform.TransformDirection(Vector3.forward) * 10f * Time.deltaTime;
            }
            else
            {
                isMotion = false;
            }
        }
    }
    private void RollBack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("RollBack"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f)
            {
                LookAt_Rotation_Y(Target.transform);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f)
            {
                isMotion = true;
                transform.position += transform.TransformDirection(Vector3.back) * 10f * Time.deltaTime;
            }
            else
            {
                isMotion = false;
            }
        }
    }

    private IEnumerator BackBackPattern()
    {
        isMotion = true;
        animator.SetTrigger("RollBack");
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("RollBack");
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("MiddlePattern");
        yield break;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 5f);
        Gizmos.DrawWireSphere(transform.position, 10f);
        Gizmos.DrawWireSphere(transform.position, 20f);
        Gizmos.DrawWireSphere(transform.position, 40f);
    }
}
