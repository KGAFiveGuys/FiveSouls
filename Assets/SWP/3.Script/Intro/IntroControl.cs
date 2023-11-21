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
    private bool isStart = false;
    private bool isGen = false;
    private float Timer = 3f;
    private float flowTime = 0f;

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
            if (!isStart)
            {
                isStart = true;
                StartCoroutine(PrayEndCo());
            }
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
        BossBall.SetActive(true);
        yield return new WaitForSeconds(2f);
        IntroAni.SetBool("isPray", cremoa.isOver);

        yield return new WaitForSeconds(5f);
        PrayerAni.SetBool("isStart", cremoa.isOver);
    }

    private IEnumerator BossGenCo()
    {
        SparkEffect.SetActive(true);
        isGen = true;

        yield return new WaitForSeconds(1.5f);        
        IntroAni.SetBool("isGen", isGen);

        yield return new WaitForSeconds(1.9f);
        HulkBoss.SetActive(true);
        HulkAni.SetTrigger("isBorn");
        BossBall.SetActive(false);
        playerController.ControlState = ControlState.Controllable;

         yield return new WaitForSeconds(3f);
        HulkAni.ResetTrigger("isBorn");
    }

}
