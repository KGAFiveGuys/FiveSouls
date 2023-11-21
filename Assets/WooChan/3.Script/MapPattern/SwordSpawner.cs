using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSpawner : MonoBehaviour
{
    [SerializeField] private Health _Health;
    [SerializeField] private Health _BossHealth;

    [SerializeField] private GameObject Sword1;
    [SerializeField] private GameObject Sword2;
    [SerializeField] private GameObject Sword3;
    [SerializeField] private GameObject Sword4;
    [SerializeField] private GameObject Sword5;

    [SerializeField] private float StartTime = 0f;
    [SerializeField] private float SpawnTime = 10f;

    [SerializeField] private GameObject[] Fires;
    [SerializeField] private GameObject[] Explosions;
    [SerializeField] private Renderer[] Meshs;
    [SerializeField] private Collider[] ExplosionCollider;


    public bool startFire { get; private set; } = false;
    public bool startExplosion { get; private set; } = false;

    private float FireTime = 0f;
    [SerializeField] private float StartFire = 5f;
    private float ExplosionTime = 0f;
    [SerializeField] private float StartExplosion = 5f;
    private float DeleteTime = 0f;


    private void Update()
    {
        if (_Health.CurrentHP > 0 && _BossHealth.CurrentHP > 0)
        {
            if (startFire)
            {
                for (int i = 0; i < Fires.Length; i++)
                {
                    Fires[i].SetActive(true);
                }
            }
            if (startExplosion)
            {
                for (int i = 0; i < Explosions.Length; i++)
                {
                    Explosions[i].SetActive(true);
                    Meshs[i].enabled = false;
                }
                DeleteTime += Time.deltaTime;
                if (DeleteTime > 0.8f)
                {
                    Sword1.SetActive(false);
                    Sword2.SetActive(false);
                    Sword3.SetActive(false);
                    Sword4.SetActive(false);
                    Sword5.SetActive(false);
                    StartTime = 0f;
                    FireTime = 0f;
                    ExplosionTime = 0f;
                    DeleteTime = 0f;
                    startFire = false;
                    startExplosion = false;
                    for (int i = 0; i < Fires.Length; i++)
                    {
                        Fires[i].SetActive(false);
                        Explosions[i].SetActive(false);
                        Meshs[i].enabled = true;
                    }
                }
            }

            if (!Sword5.activeSelf)
            {
                StartTime += Time.deltaTime;
                if (StartTime > SpawnTime)
                {
                    if (!Sword1.activeSelf)
                    {
                        Sword1.SetActive(true);
                        ExplosionCollider[0].enabled = true;
                    }
                    else if (!Sword2.activeSelf)
                    {
                        Sword2.SetActive(true);
                        ExplosionCollider[1].enabled = true;
                    }
                    else if (!Sword3.activeSelf)
                    {
                        Sword3.SetActive(true);
                        ExplosionCollider[2].enabled = true;
                    }
                    else if (!Sword4.activeSelf)
                    {
                        Sword4.SetActive(true);
                        ExplosionCollider[3].enabled = true;
                    }
                    else if (!Sword5.activeSelf)
                    {
                        Sword5.SetActive(true);
                        ExplosionCollider[4].enabled = true;
                    }
                    StartTime = 0f;
                }
            }

            if (Sword5.activeSelf)
            {
                FireTime += Time.deltaTime;
                if (FireTime > StartFire)
                {
                    startFire = true;
                    ExplosionTime += Time.deltaTime;
                    if (ExplosionTime > StartExplosion)
                    {
                        startExplosion = true;
                    }
                }
            }


        }
    }
}

