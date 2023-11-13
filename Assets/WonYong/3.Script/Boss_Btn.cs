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
            print("������?");

            Boss_btn = true;
            if (Boss_btn)
            {
                // �θ� ������Ʈ���� �ڽ� ������Ʈ�� ã�Ƽ� Ȱ��ȭ�մϴ�.
                boss_btn.SetActive(true);
                boss.SetActive(true);
                objectMaterial.color = Color.red;

            }

        }
    }
}