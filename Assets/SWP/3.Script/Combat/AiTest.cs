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
    private enum HulkState
    {
        Navi,
        Attack,
    }

    [Header("Ragdoll Field")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("Hulk Field")]
    [SerializeField] private LayerMask TargetMask;
    private float distance = 100f;
    [SerializeField] private float detectRange = 30f;
    private float AdDistance = 5f;
    [SerializeField] private float stopdistance = 1.25f;//근접공격거리 & 보스멈출거리
    [SerializeField] private Collider[] AttackCollider;
    [SerializeField] private float PatternCoolTime = 4f;
    [SerializeField] private float Timer = 4f;
    [SerializeField] private bool PatternOn;
    private Transform Target;
    private Animator HulkAnimator;
    private NavMeshAgent agent;
    private Health bossHealth;
    private AttackController attackController;

    [Header("ETC")]
    [SerializeField] private GameObject JumpLandingEffect;
    [SerializeField] private GameObject Curtain;

    //보스상태
    private HulkState currentState;
    private bool isDead = false;
    private bool isTarget = false;
    private bool isLook = false;
    private bool isAngry = true; // 광폭화
    private bool onlyone = true; // 광폭화


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
        currentState = HulkState.Navi;
    }
    private void Start()
    {
        if (isTarget)
        {
            StartCoroutine(AttackCooldown());
        }
    }

    private void FixedUpdate()
    {
        CalcDistance();
        HulkAnimator.SetBool("HasTarget", isTarget);
        if (isTarget)
        {
            LookTarget();
            if (PatternOn)
            {
                DrawPattern();
            }
            //else
            //{
            //    SetNavi();
            //}
        }
    }
    private void Update()
    {
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
    private void LookTarget()
    {
            if (isLook || !PatternOn)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                Vector3 lookVec = new Vector3(h, transform.position.y, v) * 5f;
                Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
                transform.LookAt(targetTF + lookVec);
            }
    }
    private IEnumerator AttackCooldown()
    {
        while (!isDead)
        {
            if (!PatternOn)
            {
                Timer = PatternCoolTime;
                while (Timer <= 0)
                {
                    Timer -= Time.deltaTime;
                    Debug.Log(Timer);
                }
                PatternOn = true;
            }
            yield return null;
        }
        yield return null;
    }
    private void SetNavi()
    {
        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        agent.SetDestination(targetTF);
        if (distance <= stopdistance * transform.localScale.x)
        {
            agent.isStopped = true;
        }
    }


    private void DrawPattern()
    {
        PatternOn = false;
        isLook = true;
        agent.isStopped = true;
        //agent.updateRotation = false;
        if (distance <= AdDistance * transform.localScale.x && distance > stopdistance * transform.localScale.x * 2f)
        {
            StopCoroutine(SlideAttack());
            StartCoroutine(SlideAttack());
        }
        else if (distance <= stopdistance * transform.localScale.x * 2f && distance > stopdistance * transform.localScale.x * 1.1f)
        {
            StopCoroutine(RightAttack());
            StartCoroutine(RightAttack());
        }
        else if (distance <= stopdistance * transform.localScale.x * 1.1f)
        {
            StopCoroutine(NearAttack());
            StartCoroutine(NearAttack());
        }
    }

    private IEnumerator FuryAttack()
    {
        AttackAlarm.Instance.RedAlarm();
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[3];
        attackController.StrongAttackBaseDamage = 50f;

        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 AttackPoint = Target.position + targetTF.normalized * 5f;
        agent.SetDestination(AttackPoint);
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

        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 AttackPoint = Target.position + targetTF.normalized * (AdDistance * transform.localScale.x - distance);
        agent.SetDestination(AttackPoint);

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

        Vector3 targetTF = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        Vector3 AttackPoint = targetTF.normalized * 2f*transform.localScale.z;
        agent.SetDestination(AttackPoint);

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
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }
    private void RestartChase()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }
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
