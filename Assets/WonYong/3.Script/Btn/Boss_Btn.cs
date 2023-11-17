using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Btn : MonoBehaviour
{
    [SerializeField] private GameObject boss_btn;
    [SerializeField] private GameObject boss;
    [Header("bossOn=>rockpool Off")]
    [SerializeField] private GameObject RockPool;
    [SerializeField] private GameObject[] Particle;
    private ParticleSystem particleSystem_;
    public Material objectMaterial;
    private void Awake()
    {
        objectMaterial = TryGetComponent(out Renderer renderer) ? renderer.material : null;
        particleSystem_ = GetComponent<ParticleSystem>();
    }

    public static bool Boss_btn = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            Boss_btn = true;
            if (Boss_btn)
            {
                boss_btn.SetActive(true);
                boss.SetActive(true);
                RockPool.SetActive(false);
                objectMaterial.color = Color.red;
                if (particleSystem_ != null)
                {
                    ParticleSystem.MainModule mainModule = particleSystem_.main;

                    // 색상 변경
                    mainModule.startColor = Color.red; 
                }

            }

        }
    }



    

    private void Start()
    {

    }

    private void Update()
    {

    }
}