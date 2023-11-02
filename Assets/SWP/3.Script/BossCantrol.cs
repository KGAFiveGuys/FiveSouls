using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCantrol : MonoBehaviour
{
    [SerializeField] private float detectRange = 30f;
    [SerializeField] private LayerMask TargetMask;

    private GameObject Target;
    private float distance = 50f;


    private void Start()
    {

    }

    private void Update()
    {
        CalcDistance();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

    private void CalcDistance()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectRange, TargetMask); //태클이 10f
        foreach (Collider col in colls)
        {
            if (col.gameObject.layer.Equals(TargetMask))
            {
                Target = col.gameObject;
                distance = Vector3.Distance(Target.transform.position, transform.position);
                Debug.Log($"타겟거리 : {distance}");
                break;
            }
        }
    }





}
