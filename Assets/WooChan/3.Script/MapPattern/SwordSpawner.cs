using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSpawner : MonoBehaviour
{
    [SerializeField] private GameObject Sword1;
    [SerializeField] private GameObject Sword2;
    [SerializeField] private GameObject Sword3;
    [SerializeField] private GameObject Sword4;
    [SerializeField] private GameObject Sword5;

    [SerializeField] private float StartTime = 0f;
    [SerializeField] private float SpawnTime = 10f;

    private void Start()
    {

    }

    private void Update()
    {
        StartTime += Time.deltaTime;
        if (StartTime > SpawnTime)
        {
            if (!Sword1.activeSelf)
            {
                Sword1.SetActive(true);
            }
            else if (!Sword2.activeSelf)
            {
                Sword2.SetActive(true);
            }
            else if (!Sword3.activeSelf)
            {
                Sword3.SetActive(true);
            }
            else if (!Sword4.activeSelf)
            {
                Sword4.SetActive(true);
            }
            else if (!Sword5.activeSelf)
            {
                Sword5.SetActive(true);
            }
            else
            {
                Sword1.SetActive(false);
                Sword2.SetActive(false);
                Sword3.SetActive(false);
                Sword4.SetActive(false);
                Sword5.SetActive(false);
            }
            StartTime = 0f;

        }
    }

}

