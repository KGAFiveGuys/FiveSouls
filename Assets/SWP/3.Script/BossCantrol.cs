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
    [Header("���� �ʼ�ǰ")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("���� ������")]
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private float detectRange = 30f;
    [SerializeField] private float stopdistance = 1.25f;//�������ݰŸ� & ��������Ÿ�
    [SerializeField] private Collider[] AttackCollider;
    private Transform Target;
    private Animator HulkAnimator;
    private NavMeshAgent agent;
    private Health bossHealth;
    private AttackController attackController;

    [Header("��ƼŬ ����Ʈ")]
    [SerializeField] private GameObject JumpLandingEffect;

    private RedAlarm alarm;

    //���ݹ���
    private float distance = 50f;
    private float AdDistance = 5f;

    //��������
    private bool PatternOn = false; //���� ����������
    private bool isDead = false;
    private bool isAngry = true; // ����ȭ
    private bool isGroggyHit = false;//���ݹ޾Ҵ��� �ƴ����� Ʈ�� �޽� �����ϴ� �ŷ� �����ʿ�
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

    //��������
    private float delayTime;
    private float groggyStat = 0;
    int AttackNum;

    private void OnDrawGizmos()
    {
        //Ž������
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        if (isTarget)
        {
            Gizmos.DrawLine(transform.position, new Vector3(Target.position.x, 0, Target.position.z).normalized * 15f);
        }

        //�뽬���� ����
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AdDistance * transform.localScale.x);

        //�������� ����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopdistance * transform.localScale.x);
    }

    private void Awake()
    {
        TryGetComponent(out agent);
        TryGetComponent(out HulkAnimator);
        TryGetComponent(out bossHealth);
        TryGetComponent(out attackController);
        alarm = FindObjectOfType<RedAlarm>();
    }

    private void Update()
    {
        if (bossHealth.CurrentHP <= 0)
        {
            isDead = true;
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
            delayTime = Random.Range(0.5f, 2);
            yield return new WaitForSeconds(delayTime);
            //�Ϲݰ���
            if (distance > stopdistance * transform.localScale.x * 2 && distance <= AdDistance * transform.localScale.x)
            {
                //�����̵��آa
                yield return SlideAttackCo();
            }
            else if (distance > stopdistance * transform.localScale.x && distance <= stopdistance * transform.localScale.x * 2)
            {
                //���������آa
                yield return RightAttackCo();
            }
            else if (distance <= stopdistance * transform.localScale.x)
            {
                //���������آa
                yield return AttackCo();
            }
            PatternOn = false;
        }
    }

    private IEnumerator AngryAttackCo()
    {
        StartCoroutine(alarm.StorngAlarm());
        attackController.ChangeAttackType(AttackType.Strong);
        attackController.AttackCollider = AttackCollider[3];
        attackController.WeakAttackBaseDamage = 50f;

        Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        Vector3 LandPoint = Target.position + targetTF.normalized * 5f;
        agent.SetDestination(LandPoint);
        HulkAnimator.SetTrigger("Angry");

        //agent.enabled = false;

        //yield return new WaitForSeconds(.7f);
        //agent.enabled = true;
        //targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        //agent.enabled = false;

        //yield return new WaitForSeconds(1.2f);
        //agent.enabled = true;
        //targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        //agent.enabled = false;

        //yield return new WaitForSeconds(1.2f);
        //agent.enabled = true;
        //targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        //agent.enabled = false;

        //yield return new WaitForSeconds(1f);
        //agent.enabled = true;

        yield return new WaitForSeconds(9f);
        //attackController.AttackCollider.enabled = false;
        agent.SetDestination(targetTF + targetTF.normalized*5f);
        JumpEffectOff();
    }

    private IEnumerator SlideAttackCo()
    {
        StartCoroutine(alarm.WeakAlarm());
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
        StartCoroutine(alarm.WeakAlarm());
        attackController.ChangeAttackType(AttackType.Weak);
        attackController.AttackCollider = AttackCollider[0];
        attackController.WeakAttackBaseDamage = 10f;

        HulkAnimator.SetInteger("AttackNum", AttackNum);
        HulkAnimator.SetTrigger("NormalAttack");
        agent.isStopped = true;

        if (AttackNum == 0)
        {
            Debug.Log("����1");
            //yield return new WaitForSeconds(0.8f);
            //agent.updateRotation = false;
            //attackController.AttackCollider.gameObject.SetActive(true);

            //yield return new WaitForSeconds(0.3f);
            //agent.updateRotation = true;
            //attackController.AttackCollider.gameObject.SetActive(false);

            //yield return new WaitForSeconds(0.4f);
            //agent.updateRotation = false;
            //attackController.AttackCollider.gameObject.SetActive(true);

            //yield return new WaitForSeconds(0.25f);
            //agent.updateRotation = true;
            //attackController.AttackCollider.gameObject.SetActive(false);

            //yield return new WaitForSeconds(0.75f);
            //agent.isStopped = false;
            yield return new WaitForSeconds(2f);
            HulkAnimator.ResetTrigger("NormalAttack");
        }
        else
        {
            Debug.Log("����2");
            //yield return new WaitForSeconds(0.8f);
            //agent.updateRotation = false;
            //attackController.AttackCollider.gameObject.SetActive(true);

            //yield return new WaitForSeconds(0.3f);
            //agent.updateRotation = true;
            //attackController.AttackCollider.gameObject.SetActive(false);

            //yield return new WaitForSeconds(0.3f);
            //agent.updateRotation = false;
            //attackController.AttackCollider.gameObject.SetActive(true);

            //yield return new WaitForSeconds(0.4f);
            //agent.updateRotation = true;
            //attackController.AttackCollider.gameObject.SetActive(false);

            //yield return new WaitForSeconds(0.2f);
            //HulkAnimator.SetFloat("AniSpeed", 0.5f);

            //yield return new WaitForSeconds(0.5f);
            //HulkAnimator.SetFloat("AniSpeed", 1.5f);

            //yield return new WaitForSeconds(0.1f);
            //agent.updateRotation = false;
            //attackController.AttackCollider.gameObject.SetActive(true);

            //yield return new WaitForSeconds(0.2f);
            //agent.updateRotation = true;
            //attackController.AttackCollider.gameObject.SetActive(false);

            //yield return new WaitForSeconds(0.7f);
            //agent.isStopped = false;
            yield return new WaitForSeconds(3.25f);
            HulkAnimator.ResetTrigger("NormalAttack");
            HulkAnimator.SetFloat("AniSpeed", 1f);
        }
    }

    private IEnumerator StrongAttackCo()
    {
        StartCoroutine(alarm.StorngAlarm());
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
        StartCoroutine(alarm.StorngAlarm());
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
            yield return new WaitForSeconds(5f);//�÷��̾� �׷α� ���ýð���ŭ!!!!
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

        #region �ִϸ��̼� �̺�Ʈ�Լ�
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
    private void RedAlarmOn()
    {
        Material hulkMt = transform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material;
        hulkMt.color = new Color32(255, 0, 0, 80);
    }
    private void RedAlarmOff()
    {
        Material hulkMt = transform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material;
        hulkMt.color = new Color32(204, 204, 204, 255);
    }

    #endregion

}
