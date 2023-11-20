using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroControl : MonoBehaviour
{
    [SerializeField] private Animator HulkAni;
    [SerializeField] private Animator IntroAni;
    [SerializeField] private Animator PrayerAni;
    [SerializeField] private GameObject HulkBoss;
    [SerializeField] private GameObject BossBall;
    [SerializeField] private GameObject SparkEffect;
    private Cremoa cremoa;
    private PlayerController playerController;
    private bool isStart = false; //��Ʈ�� ī�޶� �����ϰ� �⵵�ϴ°� 1������ �����ָ� true ����
    private bool isGen = false; //�⵵�ڿ� �ڽ��ݶ��̴� �浹�ϸ� ��ƼŬ ���ְ� ��ƼŬ ������ Ʈ�����༭ �ִϸ��̼� ����ϰ� ������Ʈ ��������
    private float flowTime = 0f;
    private float Timer = 3f;

    private void Awake()
    {
        cremoa = FindObjectOfType<Cremoa>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        //�÷��̾ �Ա��� �ݶ��̴��� Ʈ���ŵǸ� start�� true�� �޾ƿ´�
        if (cremoa.isOver)
        {
            StartCoroutine(PrayEndCo());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Prayer"))
        {
            other.gameObject.SetActive(false);
            StartCoroutine(BossGenCo());
        }
    }

    private IEnumerator PrayEndCo()
    {
        var startScale = Vector3.one;
        var endScale = Vector3.one * 20;
        while (Timer > flowTime)
        {
            flowTime += Time.deltaTime;
            BossBall.transform.localScale = Vector3.Lerp(startScale, endScale, flowTime / Timer);
            yield return null;
        }
        yield return new WaitForSeconds(Timer * 2f);
        PrayerAni.SetBool("isStart", cremoa.isOver);
    }

    private IEnumerator BossGenCo()
    {
        SparkEffect.SetActive(true);
        isGen = true;

        yield return new WaitForSeconds(1.5f);        
        IntroAni.SetBool("isGen", isGen);

        yield return new WaitForSeconds(2f);
        HulkBoss.SetActive(true);
        HulkAni.SetTrigger("isBorn");
        BossBall.SetActive(false);
        cremoa.isOver = true;
        playerController.ControlState = ControlState.Controllable;

         yield return new WaitForSeconds(3f);
        HulkAni.ResetTrigger("isBorn");
    }

}
