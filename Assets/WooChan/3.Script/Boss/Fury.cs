using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fury : MonoBehaviour
{
    //    - 분노 관리
    //   분노는 자동으로 서서히 차올라야함.
    //   맞으면 분노가 줄어들고 잠시 차오르는 게 멈춰야 함.
    //   분노가 가득 차면 Flag 값을 true로 변경해야 함.

    //- 분노가 가득 찼음을 알리는 Flag (Bool Property) 만들기

    public bool Flag { get; private set; }

    private float FuryGauge = 100f;
    private float CurrentFuryGauge = 0f;
    private float RecoveryGauge = 0.1f;
    private float DecreaseFuryValue;

    private void Awake()
    {
        Flag = false;
    }

    private void FixedUpdate()
    {
        CurrentFuryGauge += RecoveryGauge * Time.deltaTime;
    }


    private IEnumerator StopRecovery()
    {
        float WaitTime = 1.5f;
        yield return new WaitForSeconds(WaitTime);
        yield break;
    }

    private void HitType(string type)
    {
        if (type == "HitWeak")
        {
            DecreaseFuryValue = 1f;
        }
        else if (type == "HitStrong")
        {
            DecreaseFuryValue = 2f;
        }
        else
        {
            DecreaseFuryValue = 0f;
        }
    }

    private void DecreaseGauge()
    {
        if(CurrentFuryGauge < DecreaseFuryValue)
        {
            CurrentFuryGauge = 0f;
        }
        else
        {
            CurrentFuryGauge -= DecreaseFuryValue;
        }
    }


}
