using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Btn : MonoBehaviour
{
    [SerializeField] private GameObject boss_btn;
    [SerializeField] private GameObject boss;
    public Material objectMaterial;
    private void Awake()
    {
        objectMaterial = TryGetComponent(out Renderer renderer) ? renderer.material : null;
    }

    public static bool Boss_btn = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            print("켜졌나?");

            Boss_btn = true;
            if (Boss_btn)
            {
                // 부모 오브젝트에서 자식 오브젝트를 찾아서 활성화합니다.
                boss_btn.SetActive(true);
                boss.SetActive(true);
                objectMaterial.color = Color.red;

            }

        }
    }
}