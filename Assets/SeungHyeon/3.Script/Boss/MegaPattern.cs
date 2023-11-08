using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaPattern : MonoBehaviour
{
    public Transform centerTransform;
    public int rowCount = 3;
    public int colCount = 8;
    public GameObject ThunderToSpawn;
    public GameObject FireToSpawn;
    public GameObject StormToSpawn;
    public float spawnDirection = 10f;
    public List<List<GameObject>> Thunderobjectpool;
    public List<List<GameObject>> Fireobjectpool;
    public List<GameObject> StormObjectpool;


    Vector3[] Thunderdirections;
    Vector3[] Firedirections;
    Vector3 Stormdirections;
    // Start is called before the first frame update
    void Start()
    {
        Thunderobjectpool = new List<List<GameObject>>();
        Fireobjectpool = new List<List<GameObject>>();
        centerTransform = transform;
        Thunderdirections = new Vector3[8]
            {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            Vector3.forward + Vector3.left, Vector3.forward + Vector3.right,
            Vector3.back + Vector3.left, Vector3.back + Vector3.right
            };
        Firedirections = new Vector3[8]
        {
            Quaternion.Euler(0, 0, 0) * Vector3.forward,
            Quaternion.Euler(0, 45, 0) * Vector3.forward,
            Quaternion.Euler(0, 90, 0) * Vector3.forward,
            Quaternion.Euler(0, 135, 0) * Vector3.forward,
            Quaternion.Euler(0, 180, 0) * Vector3.forward,
            Quaternion.Euler(0, 225, 0) * Vector3.forward,
            Quaternion.Euler(0, 270, 0) * Vector3.forward,
            Quaternion.Euler(0, 315, 0) * Vector3.forward
        };
        for (int i = rowCount; i > 0; i--)
        {
            List<GameObject> objectPool = new List<GameObject>();
            foreach (Vector3 direction in Thunderdirections)
            {
                // 중심 위치에서 방향 벡터를 사용하여 새로운 위치 계산
                Vector3 spawnPosition = centerTransform.position + direction * (spawnDirection * i);

                // 오브젝트 생성
                GameObject newobj = Instantiate(ThunderToSpawn, gameObject.transform);
                newobj.transform.position = spawnPosition;
                newobj.SetActive(false);
                objectPool.Add(newobj);
            }
            Thunderobjectpool.Add(objectPool);
        }
        for (int i = rowCount; i > 0; i--)
        {
            List<GameObject> objectPool = new List<GameObject>();
            foreach (Vector3 direction in Firedirections)
            {
                Vector3 spawnPosition = centerTransform.position + direction * (spawnDirection * i);
                GameObject newobj = Instantiate(FireToSpawn, gameObject.transform);
                newobj.transform.position = spawnPosition;
                newobj.SetActive(false);
                objectPool.Add(newobj);
            }
            Fireobjectpool.Add(objectPool);
        }
        foreach (Vector3 direction in Thunderdirections)
        {
            // 중심 위치에서 방향 벡터를 사용하여 새로운 위치 계산
            Vector3 spawnPosition = centerTransform.position + direction * spawnDirection;

            // 오브젝트 생성
            GameObject newobj = Instantiate(StormToSpawn, gameObject.transform);
            newobj.transform.position = spawnPosition;
            newobj.SetActive(false);
            StormObjectpool.Add(newobj);
        }
        for(int i =0;i<StormObjectpool.Count;i++)
        {
            float yawAngle = 45 * i; // Yaw (Y 축) 각도 계산
            Vector3 eulerRotation = new Vector3(0, yawAngle, 0); // Yaw 각도로 오일러 회전 벡터 생성
            Quaternion rotation = Quaternion.Euler(eulerRotation); // Quaternion으로 변환
            StormObjectpool[i].transform.rotation = rotation; // 로테이션 설정
        }
    }
    GameObject GetObjectFromPool(int poolIndex)
    {
        foreach (GameObject obj in Thunderobjectpool[poolIndex])
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return null;
    }
    public IEnumerator MegaThunderPatternUse()
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                Thunderobjectpool[i][j].SetActive(true);
            }
            yield return new WaitForSeconds(0.5f);
        }
        StartCoroutine(MegaFirePatternUse());
    }
    public IEnumerator MegaFirePatternUse()
    {
        for (int i = rowCount; i > 0; i--)
        {
            for (int j = colCount; j > 0; j--)
            {
                Fireobjectpool[i-1][j-1].SetActive(true);
            }
            yield return new WaitForSeconds(0.5f);
        }
        StartCoroutine(MegaStormPatternUse());
    }
    public IEnumerator MegaStormPatternUse()
    {
        for(int i = 0;i<StormObjectpool.Count;i++)
        {
            StormObjectpool[i].SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < StormObjectpool.Count; i++)
        {
            StormObjectpool[i].SetActive(false);
        }
    }

}
