using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BossCantrol : MonoBehaviour
{
    [Header("���� �ʼ�ǰ")]
    [SerializeField] private List<Collider> ragdollColliders = new List<Collider>();
    [SerializeField] private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    [Header("���� ������")]
    [SerializeField] private LayerMask TargetMask;
    [SerializeField] private float detectRange = 30f;
    [SerializeField] private float stopdistance = 1.25f;//�������ݰŸ� & ��������Ÿ�
    private Animator HulkAnimator;
    //private Rigidbody rb;
    private Transform Target;
    private NavMeshAgent agent;

    //���ݹ���
    private float distance = 50f;
    private float AdDistance = 5f;

    //��������
    private bool isNormal = false;
    private bool isStrong = false;
    private bool isGroggy = false;
    private bool isGroggyHit = false;
    private bool isAngry = false;
    private bool isSlide = false;
    private bool isTarget
    {
        get
        {
            if (distance <= detectRange && distance >= stopdistance * transform.localScale.x /*&& !Target.isDead*/)   //�÷��̾� �������� �˷����~~~~~~~~~~~
            {
                return true;
            }
            return false;
        }
    }

    //��������
    private float delayTime;
    private float groggyStat = 0;
    private float MaxHP = 1000f;
    private float currentHP;


    private void Start()
    {
        currentHP = MaxHP;
        TryGetComponent(out agent);
        TryGetComponent(out HulkAnimator);
    }

    private void Update()
    {
        delayTime = Random.Range(0, 5);
        CalcDistance();
        NaviTarget(distance);
        HulkAnimator.SetBool("HasTarget", isTarget);
        StartCoroutine(SelectPatternCo(distance));
        //���� ������ �־���

    }

    private void OnDrawGizmos()
    {
        //Ž������
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        //Gizmos.DrawRay(transform.position, (Target.position - transform.position)*5f);

        //�뽬���� ����
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AdDistance * transform.localScale.x);

        //�������� ����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopdistance * transform.localScale.x);
    }


    private void CalcDistance()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask);
        if (colls.Length > 0 /*&& !�÷��̾� ���׾��ٸ�~*/)
        {
            Target = colls[0].transform;
            distance = Vector3.Distance(Target.position, transform.position);
            //Debug.Log($"Ÿ�ٰŸ� : {distance}");
            transform.LookAt(new Vector3(Target.position.x,0, Target.position.z));
        }
        else
        {
            Debug.Log("���� �� Ÿ�� ����");
        }
    }

    private void NaviTarget(float distance)
    {
        if (distance <= detectRange && distance >= stopdistance * transform.localScale.x)
        {
            //�÷��̾� ã�ư��a
            agent.stoppingDistance = stopdistance * transform.localScale.x;
            agent.SetDestination(Target.position);
            agent.isStopped = false;
            Debug.Log("����");
        }
        else if (isSlide)
        {
            agent.isStopped = true;
            Debug.Log("�����̵� ���ݳ�");
        }
        else
        {
            agent.isStopped = true;
            Debug.Log("�ٿԾ�");
        }
    }

    private IEnumerator SelectPatternCo(float distance)
    {
        // ü���� �� ���ϸ� ���������آa
        if (currentHP <= (MaxHP * 0.5f))
        {
            isAngry = true;
            HulkAnimator.SetBool("isAngry", isAngry);  
        }

        if (distance > stopdistance * transform.localScale.x * 2 && distance <= AdDistance * transform.localScale.x)
        {
            //�����̵��آa
            SlideAttack();
            Debug.Log("slide");
        }
        else if (distance <= stopdistance * transform.localScale.x)
        {
            //���������آa
            Attack();
            Debug.Log("attack");
        }
        yield return new WaitForSeconds(delayTime);
        isNormal = false;
        isStrong = false;
        isGroggy = false;
        isGroggyHit = false;
        isAngry = false;
        isSlide = false;
    }

    private void SlideAttack()
    {
        isSlide = true;
        HulkAnimator.SetBool("isSlide", isSlide);
        agent.destination = (new Vector3(Target.position.x, 0, Target.position.z)- new Vector3(transform.position.x, 0, transform.position.z)).normalized*AdDistance*transform.localScale.x;
        //transform.position = Vector3.MoveTowards(transform.position, Vector3.forward, AdDistance*transform.localScale.x);
        //������ �ּ���
        if (distance < stopdistance * transform.localScale.x*2)
        {
            isSlide = false;
            HulkAnimator.SetBool("isSlide", isSlide);
        }
    }

    private void Attack()
    {
        int SelectType = Random.Range(0, 4);
        int AttackNum = Random.Range(0, 2);
        switch (SelectType)
        {
            case 0:
            case 1:
            case 2:
                isNormal = true;
                HulkAnimator.SetBool("isNormal", isNormal);
                HulkAnimator.SetInteger("NormalNum", AttackNum);
                isNormal = false;
                break;
            case 3:
                isStrong = true;
                HulkAnimator.SetBool("isStrong", isStrong);
                HulkAnimator.SetInteger("StrongNum", AttackNum);
                isStrong = false;
                break;
        }
    }








}
