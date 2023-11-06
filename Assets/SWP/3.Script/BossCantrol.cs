using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BossCantrol : MonoBehaviour
{
    [Header("랙돌 필수품")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("보스 변수들")]
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private float detectRange = 30f;
    [SerializeField] private float stopdistance = 1.25f;//근접공격거리 & 보스멈출거리
    [SerializeField] private float MaxHP;
    [SerializeField] private float currentHP;
    private Transform Target;
    private Animator HulkAnimator;
    private NavMeshAgent agent;

    //공격범위
    private float distance = 50f;
    private float AdDistance = 5f;

    //보스상태
    private bool isDead = false;
    private bool isGroggyHit = false;
    private bool OnlyOne = true;
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
    }

    private void Update()
    {
        CalcDistance();
        HulkAnimator.SetBool("HasTarget", isTarget);
        NaviTarget();
        StartCoroutine(SelectPatternCo(distance));
        if (groggyStat >= 100)
        {
            groggyStat = 0;
            StartCoroutine(GroggyCo());
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
            agent.SetDestination(new Vector3(Target.position.x, 0, Target.position.z));
            Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
            transform.LookAt(Target.position + targetTF.normalized * 5f);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
        }

    }

    private IEnumerator SelectPatternCo(float distance)
    {
        delayTime = Random.Range(3, 8);
        // 체력이 반 이하면 점프공격해줭
        if (currentHP <= (MaxHP * 0.5f) && OnlyOne)
        {
            OnlyOne = false;
            yield return StartCoroutine(AngryAttackCo());
        }

        if (distance > stopdistance * transform.localScale.x * 2 && distance <= AdDistance * transform.localScale.x)
        {
            //슬라이딩해줭
            yield return StartCoroutine(SlideAttackCo());
        }
        else if (distance <= stopdistance * transform.localScale.x)
        {
            //근접공격해줭
            yield return StartCoroutine(AttackCo());
        }
        yield return new WaitForSeconds(delayTime);
    }

    private IEnumerator SlideAttackCo()
    {
        Debug.Log("slide");
        HulkAnimator.SetTrigger("Sliding");
        yield return new WaitForSeconds(1.5f);
        HulkAnimator.ResetTrigger("Sliding");
    }

    private IEnumerator AttackCo()
    {
        
        int SelectType = Random.Range(0, 4);
        AttackNum = Random.Range(0, 2);
        switch (SelectType)
        {
            case 0:
            case 1:
            case 2:
                yield return StartCoroutine(NormalAttackCo());
                break;

            case 3:
                yield return StartCoroutine(StrongAttackCo());
                break;
        }
    }

    private IEnumerator NormalAttackCo()
    {
        HulkAnimator.SetInteger("AttackNum", AttackNum);
        HulkAnimator.SetTrigger("NormalAttack");

        if (AttackNum == 0)
        {
            Debug.Log("normal1");
            yield return new WaitForSeconds(2.8f);
        }
        else
        {
            Debug.Log("normal2");
            yield return new WaitForSeconds(4.2f);
        }
        HulkAnimator.ResetTrigger("NormalAttack");
    }

    private IEnumerator StrongAttackCo()
    {
        HulkAnimator.SetInteger("AttackNum", AttackNum);
        HulkAnimator.SetTrigger("StrongAttack");

        if (AttackNum == 1)
        {
            Debug.Log("strong2");
            yield return new WaitForSeconds(2.2f);
            HulkAnimator.SetBool("HasTarget", false);
        }
        else
        {
            Debug.Log("strong1");
            yield return new WaitForSeconds(0.8f);
            HulkAnimator.SetBool("HasTarget", false);
        }
        HulkAnimator.ResetTrigger("StrongAttack");
        HulkAnimator.SetBool("HasTarget", isTarget);

    }

    private IEnumerator AngryAttackCo()
    {

        Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        Vector3 LandPoint = Target.position + targetTF.normalized * 5f;
        agent.enabled = false;
        HulkAnimator.SetTrigger("Angry");

        yield return new WaitForSeconds(3.3f);
        agent.enabled = true;
        targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        agent.enabled = false;

        yield return new WaitForSeconds(3.3f);
        agent.enabled = true;
        targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        agent.enabled = false;

        yield return new WaitForSeconds(3.3f);
        agent.enabled = true;
        targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        agent.enabled = false;

        yield return new WaitForSeconds(3f);
        agent.enabled = true;
    }

    private IEnumerator GroggyCo()
    {
        HulkAnimator.SetTrigger("Groggy");
        if (isGroggyHit)
        {
            HulkAnimator.SetBool("isGroggyHit", isGroggyHit);
            yield return new WaitForSeconds(5f);
            isGroggyHit = false;
            HulkAnimator.SetBool("isGroggyHit", isGroggyHit);

        }
        yield return new WaitForSeconds(7.5f);
        HulkAnimator.ResetTrigger("Groggy");

    }

    private void Die()
    {
        isDead = true;
    }
}
