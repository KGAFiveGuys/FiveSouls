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
    [SerializeField] private float ThunderDelay = 0.5f;
    [SerializeField] private ThunderBoltCircle thunderBoltCircle;
    [SerializeField] private GameObject Fireball_Spawner;
    [SerializeField] private FireBallSpawner fireBallSpawner;
    [SerializeField] private float BackwardForce = 100f;
    [SerializeField] private Rigidbody Wizard_rb;

    [Header("이펙트")]
    [SerializeField] private AttackEffect[] Attack_effect;
    [SerializeField] private GameObject[] FireBall;
    [SerializeField] private ParticleSystem CurrnetEffect;
    [SerializeField] private GameObject ReadyEffect;


    [Header("위자드 상태창")]
    [SerializeField] public Wizardinfo wizardinfo;

    private void Awake()
    {
        wizardinfo.ChaseTarget = FindObjectOfType<playerController>().gameObject;
        fireBallSpawner = FindObjectOfType<FireBallSpawner>();
        wizardinfo.status = Status.Idle;
        TryGetComponent(out Wizard_anim);
        TryGetComponent(out Wizard_rb);
        thunderBoltCircle = FindObjectOfType<ThunderBoltCircle>();
    }
    private void Update()
    {
        CheckPlayerPosition();

        if (wizardinfo.status.Equals(Status.Ready))
        {
            AttackTime += Time.deltaTime;
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
        SelectPattern(AttackPlayer);
        CurrnetEffect.Stop();
    }
    private IEnumerator UseThunderbolt()
    {
        int count = 0;
        float time = 0;
        float Lasttime = 0;
        while(count < 3)
        {
            time += Time.deltaTime;
            if(time >= Lasttime + ThunderDelay)
            {
                //Instantiate(Thunderbolt, wizardinfo.ChaseTarget.transform);
                thunderBoltCircle.objectpool[count].transform.position = new Vector3(wizardinfo.ChaseTarget.transform.position.x, wizardinfo.ChaseTarget.transform.position.y+0.2f, wizardinfo.ChaseTarget.transform.position.z);
                thunderBoltCircle.objectpool[count].SetActive(true);
                Lasttime = time;
                count++;
            }
            yield return null;
        }
    }
    private void ClosePattern()
    {
        Debug.DrawRay(transform.position, -transform.forward * 20f, Color.blue);
        if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit hit, 20f))
        {
            Debug.Log("벽있음");
        }
        else
        {
            StartCoroutine(BackStep());
        }
    }

    private void SelectPattern(int pattern)
    {
        switch(pattern)
        {
            case 0:
                ClosePattern();
                return;
            case 1:
                StartCoroutine(fireBallSpawner.CreateFireBall());
                return;
            case 2:
                StartCoroutine(UseThunderbolt());
                return;
        }    
    }
    private IEnumerator BackStep()
    {
        Vector3 Backward_Movement = -transform.forward * BackwardForce;
        Wizard_rb.AddForce(Backward_Movement);
        yield return null;
    }
}
