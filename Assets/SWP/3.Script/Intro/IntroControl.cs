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
    private bool isStart = false; //인트로 카메라 시작하고 기도하는거 1초정도 보여주면 true 가자
    private bool isGen = false; //기도자와 박스콜라이더 충돌하면 파티클 켜주고 파티클 끝나면 트루해줘서 애니메이션 재생하고 오브젝트 꺼버리자
    private float flowTime = 0f;
    private float Timer = 3f;

    private void Awake()
    {
        cremoa = FindObjectOfType<Cremoa>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        //플레이어가 입구쪽 콜라이더랑 트리거되면 start값 true로 받아온다
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
