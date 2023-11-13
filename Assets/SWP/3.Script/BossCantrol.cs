using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AttackController))]
public class BossCantrol : MonoBehaviour
{
    [Header("랙돌 필수품")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("보스 변수들")]
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private float detectRange = 30f;
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

    //공격범위
    private float distance = 50f;
    private float AdDistance = 5f;

    //보스상태
    private bool PatternOn = false; //패턴 공격중인지
    private bool isDead = false;
    private bool isAngry = true; // 광폭화
    private bool isGroggyHit = false;//공격받았는지 아닌지로 트루 펄스 리턴하는 거로 수정필요
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

    //상태조건
    private float delayTime;
    private float groggyStat = 0;
    int AttackNum;

    private void OnDrawGizmos()
    {
        //탐지범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        if (isTarget)
        {
            Gizmos.DrawLine(transform.position, new Vector3(Target.position.x, 0, Target.position.z).normalized * 15f);
        }

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

    private void Update()
    {
        if (bossHealth.CurrentHP <= 0)
        {
            isDead = true;
            Curtain.SetActive(false);
            ToggleRagdoll(isDead);
        }
        else
        {
            CalcDistance();
            HulkAnimator.SetBool("HasTarget", isTarget);
            if (agent.enabled != false)
            {
                NaviTarget();
            }
            StartCoroutine(SelectPatternCo(distance));
            if (groggyStat >= 100)
            {
                groggyStat = 0;
                StartCoroutine(GroggyCo());
            }
            
        }
    }

    private void CalcDistance()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask);
        if (colls.Length > 0)
        {
            Target = colls[0].transform;
            Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
            Vector3 currentTF = new Vector3(transform.position.x, 0, transform.position.z);
            distance = Vector3.Distance(targetTF, currentTF);
        }
    }

    private void NaviTarget()
    {
        if (isTarget)
        {
            agent.stoppingDistance = stopdistance * transform.localScale.x;
            Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
            agent.SetDestination(targetTF);
            transform.LookAt(targetTF);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private IEnumerator SelectPatternCo(float distance)
    {                
        if (!PatternOn)
        {
            PatternOn = true;
            if (bossHealth.CurrentHP <= (bossHealth.MaxHP * 0.5f) && isAngry)
            {
                isAngry = false;
                yield return AngryAttackCo();
            }
            delayTime = Random.Range(0.5f, 3);
            yield return new WaitForSeconds(delayTime);
            //일반공격
            if (distance > stopdistance * transform.localScale.x * 3f && distance <= AdDistance * transform.localScale.x)
            {
                //슬라이딩해줭
                yield return SlideAttackCo();
            }
            else if (distance > stopdistance * transform.localScale.x*2f && distance <= stopdistance * transform.localScale.x * 2.5f)
            {
                //근접공격해줭
                yield return RightAttackCo();
            }
            else if (distance <= stopdistance * transform.localScale.x)
            {
                //근접공격해줭
                yield return AttackCo();
            }
            PatternOn = false;
        }
    }

    private IEnumerator AngryAttackCo()
    {
        StartCoroutine(RedAlarm.Instance.StrongAlarm());
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[3];
        attackController.WeakAttackBaseDamage = 50f;

        Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        Vector3 LandPoint = Target.position + targetTF.normalized * 5f;
        agent.SetDestination(LandPoint);
        HulkAnimator.SetTrigger("Angry");

        yield return new WaitForSeconds(9f);
        //attackController.AttackCollider.enabled = false;
        agent.SetDestination(targetTF + targetTF.normalized*5f);
        JumpEffectOff();
    }

    private IEnumerator SlideAttackCo()
    {
        StartCoroutine(RedAlarm.Instance.WeakAlarm());

        yield return new WaitForSeconds(1f);
        attackController.ChangeAttackType(AttackType.Weak);
        attackController.AttackCollider = AttackCollider[2];
        attackController.WeakAttackBaseDamage = 25f;

        HulkAnimator.SetTrigger("Sliding");

        yield return new WaitForSeconds(1.1f);
        HulkAnimator.ResetTrigger("Sliding");
        //attackController.AttackCollider.enabled = false;
    }

    private IEnumerator AttackCo()
    {        
        int SelectType = Random.Range(0, 3);
        AttackNum = Random.Range(0, 2);
        switch (SelectType)
        {
            case 0:
            case 1:
                yield return NormalAttackCo();
                break;

            case 2:
                yield return StrongAttackCo();
                break;
        }
    }

    private IEnumerator NormalAttackCo()
    {
        StartCoroutine(RedAlarm.Instance.WeakAlarm());
        attackController.ChangeAttackType(AttackType.Weak);
        attackController.AttackCollider = AttackCollider[0];
        attackController.WeakAttackBaseDamage = 10f;

        HulkAnimator.SetInteger("AttackNum", AttackNum);
        HulkAnimator.SetTrigger("NormalAttack");
        agent.isStopped = true;

        if (AttackNum == 0)
        {
            yield return new WaitForSeconds(2f);
            HulkAnimator.ResetTrigger("NormalAttack");
        }
        else
        {
            yield return new WaitForSeconds(3.25f);
            HulkAnimator.ResetTrigger("NormalAttack");
            HulkAnimator.SetFloat("AniSpeed", 1f);
        }
    }

    private IEnumerator StrongAttackCo()
    {
        StartCoroutine(RedAlarm.Instance.StrongAlarm());
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[0];
        attackController.WeakAttackBaseDamage = 30f;

        HulkAnimator.SetInteger("AttackNum", 1);
        HulkAnimator.SetTrigger("StrongAttack");

        yield return new WaitForSeconds(2f);
        HulkAnimator.ResetTrigger("StrongAttack");
        //attackController.AttackCollider.enabled = false;
    }

    private IEnumerator RightAttackCo()
    {
        StartCoroutine(RedAlarm.Instance.StrongAlarm());
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[1];
        attackController.WeakAttackBaseDamage = 30f;

        HulkAnimator.SetInteger("AttackNum", 0);
        HulkAnimator.SetTrigger("StrongAttack");

        yield return new WaitForSeconds(1.5f);
        HulkAnimator.ResetTrigger("StrongAttack");
        //attackController.AttackCollider.enabled = false;
    }

    private IEnumerator GroggyCo()
    {
        HulkAnimator.SetTrigger("Groggy");
        if (isGroggyHit)
        {
            HulkAnimator.SetBool("isGroggyHit", isGroggyHit);
            yield return new WaitForSeconds(5f);//플레이어 그로기 어택시간만큼!!!!
            isGroggyHit = false;
            HulkAnimator.SetBool("isGroggyHit", isGroggyHit);
            Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
            agent.SetDestination(targetTF);
        }
        yield return new WaitForSeconds(7.5f);
        HulkAnimator.ResetTrigger("Groggy");
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
