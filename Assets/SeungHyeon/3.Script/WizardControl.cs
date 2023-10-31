using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Effect
{
    Lightning = 0,
    Fire,
    Ground
}
public enum Status
{
    Idle = 0,
    Ready,
    Attack,
    Death
}
[System.Serializable]
public class Wizardinfo
{
    public Status status;
    public float Health = 100f;
    public GameObject ChaseTarget;
}
[System.Serializable]
public class AttackEffect
{
    public Effect effect;
    public ParticleSystem Effect_Particle;
}

public class WizardControl : MonoBehaviour
{

    private float Dist;//플레이어와 보스와의 거리
    private Animator Wizard_anim;
    private float AttackTime = 0;

    [Header("이펙트")]
    [SerializeField] private AttackEffect[] Attack_effect;
    [SerializeField] private ParticleSystem CurrnetEffect;
    [SerializeField] private GameObject ReadyEffect;

    [Header("위자드 상태창")]
    [SerializeField] private Wizardinfo wizardinfo;

    private void Awake()
    {
        wizardinfo.ChaseTarget = FindObjectOfType<playerController>().gameObject;
        wizardinfo.status = Status.Idle;
        TryGetComponent(out Wizard_anim);
    }
    private void Update()
    {
        CheckPlayerPosition();
        if(wizardinfo.status.Equals(Status.Ready))
        {
            AttackTime += Time.deltaTime;
            Debug.Log(AttackTime);
            if (AttackTime >= 5f)
            {
                Debug.Log("실행");
               StartCoroutine(AttackReady(SelectPattern()));
            }
        }
    }
    public void CheckPlayerPosition()
    {
        Dist = Vector3.Distance(wizardinfo.ChaseTarget.transform.position, transform.position);
        if(Dist <= 5f && wizardinfo.status == Status.Idle)
        {
            Wizard_anim.SetBool("idle_combat", true);
            wizardinfo.status = Status.Ready;
            ReadyEffect.SetActive(true);
        }
        
    }
    public int SelectPattern()
    {
        int rand = 0;
        if (Dist <= 8f)
        {
            return rand;
        }
        rand = Random.Range(1, 3);
        return rand;
    }
    public IEnumerator AttackReady(int AttackPlayer)
    {
        CurrnetEffect = Attack_effect[AttackPlayer].Effect_Particle;
        Attack_effect[AttackPlayer].Effect_Particle.Play();
        AttackTime = 0;
        yield return new WaitForSeconds(3f);
        Wizard_anim.SetTrigger("Attack");
        CurrnetEffect.Stop();
    }
}
