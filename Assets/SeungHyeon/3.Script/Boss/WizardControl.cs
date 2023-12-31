using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Effect
{
    Lightning = 0,
    Fire,
    Ground,
    Frost
}
public enum Status
{
    Idle = 0,
    Ready,
    Attack,
    MegaPattern,
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
    [SerializeField] private GameObject BossPrefab;
    private float Dist;//플레이어와 보스와의 거리
    public Animator Wizard_anim;
    private float AttackTime = 0;
    [SerializeField] private AudioClip BossClip;
    [SerializeField] private AudioClip VillageClip;
    [SerializeField] private AudioSource bgm;
    [SerializeField] private Health health; 
    [SerializeField]private float ThunderDelay = 0.5f;
    [SerializeField]private ThunderBoltCircle thunderBoltCircle;
    [SerializeField]private ParticleSystem shadowburst;
    [SerializeField]private GameObject Fireball_Spawner;
    [SerializeField]private GameObject FrostMissilePrefab;
    [SerializeField]private GameObject RightHand;
    [SerializeField]private GameObject FrostSpawner;
    [SerializeField]private FireBallSpawner fireBallSpawner;
    [SerializeField]public MegaPattern megapattern;
    [SerializeField] private float BackwardForce = 100f;
    [SerializeField] private Rigidbody Wizard_rb;
    [SerializeField] private GameObject MagicImage;
    [SerializeField] private Portal portal;

    [Header("이펙트")]
    [SerializeField] private AttackEffect[] Attack_effect;
    [SerializeField] private GameObject[] FireBall;
    [SerializeField] private ParticleSystem CurrnetEffect;
    [SerializeField] private GameObject ReadyEffect;


    [Header("위자드 상태창")]
    [SerializeField] public Wizardinfo wizardinfo;

    private PlayerHUDController playerHUDController;

    private void Awake()
    {
        playerHUDController = FindObjectOfType<PlayerHUDController>();
        portal = FindObjectOfType<Portal>();
        health = GetComponentInParent<Health>();
        wizardinfo.ChaseTarget = FindObjectOfType<PlayerController>().gameObject;
        fireBallSpawner = FindObjectOfType<FireBallSpawner>();
        megapattern = FindObjectOfType<MegaPattern>();
        wizardinfo.status = Status.Idle;
        TryGetComponent(out Wizard_anim);
        TryGetComponent(out Wizard_rb);
        thunderBoltCircle = FindObjectOfType<ThunderBoltCircle>();
        MagicImage = Instantiate(MagicImage, FindObjectOfType<PlayerHUDController>().transform);
        MagicImage.SetActive(false);
        ReadyEffect.SetActive(false);
    }

    private void Start()
    {
        health.OnDead += playerHUDController.ShowEnemyDied;
    }

    private void OnEnable()
    {
        wizardinfo.ChaseTarget.GetComponent<Health>().OnRevive += RespwanBoss;
    }

    private void OnDisable()
    {
        wizardinfo.ChaseTarget.GetComponent<Health>().OnRevive -= RespwanBoss;
    }

    private void Update()
    {
        CheckPlayerPosition();
        if(health.CurrentHP <= 0 && !wizardinfo.status.Equals(Status.Death))
        {
            Die();
        }

        if (wizardinfo.status.Equals(Status.Ready))
        {
            var targetPos = wizardinfo.ChaseTarget.transform.position;
            var lookAtPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            transform.LookAt(lookAtPos);
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
        if(Dist <= 20f && wizardinfo.status == Status.Idle)
        {
            wizardinfo.status = Status.Ready;
            Wizard_anim.SetBool("Ready",true);
            ReadyEffect.SetActive(true);
            MagicImage.SetActive(true);
            bgm.Stop();
            SFXManager.Instance.OnBossFight3_Started();
        }
        
    }
    public int SelectPattern()
    {
        int rand = 0;
        rand = Random.Range(0, 4);
        return rand;
    }
    public IEnumerator AttackReady(int AttackPlayer)
    {
        CurrnetEffect = Attack_effect[AttackPlayer].Effect_Particle;
        Attack_effect[AttackPlayer].Effect_Particle.Play();
        SelectAnimation(AttackPlayer);
        AttackTime = 0;
        yield return new WaitForSeconds(2f);
        SelectPattern(AttackPlayer);
        CurrnetEffect.Stop();
    }
    private IEnumerator UseThunderbolt()
    {
        wizardinfo.status = Status.Attack;
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
        wizardinfo.status = Status.Ready;
    }
    private void ClosePattern()
    {
        var collisionModule = shadowburst.collision;
        collisionModule.enabled = true;
        shadowburst.gameObject.SetActive(true);
        Debug.DrawRay(transform.position, -transform.forward * 20f, Color.blue);
        //if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit hit, 20f))
        //{
        //    Debug.Log("벽있음");
        //}
        //else
        //{
        //    StartCoroutine(BackStep());
        //}
    }
    private void SelectAnimation(int pattern)
    {
      switch(pattern)
        {
            case 0:
                Wizard_anim.SetTrigger("CloseBurst");
                return;
            case 1:
                Wizard_anim.SetTrigger("FireBall");
                return;
            case 2:
                Wizard_anim.SetTrigger("Lightning");
                return;
            default:
                return;
        }
    }
    private void SelectPattern(int pattern)
    {
        switch(pattern)
        {
            case 0:
                AttackAlarm.Instance.YellowAlarm();
                ClosePattern();
                return;
            case 1:
                AttackAlarm.Instance.RedAlarm();
                StartCoroutine(fireBallSpawner.CreateFireBall());
                return;
            case 2:
                AttackAlarm.Instance.RedAlarm();
                StartCoroutine(UseThunderbolt());
                //StartCoroutine(megapattern.MegaThunderPatternUse());
                return;
            case 3:
                AttackAlarm.Instance.YellowAlarm();
                UseFrostMissile();
                return;
        }    
    }
    private IEnumerator BackStep()
    {
        Vector3 Backward_Movement = -transform.forward * BackwardForce;
        Wizard_rb.AddForce(Backward_Movement);
        yield return null;
    }
    public void FrostMissile()
    {
        Instantiate(FrostMissilePrefab, RightHand.transform.position,Quaternion.identity ,FrostSpawner.transform);
    }
    private void UseFrostMissile()
    {
        Wizard_anim.SetTrigger("Frost");
    }
    public void Die()
    {
        wizardinfo.status = Status.Death;
        bgm.Stop();
        Wizard_anim.SetBool("Ready", false);
        Wizard_anim.SetTrigger("Die");
        ReadyEffect.SetActive(false);
        MagicImage.SetActive(false);
        portal.Portal_Obj.SetActive(true);
        portal.Portal_col.enabled = true;
    }
    public void StartStormMove()
    {
        megapattern.MoveStorm();
    }
    public void StopStormMove()
    {
        StartCoroutine(megapattern.StopMoveStorm_Co());
    }
    public void RespwanBoss()
    {
        Destroy(MagicImage);
        bgm.clip = VillageClip;
        bgm.Play();
        Instantiate(BossPrefab, this.transform.parent.position , Quaternion.Euler(0, -90, 0));
        Destroy(this.transform.parent.gameObject);
    }
}
