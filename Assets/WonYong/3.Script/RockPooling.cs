using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPooling : MonoBehaviour
{
    [SerializeField] private GameObject RockPrefab_Two;
    [SerializeField] private Transform pool_position;
    [SerializeField] private int count;
    List<GameObject> poolRock = new List<GameObject>();

    public static int Rock_count;
    private void Awake()
    {
        Creat_Rock_Pool(count);
        Rock_count = count;
    }

    private void Start()
    {
        StartCoroutine(Rock_Creat_co());
    }
    private void Creat_Rock_Pool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Creat_Rock();
        }
    }

    private void Creat_Rock()
    {
        var offset = new Vector3(0f, 0f, Random.Range(-50f, 50f));
        var newRock = Instantiate(RockPrefab_Two, pool_position.position + offset, Quaternion.identity);
        newRock.SetActive(false);
        poolRock.Add(newRock);
    }

    private IEnumerator Rock_Creat_co()
    {

        foreach (var rock in poolRock)
        {
            if (rock != null && !rock.activeSelf)
            {
                int randomValue = Random.Range(1, 5);
                rock.SetActive(true);
                Rigidbody rb = rock.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 forceDirection = new Vector3(-10.0f, -25f, 0);
                    float forceMagnitude = 2f;
                    rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
                }

                Rock_count--;
                yield return new WaitForSeconds(randomValue);

                if (Rock_count == 20)
                {
                    StartCoroutine(Rock_Creat_co());
                }
            }
        }

    }




}
