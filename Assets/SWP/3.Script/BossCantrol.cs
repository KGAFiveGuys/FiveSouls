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
    [SerializeField] private AnimationClip[] AniClips;
    private AnimationClip currentAniClip;
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
    private bool OnlyOne = true;
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

    private void OnDrawGizmos()
    {
        //Ž������
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        if (Target != null)
        {
            Gizmos.DrawLine(transform.position, new Vector3(Target.position.x, 0, Target.position.z));
        }

        //�뽬���� ����
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AdDistance * transform.localScale.x);

        //�������� ����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopdistance * transform.localScale.x);
    }

    private void Start()
    {
        currentHP = MaxHP;
        TryGetComponent(out agent);
        TryGetComponent(out HulkAnimator);
        AniClips = transform.GetComponents<AnimationClip>();
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

    private void CalcDistance()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask);
        if (colls.Length > 0 /*&& !�÷��̾� ���׾��ٸ�~*/)
        {
            Target = colls[0].transform;
            Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
            Vector3 currentTF = new Vector3(transform.position.x, 0, transform.position.z);
            distance = Vector3.Distance(targetTF, currentTF);
            //Debug.Log($"Ÿ�ٰŸ� : {distance}");
            //transform.LookAt(new Vector3(Target.position.x,0, Target.position.z));
        }
        else
        {
            Debug.Log("���� �� Ÿ�� ����");
        }
    }

    private void NaviTarget(float distance)
    {
        if (isTarget)
        {
            //�÷��̾� ã�ư��a
            agent.stoppingDistance = stopdistance * transform.localScale.x;
            agent.SetDestination(new Vector3(Target.position.x,0, Target.position.z));
            agent.isStopped = false;
        }
        //else if (isSlide || isAngry || isNormal || isStrong)
        //{
        //    agent.isStopped = true;
        //    Debug.Log("���� �� AI����!");
        //}
        else
        {
            agent.isStopped = true;
        }
    }

    private IEnumerator SelectPatternCo(float distance)
    {
        // ü���� �� ���ϸ� ���������آa
        if (currentHP <= (MaxHP * 0.5f) && OnlyOne)
        {
            OnlyOne = false;
            agent.ResetPath();
            Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
            Vector3 currentTF = new Vector3(transform.position.x, 0, transform.position.z);
            transform.position = Vector3.MoveTowards(currentTF, targetTF, 1.5f * Time.deltaTime);
            AngryAttack();
        }

        if (distance > stopdistance * transform.localScale.x * 2 && distance <= AdDistance * transform.localScale.x)
        {
            //�����̵��آa
            SlideAttack();
        }
        else if (distance <= stopdistance * transform.localScale.x)
        {
            //���������آa
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
            Debug.Log("attack");
        }

        yield return null;/*new WaitForSeconds(delayTime);*/
        isSlide = false;
        isGroggy = false;
        isGroggyHit = false;
    }

    private void SlideAttack()
    {
        for (int i = 0; i < AniClips.Length; i++)
        {
            if (AniClips[i].name == "Sliding")
            {
                currentAniClip = AniClips[i];
                break;
            }            
        }
        Debug.Log("slide");
        isSlide = true;
        agent.ResetPath();
        Vector3 targetTF = new Vector3(Target.position.x, 0, Target.position.z);
        Vector3 currentTF = new Vector3(transform.position.x, 0, transform.position.z);
        HulkAnimator.SetBool("isSlide", isSlide);
        transform.position = Vector3.MoveTowards(currentTF, targetTF.normalized*AdDistance*transform.localScale.x, currentAniClip.length * Time.deltaTime);
        //agent.destination = new Vector3(Target.position.x, 0, Target.position.z).normalized * AdDistance * transform.localScale.x;
        //������ �ּ���
        TurnOff(currentAniClip.length, isSlide);        
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
                transform.position = 
                //isNormal = false;
                //HulkAnimator.SetBool("isNormal", isNormal);
                break;
            case 3:
                isStrong = true;
                HulkAnimator.SetBool("isStrong", isStrong);
                HulkAnimator.SetInteger("StrongNum", AttackNum);
                //isStrong = false;
                //HulkAnimator.SetBool("isStrong", isStrong);
                break;
        }
    }

    private void AngryAttack()
    {
        isAngry = true;
        HulkAnimator.SetBool("isAngry", isAngry);

        //isAngry = false;
        //HulkAnimator.SetBool("isAngry", isAngry);
    }

    private bool TurnOff(float time, bool what)
    {
        float needTime = 0;
        needTime += Time.deltaTime;
        if (needTime >= time)
        {
            what = false;
        }
        return what;
    }






}
