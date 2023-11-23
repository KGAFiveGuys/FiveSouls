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

    [SerializeField] private GameObject Boss_prefab;
    [SerializeField] private Vector3 positionToTeleport = new Vector3(401.600006f, -701.710022f, 1016.40002f);
    [SerializeField] private Vector3 rotationEulerAngles = new Vector3(401.600006f, -192.521f, 1016.40002f);
    private Quaternion RotationToTeleport;
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
                SFXManager.Instance.OnBossFight2_Started();
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
}