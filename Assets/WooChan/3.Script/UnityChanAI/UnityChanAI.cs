using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Fury))]
[RequireComponent(typeof(Health))]
public class UnityChanAI : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;
    private AttackController attackController;
    private Animator animator;
    private Fury fury;

    [SerializeField] private Health _Health;
    private bool isDie = false;
    [SerializeField] private GameObject Eye_L;
    [SerializeField] private GameObject Eye_R;
    [SerializeField] private GameObject Hand_L;
    [SerializeField] private GameObject Hand_R;


    [SerializeField] private Health p_Health;
    [SerializeField] private GameObject Sword;
    [SerializeField] private Material SwordMaterial;
    private Color AlphaZero;
    private Color SwordColor;

    [SerializeField] private GameObject Aura1;
    private Transform Aura1Parent;
    [SerializeField] private Collider Aura1Collider;

    [SerializeField] private GameObject Aura2;
    private Transform Aura2Parent;
    [SerializeField] private Collider Aura2Collider;

    [SerializeField] private GameObject Aura3;
    private Transform Aura3Parent;
    [SerializeField] private Collider Aura3Collider;

    // AttackCollider
    [SerializeField] private Collider hookCollider;
    [SerializeField] private Collider rightHookCollider;
    [SerializeField] private Collider screwAttackCollider;
    [SerializeField] private Collider projectileCollider;
    [SerializeField] private Collider advPunchCollider;
    [SerializeField] private Collider kickCollider;
    [SerializeField] private Collider sweepCollider;
    [SerializeField] private Collider dropKickCollider;

    [SerializeField] private Collider slideCollider;
    [SerializeField] private Collider backFlipCollider;
    [SerializeField] private Collider crossPunchCollider;
    //------------------------------------------------------
    [SerializeField] private bool isIdle = false;
    [SerializeField] private bool isWalk = false;
    [SerializeField] private bool isWalk_R = false;
    [SerializeField] private bool isWalk_L = false;

    [SerializeField] private GameObject Target = null;
    [SerializeField] private float TargetDistance = 0f;
    [SerializeField] private float WalkSpeed = 4f;
    [SerializeField] private float RunSpeed = 50f;
    [SerializeField] private float MDSpeed = 100f;

    [SerializeField] private bool near = false;
    [SerializeField] private bool middle = false;
    [SerializeField] private bool far = false;

    [SerializeField] private float NearPatternGraceTime = 0f;
    [SerializeField] private float NearPatternTime = 0f;
    [SerializeField] private float MiddlePatternGraceTime = 0f;
    [SerializeField] private float MiddlePatternTime = 0f;
    [SerializeField] private float farPatternGraceTime = 0f;

    [SerializeField] private float MustDieGraceTime = 0f;

    [SerializeField] private GameObject projectile;
    private Transform ProjectileParent;
    [SerializeField] private float ProjectileSpeed;

    private bool middleP1 = false;
    private bool middleP2 = false;
    private bool AdvPunch_T1 = false;
    private bool AdvPunch_T2 = false;
    private bool CartWheelNext = false;
    private int CartWheelRan = 0;

    private Coroutine MoveDelay = null;

    [SerializeField] private bool farP_Next = false; // farpattern 다음으로 넘어가기 위해서 한번더 체크하는용도
    [SerializeField] private bool farP_trigger = false;

    [SerializeField] private bool isMotion = false;
    
    private bool isFury = false;
    private bool isMove = false;
    private int P_layer;

    private void Awake()
    {
        TryGetComponent(out attackController);
        TryGetComponent(out animator);
        TryGetComponent(out fury);
    }

    private void Start()
    {
        P_layer = LayerMask.GetMask("Player", "Ghost");

        MiddlePatternTime = Random.Range(3, 5);
        NearPatternTime = Random.Range(1f, 2f);

        AlphaZero = new Color(0, 0, 0, 0);
        SwordColor = new Color(0, 0, 0, 1);

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
        if (_Health.CurrentHP <=0 && !isDie)
        {
            isDie = true;
            isMotion = true;
            animator.SetTrigger("Die");
            Eye_L.SetActive(false);
            Eye_R.SetActive(false);
            Hand_L.SetActive(false);
            Hand_R.SetActive(false);
        }
        if (fury.Flag && !isFury && p_Health.CurrentHP > 0 && !isMotion)
        {
            StopAllCoroutines();
            ResetPos();
            animator.SetTrigger("MustDie");
            isFury = true;
        }
        
        if (Target && !isMotion && p_Health.CurrentHP > 0)
        {
            LookAt_Rotation_Y(Target.transform);
        }
        if(p_Health.CurrentHP <= 0)
        {
            StopCoroutine(DecidePattern());
            StopMovement();
            ResetPos();
        }

        AuraSlash();
        CartWheel();
        AdvancePunch();
        MiddlePattern();
        FarPattern();
        MustDiePattern();
    }

    private void SearchPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 100f, P_layer);
        foreach (Collider collider in hitColliders)
        {
            Target = collider.gameObject;
        }
    }

    private void CheckDistance()
    {

        Collider[] near = Physics.OverlapSphere(transform.position, 11f, P_layer);

        Collider[] middle = Physics.OverlapSphere(transform.position, 40f, P_layer);
        
        Collider[] far = Physics.OverlapSphere(transform.position, 80f, P_layer);
        
        if (near.Length > 0)
        {
            this.near = true;
            this.middle = false;
            this.far = false;
        }
        else if (middle.Length > 0)
        {
            this.near = false;
            this.middle = true;
            this.far = false;
        }
        else if (far.Length > 0)
        {
            this.near = false;
            this.middle = false;
            this.far = true;
        }
        else
        {
            this.near = false;
            this.middle = false;
            this.far = false;
            fury.CurrentFuryGauge += 20f * Time.deltaTime;

        }

        TargetDistance = Vector3.Distance(transform.position, Target.transform.position);

        if (TargetDistance < 25f)
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
        Vector3 direction = lookAtPosition - transform.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
    }


    private IEnumerator DecidePattern()
    {
        while (true)
        {

            if (!isMotion && p_Health.CurrentHP > 0)
            {

                if (near)
                {
                    StopMovement();

                    while (NearPatternGraceTime < NearPatternTime && near)
                    {
                        if (!isMotion)
                        {
                            NearPatternGraceTime += Time.deltaTime;
                        }
                        yield return null;
                    }
                    if (near && NearPatternGraceTime > NearPatternTime)
                    {
                        DecideNearPattern();

                        NearPatternGraceTime = 0f;
                        NearPatternTime = Random.Range(0.5f, 2f);
                    }
                }
                else
                {
                    RandomMovement();
                }

                if ((middle || near) && farPatternGraceTime > 2f)
                {
                    farPatternGraceTime = 2f;
                }
                if (middle)
                {
                    while (MiddlePatternGraceTime < MiddlePatternTime && middle)
                    {
                        if (!isMotion)
                        {
                            MiddlePatternGraceTime += Time.deltaTime;
                        }
                        yield return null;
                    }
                    if(middle && MiddlePatternGraceTime > MiddlePatternTime)
                    {
                        int Ran = Random.Range(0, 4);
                        if(Ran == 0 || Ran == 1)
                        {
                            ResetPos();
                            //isIdle = true;
                            animator.SetTrigger("Roll");
                        }
                        else if (Ran == 2)
                        {
                            ResetPos();
                            isIdle = true;
                            animator.SetTrigger("AuraSlash");
                        }
                        else if (Ran == 3)
                        {
                            ResetPos();
                            isIdle = true;
                            animator.SetTrigger("AdvancePunch");
                        }
                        MiddlePatternGraceTime = 0f;
                        MiddlePatternTime = Random.Range(2f, 4f);
                    }
                }
                if (far)
                {
                    RandomMovement();
                    while (farPatternGraceTime < 6f && far)
                    {
                        if (!isMotion)
                        {
                            farPatternGraceTime += Time.deltaTime;
                        }
                        yield return null;
                    }
                    if (far && farPatternGraceTime > 5f)
                    {
                        int Ran = Random.Range(0, 2);
                        
                        if(Ran == 0)
                        {
                            ResetPos();
                            isIdle = true;
                            animator.SetTrigger("FarPattern");
                        }
                        else if(Ran == 1)
                        {
                            ResetPos();
                            isIdle = true;
                            animator.SetTrigger("MiddlePattern");
                        }
                        farPatternGraceTime = 0f;
                    }
                }
                yield return null;


            }
            yield return null;
        }
        
    }

    private void DecideNearPattern()
    {
        int Ran = Random.Range(0, 8);
        isMotion = true;
        ResetPos();
        if (Ran == 0 || Ran == 7)
        {
            animator.SetTrigger("Hook");
            attackController.ChangeAttackType(AttackType.Weak);
            attackController.AttackCollider = hookCollider;
            attackController.StrongAttackBaseDamage = 20;
        }
        else if (Ran == 1 || Ran == 2)
        {
            animator.SetTrigger("RightHook");
            attackController.ChangeAttackType(AttackType.Weak);
            attackController.AttackCollider = rightHookCollider;
            attackController.StrongAttackBaseDamage = 20;
        }
        else if (Ran == 3)
        {
            animator.SetTrigger("Kick");
            attackController.ChangeAttackType(AttackType.Weak);
            attackController.AttackCollider = kickCollider;
            attackController.StrongAttackBaseDamage = 25;
        }
        else if (Ran == 4)
        {
            animator.SetTrigger("LegSweep");
            attackController.ChangeAttackType(AttackType.Weak);
            attackController.AttackCollider = sweepCollider;
            attackController.StrongAttackBaseDamage = 25;
        }
        else if (Ran == 5)
        {
            animator.SetTrigger("DropKick");
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = dropKickCollider;
            attackController.StrongAttackBaseDamage = 70;
        }
        
        else if (Ran == 6)
        {
            animator.SetTrigger("CartWheel");
        }

    }

    private void Off_isMotion() //Animator용
    {
        isMotion = false;
    }

    private void ResetPos()
    {
        isIdle = true;
        isWalk = false;
        isWalk_R = false;
        isWalk_L = false;
        animator.SetBool("isIdle",true);
        animator.SetBool("isWalk", false);
        animator.SetBool("isSide_R", false);
        animator.SetBool("isSide_L", false);
    }
    private IEnumerator MoveDelay_co()
    {
        if (MoveDelay == null)
        {
            float Motiondelay;
            Motiondelay = Random.Range(2, 4);
            isMove = true;
            yield return new WaitForSeconds(Motiondelay);
            isMove = false;
        }
    }

    private void RandomMovement()
    {
        if (!isMove)
        {
            ResetPos();
            isIdle = false;
            int Ran = Random.Range(0, 5);
            if (Ran == 0 || Ran == 1 || Ran == 4)
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
            //else if (Ran == 4)
            //{
            //    isIdle = true;
            //}
            StartCoroutine(MoveDelay_co());
        }
    }
    private void StopMovement()
    {
        if (isMove)
        {
            MoveDelay = null;
            isMove = false;
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
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = screwAttackCollider;
            attackController.StrongAttackBaseDamage = 100;

            isMotion = true;
            transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;

            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("FarPattern2"))
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
            isMotion = true;
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f)
            {
                hitCollider.enabled = false;
                transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                hitCollider.enabled = true;
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
                attackController.ChangeAttackType(AttackType.Strong);
                attackController.AttackCollider = slideCollider;
                attackController.StrongAttackBaseDamage = 50;
            }
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1")) // 슬라이드
        {
            
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.7f)
            {
                transform.position += transform.TransformDirection(Vector3.forward) * MDSpeed * Time.deltaTime;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                Quaternion Look = Quaternion.LookRotation(Target.transform.position - transform.position);

                transform.rotation = Quaternion.Slerp(transform.rotation, Look, 0.15f);
            }
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1_5") && !near) // 달리는 중이고 플레이어가 5f안에 없을때
        {
            LookAt_Rotation_Y(Target.transform);
            transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
            
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie1_5") && near)
        {
            animator.SetTrigger("MD_Near");
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = backFlipCollider;
            attackController.StrongAttackBaseDamage = 50;
        }
        else if(animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie2")) //백플립
        {
            
            LookAt_Rotation_Y(Target.transform);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MustDie3")) //크로스펀치
        {
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = crossPunchCollider;
            attackController.StrongAttackBaseDamage = 10000;
            LookAt_Rotation_Y(Target.transform);
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    //--------------------------------------------middlepattern
    private void MiddlePattern()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MiddlePattern1"))
        {
            isMotion = true;
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = projectileCollider;
            attackController.StrongAttackBaseDamage = 100;
            LookAt_Rotation_Y(Target.transform);
            if (!middleP1)
            {
                StartCoroutine(MiddleP_Delay());
            } 
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("MiddlePattern3"))
        {

            if (!middleP2)
            {
                StartCoroutine(MiddleP_Delay2());
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
        float Ran = Random.Range(1f, 2f);
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


    //--------------------------------------------AdvancePunch & CartWheel
    private void CartWheel()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Cartwheel"))
        {
            isMotion = true;
            LookAt_Rotation_Y(Target.transform);
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.7f)
            {
                transform.position += transform.TransformDirection(Vector3.back) * 20f * Time.deltaTime;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.75f)
            {
                if (!CartWheelNext)
                {
                    CartWheelNext = true;
                    CartWheelRan = Random.Range(0, 3);
                    if (CartWheelRan == 0)
                    {
                        animator.SetTrigger("AdvancePunch");
                    }
                    else
                    {
                        animator.SetTrigger("CartWheelNext");
                    }
                    StartCoroutine(CartWheelDelay());
                }
            }
        }
    }
    private IEnumerator CartWheelDelay()
    {
        yield return new WaitForSeconds(0.5f);
        CartWheelNext = false;
        isMotion = false;
        yield break;
    }


    private void AdvancePunch()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("AdvancePunch1"))
        {
            isMotion = true;
            attackController.ChangeAttackType(AttackType.Strong);
            attackController.AttackCollider = advPunchCollider;
            attackController.StrongAttackBaseDamage = 100;
            LookAt_Rotation_Y(Target.transform);
            if (!AdvPunch_T1)
            {
                StartCoroutine(AdvPunchDelay()); // 1~3초 후 패턴2로 넘어감
            }
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("AdvancePunch2"))
        {
            
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.65f)
            {
                LookAt_Rotation_Y(Target.transform);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.65f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f)
            {
                transform.position += transform.TransformDirection(Vector3.forward) * 600f * Time.deltaTime;
            }
            else if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                if (!AdvPunch_T2)
                {
                    StartCoroutine(AdvPunchDelay2());
                }
                
            }
        }
        else
        {
            AdvPunch_T1 = false;
            AdvPunch_T2 = false;
        }
    }
    private IEnumerator AdvPunchDelay()
    {
        AdvPunch_T1 = true;
        float Ran = Random.Range(0.5f, 1f);
        yield return new WaitForSeconds(Ran);

        animator.SetTrigger("AdvancePunch2");
        yield break;
    }
    private IEnumerator AdvPunchDelay2()
    {
        AdvPunch_T2 = true;
        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("AdvancePunch3");
        attackController.TurnOffAttackCollider();
        isMotion = false;
        yield break;
    }

    //--------------------------------------------AdvancePunch


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
                transform.position += transform.TransformDirection(Vector3.forward) * 20f * Time.deltaTime;
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


    //--------------------------------------------AuraSlash
    private void SwordOn()
    {
        Sword.SetActive(true);
        StartCoroutine(SwordFadeIn());
    }
    private void SwordOff()
    {
        StartCoroutine(SwordFadeOut());

    }
    private IEnumerator SwordFadeIn()
    {
        float startTime = Time.time;
        while (Time.time - startTime < 1.5f)
        {
            float t = (Time.time - startTime) / 1.5f;
            SwordMaterial.color = Color.Lerp(AlphaZero, SwordColor, t);
            yield return null;
        }
        SwordMaterial.color = SwordColor;
        yield break;
    }
    private IEnumerator SwordFadeOut()
    {
        float startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            float t = (Time.time - startTime) / 1f;
            SwordMaterial.color = Color.Lerp(SwordColor, AlphaZero, t);
            yield return null;
        }
        SwordMaterial.color = AlphaZero;
        Sword.SetActive(false);

        yield break;
    }
    private void Aura1On()
    {
        Aura1Parent = Aura1.transform.parent;
        Aura1.transform.parent = null;
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = Aura1Collider;
        attackController.StrongAttackBaseDamage = 50;
        attackController.TurnOnAttackCollider();
        Aura1.SetActive(true);
    }
    private void Aura2On()
    {
        Aura2Parent = Aura2.transform.parent;
        Aura2.transform.parent = null;
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = Aura2Collider;
        attackController.StrongAttackBaseDamage = 50;
        attackController.TurnOnAttackCollider();
        Aura2.SetActive(true);
    }
    private void Aura3On()
    {
        Aura3Parent = Aura3.transform.parent;
        Aura3.transform.parent = null;
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = Aura3Collider;
        attackController.StrongAttackBaseDamage = 50;
        attackController.TurnOnAttackCollider();
        Aura3.SetActive(true);
    }
    private void AuraOff()
    {
        Aura1.transform.parent = Aura1Parent;
        Aura2.transform.parent = Aura2Parent;
        Aura3.transform.parent = Aura3Parent;
        Aura1.transform.localPosition = Vector3.zero;
        Aura2.transform.localPosition = Vector3.zero;
        Aura3.transform.localPosition = Vector3.zero;
        Aura1.transform.localRotation = Quaternion.identity;
        Aura2.transform.localRotation = Quaternion.identity;
        Aura3.transform.localRotation = Quaternion.identity;
        Aura1.SetActive(false);
        Aura2.SetActive(false);
        Aura3.SetActive(false);
    }
    private void AnimatorSpeed(float Speed)
    {
        animator.speed = Speed;
    }

    private void AuraSlash()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("AuraSlash1"))
        {
            LookAt_Rotation_Y(Target.transform);
            if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
            {
                isMotion = true;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                isMotion = false;
            }
        }
    }
    //--------------------------------------------AuraSlash

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 12f);
        Gizmos.DrawWireSphere(transform.position, 40f);
        Gizmos.DrawWireSphere(transform.position, 80f);
    }
}
