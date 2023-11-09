using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fury : MonoBehaviour
{
    //    - �г� ����
    //   �г�� �ڵ����� ������ ���ö����.
    //   ������ �г밡 �پ��� ��� �������� �� ����� ��.
    //   �г밡 ���� ���� Flag ���� true�� �����ؾ� ��.

    //- �г밡 ���� á���� �˸��� Flag (Bool Property) �����

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
