using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AttackController))]
public class HulkCantrol : MonoBehaviour
{

    #region 새거
    [Header("랙돌 필수품")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("보스 변수들")]
    [SerializeField] private LayerMask TargetMask;
    private float distance = 100f;
    [SerializeField] private float detectRange = 30f;
    private float AdDistance = 5f;
    [SerializeField] private float stopdistance = 1.25f;//근접공격거리 & 보스멈출거리
    [SerializeField] private Collider[] AttackCollider;
    private Transform Target;
    private Animator HulkAnimator;
    private NavMeshAgent agent;
    private Health bossHealth;
    private AttackController attackController;

    [Header("ETC")]
    [SerializeField] private GameObject JumpLandingEffect;
    [SerializeField] private GameObject Curtain;

    //보스상태
    private bool PatternOn = false; //패턴 공격중인지
    private bool isDead = false;
    private bool isAngry = true; // 광폭화
    private bool isTarget = false;
    private bool NaviOn = false;
    private bool isLook = false;

    //상태조건
    int AttackNum;

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
    private void FixedUpdate()
    {
        CalcDistance();
        HulkAnimator.SetBool("HasTarget", isTarget);
        if (!PatternOn)
        {
            StartCoroutine(DrawPattern());
        }
    }

    private void Update()
    {
        if (bossHealth.CurrentHP <= 0)
        {
            if (isDead)
                return;

            isDead = true;
            Curtain.SetActive(false);
            ToggleRagdoll(isDead);
        }
        if (isTarget && !PatternOn)
        {
            SetNavi();
        }
        LookTarget();
    }


    private void CalcDistance()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask);
        if (colls.Length > 0)
        {
            isTarget = true;
            Target = colls[0].transform;
            Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
            Vector3 currentTF = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            distance = Vector3.Distance(targetTF, currentTF);
        }
        else
        {
            isTarget = false;
        }
    }

    private void SetNavi()
    {
        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        agent.SetDestination(targetTF);
    }

    private void LookTarget()
    {
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Horizontal");
            Vector3 lookVec = new Vector3(h, transform.position.y, v) * 5f;
            Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
            transform.LookAt(targetTF + lookVec);
        }
    }

    private IEnumerator DrawPattern()
    {
        if (bossHealth.CurrentHP <= (bossHealth.MaxHP * 0.5f) && isAngry)
        {
            isAngry = false;
            yield return FuryAttack();
        }
        else
        {
            PatternOn = true;
            //광폭화 공격
            float delay = Random.Range(0.5f, 4f);
            yield return new WaitForSeconds(delay);
            if (distance <= AdDistance * transform.localScale.x && distance > stopdistance * transform.localScale.x * 2f)
            {
                StartCoroutine(SlideAttack());
            }
            else if (distance <= stopdistance * transform.localScale.x * 2f && distance > stopdistance * transform.localScale.x * 1.1f)
            {
                StartCoroutine(RightAttack());
            }
            else if (distance <= stopdistance * transform.localScale.x * 1.1f)
            {
                StartCoroutine(NearAttack());
            }
        }
    }

    private IEnumerator FuryAttack()
    {
        AttackAlarm.Instance.RedAlarm();
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[3];
        attackController.StrongAttackBaseDamage = 50f;

        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 LandPoint = Target.position + targetTF.normalized * 5f;
        agent.SetDestination(LandPoint);
        HulkAnimator.SetTrigger("Angry");

        yield return new WaitForSeconds(9f);
        agent.SetDestination(targetTF);
        JumpEffectOff();
    }

    private IEnumerator SlideAttack()
    {
        AttackAlarm.Instance.YellowAlarm();
        attackController.ChangeAttackType(AttackType.Weak);
        attackController.AttackCollider = AttackCollider[2];
        attackController.WeakAttackBaseDamage = 25f;

        yield return new WaitForSeconds(0.5f);
        HulkAnimator.SetTrigger("Sliding");

        yield return new WaitForSeconds(1.1f);
        HulkAnimator.ResetTrigger("Sliding");
        PatternOn = false;
    }

    private IEnumerator RightAttack()
    {
        AttackAlarm.Instance.RedAlarm();
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[1];
        attackController.StrongAttackBaseDamage = 30f;

        yield return new WaitForSeconds(0.5f);
        HulkAnimator.SetTrigger("StrongAttack");
        HulkAnimator.SetInteger("AttackNum", 0);

        yield return new WaitForSeconds(1.5f);
        HulkAnimator.ResetTrigger("StrongAttack");
        PatternOn = false;
    }

    private IEnumerator NearAttack()
    {
        int SelectType = Random.Range(0, 5);
        switch (SelectType)
        {
            case 0:
            case 1:
                AttackAlarm.Instance.YellowAlarm();
                attackController.ChangeAttackType(AttackType.Weak);
                attackController.AttackCollider = AttackCollider[0];
                attackController.WeakAttackBaseDamage = 15f;

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
                attackController.WeakAttackBaseDamage = 15f;

                yield return new WaitForSeconds(0.5f);
                HulkAnimator.SetInteger("AttackNum", 1);
                HulkAnimator.SetTrigger("NormalAttack");

                yield return new WaitForSeconds(3.2f);
                HulkAnimator.ResetTrigger("NormalAttack");
                break;
            case 4:
                AttackAlarm.Instance.RedAlarm();
                attackController.ChangeAttackType(AttackType.Strong);
                attackController.AttackCollider = AttackCollider[0];
                attackController.StrongAttackBaseDamage = 30f;

                yield return new WaitForSeconds(0.5f);
                HulkAnimator.SetTrigger("StrongAttack");
                HulkAnimator.SetInteger("AttackNum", 1);

                yield return new WaitForSeconds(2.1f);
                HulkAnimator.ResetTrigger("StrongAttack");
                break;
        }
        PatternOn = false;
    }
    #endregion


    //#region 기존
    //[Header("랙돌 필수품")]
    //[SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    //[SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    //[Header("보스 변수들")]
    //[SerializeField] private LayerMask TargetMask;
    //[SerializeField] private float detectRange = 30f;
    //[SerializeField] private float stopdistance = 1.25f;//근접공격거리 & 보스멈출거리
    //[SerializeField] private Collider[] AttackCollider;
    //private Transform Target;
    //private Animator HulkAnimator;
    //private NavMeshAgent agent;
    //private Health bossHealth;
    //private AttackController attackController;

    //[Header("ETC")]
    //[SerializeField] private GameObject JumpLandingEffect;
    //[SerializeField] private GameObject Curtain;

    ////공격범위
    //private float distance = 100f;
    //private float AdDistance = 5f;

    ////보스상태
    //private bool PatternOn = false; //패턴 공격중인지
    //private bool isDead = false;
    //private bool isAngry = true; // 광폭화
    //private bool isGroggyHit = false;
    //private bool isTarget
    //{
    //    get
    //    {
    //        if (distance <= detectRange && distance >= stopdistance * transform.localScale.x && !isDead)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //}

    ////상태조건
    //private float delayTime;
    //private float groggyStat = 0;
    //int AttackNum;

    //private void OnDrawGizmos()
    //{
    //    //탐지범위
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, detectRange);
    //    if (isTarget)
    //    {
    //        Gizmos.DrawLine(transform.position, new Vector3(Target.position.x, transform.position.y, Target.position.z).normalized * 15f);
    //    }

    //    //대쉬공격 범위
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, AdDistance * transform.localScale.x);

    //    //근접공격 범위
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireSphere(transform.position, stopdistance * transform.localScale.x);
    //}

    //private void Awake()
    //{
    //    TryGetComponent(out agent);
    //    TryGetComponent(out HulkAnimator);
    //    TryGetComponent(out bossHealth);
    //    TryGetComponent(out attackController);
    //}

    //private void Update()
    //{
    //    if (bossHealth.CurrentHP <= 0)
    //    {
    //        if (isDead)
    //            return;

    //        isDead = true;
    //        Curtain.SetActive(false);
    //        ToggleRagdoll(isDead);
    //    }
    //    else
    //    {
    //        CalcDistance();
    //        HulkAnimator.SetBool("HasTarget", isTarget);
    //        if (agent.enabled)
    //        {
    //            NaviTarget();
    //        }
    //        if (distance <= AdDistance * transform.localScale.x)
    //        {
    //            StartCoroutine(SelectPatternCo(distance));
    //        }
    //        //if (groggyStat >= 100)
    //        //{
    //        //    groggyStat = 0;
    //        //    StartCoroutine(GroggyCo());
    //        //}            
    //    }

    //    Debug.Log(Target.transform.position);
    //}

    //private void CalcDistance()
    //{
    //    Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask);
    //    if (colls.Length > 0)
    //    {
    //        Target = colls[0].transform;
    //        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    //        Vector3 currentTF = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    //        distance = Vector3.Distance(targetTF, currentTF);
    //    }
    //}

    //private void NaviTarget()
    //{
    //    if (isTarget && !PatternOn)
    //    {
    //        agent.stoppingDistance = stopdistance * transform.localScale.x;
    //        currentUpdateDestination = UpdateDestination();
    //        StartCoroutine(currentUpdateDestination);
    //        agent.isStopped = false;
    //    }
    //    else
    //    {
    //        if (currentUpdateDestination != null)
    //        {
    //            StopCoroutine(currentUpdateDestination);
    //            currentUpdateDestination = null;
    //        }
    //        agent.isStopped = true;
    //    }
    //}

    //private IEnumerator currentUpdateDestination;
    //private IEnumerator UpdateDestination()
    //{
    //    while (!PatternOn)
    //    {
    //        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    //        agent.SetDestination(targetTF);
    //        transform.LookAt(targetTF);
    //        yield return null;
    //    }
    //    currentUpdateDestination = null;
    //}

    //private IEnumerator SelectPatternCo(float distance)
    //{
    //    if (!PatternOn)
    //    {
    //        PatternOn = true;
    //        if (bossHealth.CurrentHP <= (bossHealth.MaxHP * 0.5f) && isAngry)
    //        {
    //            isAngry = false;
    //            yield return AngryAttackCo();
    //        }
    //        else
    //        {
    //            delayTime = Random.Range(0.5f, 3);
    //            yield return new WaitForSeconds(delayTime);
    //            //일반공격
    //            if (distance > stopdistance * transform.localScale.x * 2.25f && distance <= AdDistance * transform.localScale.x)
    //            {
    //                //슬라이딩해줭
    //                yield return SlideAttackCo();
    //            }
    //            else if (distance > stopdistance * transform.localScale.x * 1.5f && distance <= stopdistance * transform.localScale.x * 2.25f)
    //            {
    //                //근접공격해줭
    //                yield return RightAttackCo();
    //            }
    //            else if (distance <= stopdistance * transform.localScale.x)
    //            {
    //                //근접공격해줭
    //                yield return AttackCo();
    //            }
    //        }
    //        PatternOn = false;
    //    }
    //}

    //private IEnumerator AngryAttackCo()
    //{
    //    AttackAlarm.Instance.RedAlarm();
    //    attackController.ChangeAttackType(AttackType.Strong);
    //    attackController.AttackCollider = AttackCollider[3];
    //    attackController.StrongAttackBaseDamage = 50f;

    //    Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    //    Vector3 LandPoint = Target.position + targetTF.normalized * 5f;
    //    agent.SetDestination(LandPoint);
    //    HulkAnimator.SetTrigger("Angry");

    //    yield return new WaitForSeconds(9f);
    //    agent.SetDestination(targetTF);
    //    JumpEffectOff();
    //}

    //private IEnumerator SlideAttackCo()
    //{
    //    AttackAlarm.Instance.YellowAlarm();

    //    yield return new WaitForSeconds(1f);
    //    attackController.ChangeAttackType(AttackType.Weak);
    //    attackController.AttackCollider = AttackCollider[2];
    //    attackController.WeakAttackBaseDamage = 25f;

    //    HulkAnimator.SetTrigger("Sliding");

    //    yield return new WaitForSeconds(1.1f);
    //    HulkAnimator.ResetTrigger("Sliding");
    //    //attackController.AttackCollider.enabled = false;
    //}

    //private IEnumerator AttackCo()
    //{
    //    int SelectType = Random.Range(0, 3);
    //    AttackNum = Random.Range(0, 2);
    //    switch (SelectType)
    //    {
    //        case 0:
    //        case 1:
    //            yield return NormalAttackCo();
    //            break;

    //        case 2:
    //            yield return StrongAttackCo();
    //            break;
    //    }
    //}

    //private IEnumerator NormalAttackCo()
    //{
    //    AttackAlarm.Instance.YellowAlarm();
    //    attackController.ChangeAttackType(AttackType.Weak);
    //    attackController.AttackCollider = AttackCollider[0];
    //    attackController.WeakAttackBaseDamage = 10f;

    //    HulkAnimator.SetInteger("AttackNum", AttackNum);
    //    HulkAnimator.SetTrigger("NormalAttack");
    //    agent.isStopped = true;

    //    if (AttackNum == 0)
    //    {
    //        yield return new WaitForSeconds(2f);
    //        HulkAnimator.ResetTrigger("NormalAttack");
    //        PatternOn = false;
    //    }
    //    else
    //    {
    //        yield return new WaitForSeconds(3.25f);
    //        HulkAnimator.ResetTrigger("NormalAttack");
    //        HulkAnimator.SetFloat("AniSpeed", 1f);
    //    }
    //}

    //private IEnumerator StrongAttackCo()
    //{
    //    AttackAlarm.Instance.RedAlarm();
    //    attackController.ChangeAttackType(AttackType.Strong);
    //    attackController.AttackCollider = AttackCollider[0];
    //    attackController.StrongAttackBaseDamage = 30f;

    //    HulkAnimator.SetInteger("AttackNum", 1);
    //    HulkAnimator.SetTrigger("StrongAttack");

    //    yield return new WaitForSeconds(2f);
    //    HulkAnimator.ResetTrigger("StrongAttack");
    //    //attackController.AttackCollider.enabled = false;
    //}

    //private IEnumerator RightAttackCo()
    //{
    //    AttackAlarm.Instance.RedAlarm();
    //    attackController.ChangeAttackType(AttackType.Strong);
    //    attackController.AttackCollider = AttackCollider[1];
    //    attackController.StrongAttackBaseDamage = 30f;

    //    HulkAnimator.SetInteger("AttackNum", 0);
    //    HulkAnimator.SetTrigger("StrongAttack");

    //    yield return new WaitForSeconds(1.5f);
    //    HulkAnimator.ResetTrigger("StrongAttack");
    //    //attackController.AttackCollider.enabled = false;
    //}

    //private IEnumerator GroggyCo()
    //{
    //    HulkAnimator.SetTrigger("Groggy");
    //    if (isGroggyHit)
    //    {
    //        HulkAnimator.SetBool("isGroggyHit", isGroggyHit);
    //        yield return new WaitForSeconds(5f);//플레이어 그로기 어택시간만큼!!!!
    //        isGroggyHit = false;
    //        HulkAnimator.SetBool("isGroggyHit", isGroggyHit);
    //        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    //        agent.SetDestination(targetTF);
    //    }
    //    yield return new WaitForSeconds(7.5f);
    //    HulkAnimator.ResetTrigger("Groggy");
    //}
    //#endregion

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

    #region 애니메이션 이벤트함수
    private void StopChase()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    private void StopLook()
    {
        if (agent != null)
        {
            agent.updateRotation = false;
        }
    }

    private void RestartChase()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }
    private void RestartLook()
    {
        if (agent != null)
        {
            agent.updateRotation = true;
        }
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
