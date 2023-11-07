using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaPattern : MonoBehaviour
{
    public Transform centerTransform;
    public int rowCount = 3;
    public GameObject objectToSpawn;
    public float spawnDirection = 10f;
    public List<List<GameObject>> objectpool;


    Vector3[] directions;
    // Start is called before the first frame update
    void Start()
    {
        objectpool = new List<List<GameObject>>();
        centerTransform = transform;
        directions = new Vector3[8]
            {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            Vector3.forward + Vector3.left, Vector3.forward + Vector3.right,
            Vector3.back + Vector3.left, Vector3.back + Vector3.right
            };
        for (int i = rowCount; i < 0; i--)
        {
            List<GameObject> objectPool = new List<GameObject>();
            foreach (Vector3 direction in directions)
            {
                // �߽� ��ġ���� ���� ���͸� ����Ͽ� ���ο� ��ġ ���
                Vector3 spawnPosition = centerTransform.position + direction * (spawnDirection*i);

                // ������Ʈ ����
                GameObject newobj = Instantiate(objectToSpawn, spawnPosition, Quaternion.Euler(-90f, 0, 0));
                newobj.SetActive(false);
                objectPool.Add(newobj);
            }
            objectpool.Add(objectPool);
        }
    }
}
