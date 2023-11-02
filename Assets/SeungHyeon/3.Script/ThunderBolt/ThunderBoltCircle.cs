using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderBoltCircle : MonoBehaviour
{
    public GameObject ThunderBoltCirclePrifebs;
    public int poolsize = 3;
    public List<GameObject> objectpool = new List<GameObject>();

    private void Start()
    {
        for(int i = 0; i < poolsize; i++)
        {
            GameObject newObj = Instantiate(ThunderBoltCirclePrifebs,gameObject.transform);
            newObj.SetActive(false);
            objectpool.Add(newObj);
        }
    }
    GameObject GetObjectFromPool()
    {
        foreach (GameObject obj in objectpool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return null;
    }
}
