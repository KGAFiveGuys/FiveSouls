using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Cinemachine_Camera_Controll : MonoBehaviour
{
    private GameObject player;
    private CinemachineVirtualCamera cinemachineVirtual;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        print("Awak : Z");
        TryGetComponent(out cinemachineVirtual);
    }
    private void Start()
    {
        cinemachineVirtual.Follow = player.transform;
       // cinemachineVirtual.LookAt = player.transform;
    }

}
