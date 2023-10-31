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
    public float Health = 100;
    public GameObject ChaseTarget;
}
[System.Serializable]
public class AttackEffect
{
    public Effect effect;
    public GameObject Effect_Obj;
}

public class WizardControl : MonoBehaviour
{

    private float Dist;//�÷��̾�� �������� �Ÿ�
    private Animator Wizard_anim;

    [Header("����Ʈ")]
    [SerializeField] private AttackEffect[] Attack_effect;

    [Header("���ڵ� ����â")]
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
    }
    public void CheckPlayerPosition()
    {
        Dist = Vector3.Distance(wizardinfo.ChaseTarget.transform.position, transform.position);
        if(Dist <= 5f && wizardinfo.status == Status.Idle)
        {
            Wizard_anim.SetBool("idle_combat", true);
            wizardinfo.status = Status.Ready;
        }
    }
    public IEnumerator AttackReady()
    {
        int randAttack = Random.Range(0, 2);
        Attack_effect[randAttack].Effect_Obj.SetActive(true);
        yield return new WaitForSeconds(2f);
        //���� �ڷ�ƾ �ֱ�
    }

}
