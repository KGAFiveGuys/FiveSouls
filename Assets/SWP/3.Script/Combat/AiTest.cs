using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AttackController))]
public class AiTest : MonoBehaviour
{
    [Header("Ragdoll Field")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("Hulk Field")]
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private float detectRange = 30f;
    [SerializeField]private float distance = 100f;
    private float AdDistance = 5f;
    [SerializeField] private float stopdistance = 1.25f;//근접공격거리 & 보스멈출거리
    [SerializeField] private Collider[] AttackCollider;
    [SerializeField] private float PatternCoolTime = 4f;
    [SerializeField] private float Timer = 0f;
    [SerializeField] private bool isAttack;
    private Transform Target;
    private Animator HulkAnimator;
    private NavMeshAgent agent;
    private Health bossHealth;
    private AttackController attackController;
    //보스상태
    private bool isDead = false;
    private bool isTarget
    {
        get
        {
            if (distance <= detectRange && distance >= stopdistance * transform.localScale.x && !isDead)
            {
                return true;
            }
            return false;
        }
    }
    private bool isLook = false;
    [Header("Attack Tyoe")]
    [SerializeField]private bool isFar = false;
    [SerializeField]private bool isMiddle = false;
    [SerializeField] private bool isClose = false;
    private bool timestart = false;
    private bool isAngry = true; // 광폭화

    [Header("ETC")]
    [SerializeField] private GameObject JumpLandingEffect;
    [SerializeField] private GameObject Curtain;



    private void OnDrawGizmos()
    {
        //탐지범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        //대쉬공격 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AdDistance * transform.localScale.x);

        //근접공격 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopdistance * transform.localScale.x);
    }

    private void Awake()
    {
        TryGetComponent(out agent);
        TryGetComponent(out HulkAnimator);
        TryGetComponent(out bossHealth);
        TryGetComponent(out attackController);
    }
    private void Start()
    {
    }

    private void FixedUpdate()
    {
        GetTargetDistance();
    }
    private void Update()
    {
        agent.stoppingDistance = stopdistance * transform.localScale.x;

        if (bossHealth.CurrentHP <= 0)
        {
            if (isDead)
                return;

            isDead = true;
            Curtain.SetActive(!isDead);
            ToggleRagdoll(isDead);
        }
        //광폭화 패턴
        if (bossHealth.CurrentHP <= (bossHealth.MaxHP * 0.5f) && isAngry)
        {
            isAngry = false;
            StartCoroutine(FuryAttack());
        }
        
        StartCoroutine(TimeFlowCo());
        LookTarget();
        JudgeAttack();
        if (isAttack)
        {
            SelectAttackType();
            StartCoroutine(AttackCo());
        }
    }

    private void GetTargetDistance()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask);
        if (colls.Length > 0)
        {
            Target = colls[0].transform;
            Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
            Vector3 currentTF = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            distance = Vector3.Distance(targetTF, currentTF);
        }
        HulkAnimator.SetBool("HasTarget", isTarget);
    }
    private IEnumerator TimeFlowCo()
    {
        if (isTarget && !timestart)
        {
            timestart = true;
            while (!isDead)
            {
                Timer += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }
        yield return null;
    }
    private void LookTarget()
    {
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 lookVec = new Vector3(h * 5f, 0, v * 5f);
            Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
            transform.LookAt(targetTF + lookVec);
        }
    }

    private void JudgeAttack()
    {
        //쿨탐 지났고 공격중이 아니면 공격실행
        if (Timer >= PatternCoolTime)
        {
            if (!isAttack)
            {
                isAttack = true;
                isLook = false;
                agent.enabled = false;
                Timer = 0;
                //if (!isFar && !isMiddle && !isClose)
                //{
                //    Navi();
                //}
            }
        }
        else
        {
            Navi();
        }
    }

    private void Navi()
    {
        agent.enabled = false;
        if (isTarget && !isAttack)
        {
            isLook = true;
            agent.enabled = true;
            Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
            agent.SetDestination(targetTF);
            if (distance <= (stopdistance * transform.localScale.x) && distance > (stopdistance * transform.localScale.x)*0.8f)
            {
                agent.enabled = false;
            }
            else if(distance <= (stopdistance * transform.localScale.x) * 0.8f)
            {
                agent.enabled = true;
                Vector3 center = new Vector3(0, transform.position.y, 0);
                agent.SetDestination(center);
            }
        }
        //if (isAttack && !isFar && !isMiddle && !isClose)
        //{
        //    isLook = true;
        //    agent.enabled = true;
        //    Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        //    agent.SetDestination(targetTF);
        //    if (distance <= (stopdistance * transform.localScale.x) && distance > (stopdistance * transform.localScale.x) * 0.8f)
        //    {
        //        agent.enabled = false;
        //    }
        //    else if (distance <= (stopdistance * transform.localScale.x) * 0.8f)
        //    {
        //        agent.enabled = true;
        //        Vector3 center = new Vector3(0, transform.position.y, 0);
        //        agent.SetDestination(center);
        //    }
        //}
    }

    private void SelectAttackType()
    {
        if (distance <= AdDistance * transform.localScale.x && distance > stopdistance * transform.localScale.x * 2.2f)
        {
            isFar = true;
            if (isMiddle || isClose)
            {
                isFar = false;
            }
        }
        else if (distance <= stopdistance * transform.localScale.x * 2.2f && distance > stopdistance * transform.localScale.x * 1.5f)
        {
            isMiddle = true;
            if (isFar || isClose)
            {
                isMiddle = false;
            }
        }
        else if (distance <= stopdistance * transform.localScale.x * 1.2f)
        {
            isClose = true;
            if (isMiddle || isFar)
            {
                isClose = false;
            }
        }
        if (!isAttack)
        {
            isFar = false;
            isMiddle = false;
            isClose = false;
        }
    }

    private IEnumerator FuryAttack()
    {
        AttackAlarm.Instance.RedAlarm();
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[3];
        attackController.StrongAttackBaseDamage = 75f;

        HulkAnimator.SetTrigger("Angry");
        agent.enabled = true;
        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 AttackPoint = Target.position + targetTF.normalized * 5f;
        agent.SetDestination(AttackPoint);

        yield return new WaitForSeconds(9f);
        agent.ResetPath();
        agent.SetDestination(targetTF);
        JumpEffectOff();
        agent.enabled = false;
        isAttack = false;
    }

    private IEnumerator AttackCo()
    {
        if (isFar)
        {
            yield return SlideAttackCo();
            isFar = false;
        }
        if (isMiddle)
        {
            yield return RightAttackCo();
            isMiddle = false;
        }
        if (isClose)
        {
            yield return CloseAttackCo();
            isClose = false;
        }
    }

    private IEnumerator SlideAttackCo()
    {
        AttackAlarm.Instance.YellowAlarm();
        attackController.ChangeAttackType(AttackType.Weak);
        attackController.AttackCollider = AttackCollider[2];
        attackController.WeakAttackBaseDamage = 30f;


        yield return new WaitForSeconds(0.5f);
        HulkAnimator.SetTrigger("Sliding");
        agent.enabled = true;
        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 AttackPoint = Target.position + targetTF.normalized * (AdDistance * transform.localScale.x - distance);
        agent.SetDestination(AttackPoint);

        yield return new WaitForSeconds(1.1f);
        HulkAnimator.ResetTrigger("Sliding");
        agent.enabled = false;
        isAttack = false;
    }

    private IEnumerator RightAttackCo()
    {
        AttackAlarm.Instance.RedAlarm();
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[1];
        attackController.StrongAttackBaseDamage = 60f;


        yield return new WaitForSeconds(0.5f);
        HulkAnimator.SetTrigger("StrongAttack");
        HulkAnimator.SetInteger("AttackNum", 0);
        agent.enabled = true;
        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 AttackPoint = targetTF.normalized * 2f * transform.localScale.z;
        agent.SetDestination(AttackPoint);

        yield return new WaitForSeconds(1.5f);
        HulkAnimator.ResetTrigger("StrongAttack");
        agent.enabled = false;
        isAttack = false;
    }

    private IEnumerator CloseAttackCo()
    {
        int SelectType = Random.Range(0, 5);
        switch (SelectType)
        {
            case 0:
            case 1:
                AttackAlarm.Instance.YellowAlarm();
                attackController.ChangeAttackType(AttackType.Weak);
                attackController.AttackCollider = AttackCollider[0];
                attackController.WeakAttackBaseDamage = 20f;

                yield return new WaitForSeconds(0.5f);
                HulkAnimator.SetInteger("AttackNum", 0);
                HulkAnimator.SetTrigger("NormalAttack");

                yield return new WaitForSeconds(2f);
                HulkAnimator.ResetTrigger("NormalAttack");
                break;
            case 2:
            case 3:
                AttackAlarm.Instance.YellowAlarm();
                attackController.ChangeAttackType(AttackType.Weak);
                attackController.AttackCollider = AttackCollider[0];
                attackController.WeakAttackBaseDamage = 20f;

                yield return new WaitForSeconds(0.5f);
                HulkAnimator.SetInteger("AttackNum", 1);
                HulkAnimator.SetTrigger("NormalAttack");

                yield return new WaitForSeconds(4f);
                HulkAnimator.ResetTrigger("NormalAttack");
                break;
            case 4:
                AttackAlarm.Instance.RedAlarm();
                attackController.ChangeAttackType(AttackType.Strong);
                attackController.AttackCollider = AttackCollider[0];
                attackController.StrongAttackBaseDamage = 50f;

                yield return new WaitForSeconds(0.5f);
                HulkAnimator.SetTrigger("StrongAttack");
                HulkAnimator.SetInteger("AttackNum", 1);

                yield return new WaitForSeconds(2.1f);
                HulkAnimator.ResetTrigger("StrongAttack");
                break;
        }
        isAttack = false;
    }

    public void ToggleRagdoll(bool isRagdoll)
    {
        foreach (var c in ragdollColliders)
        {
            c.enabled = isRagdoll;
        }
        foreach (var rb in ragdollRigidbodies)
        {
            rb.useGravity = isRagdoll;
            rb.isKinematic = !isRagdoll;
        }
        HulkAnimator.enabled = !isRagdoll;
        foreach (var rbs in ragdollRigidbodies)
        {
            rbs.velocity = Vector3.zero;
        }
    }

    #region 애니메이션 이벤트Methods
    private void StopChase()
    {
        agent.enabled = false;
    }
    private void RestartChase()
    {
        agent.enabled = true;
    }

    private void LookOff()
    {
        isLook = false;
    }
    private void LookOn()
    {
        isLook = true;
    }

    private void JumpEffectOn()
    {
        JumpLandingEffect.SetActive(true);
    }
    private void JumpEffectOff()
    {
        JumpLandingEffect.SetActive(false);
    }

    private void AniSpeed(float speed)
    {
        HulkAnimator.SetFloat("AniSpeed", speed);
    }
    #endregion

}
