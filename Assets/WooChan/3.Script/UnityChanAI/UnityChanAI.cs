using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanAI : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject Target = null;
    [SerializeField] private float WalkSpeed = 5f;
    [SerializeField] private float RunSpeed = 10f;

    private void Awake()
    {

    }

    private void Start()
    {
        
    }


    private void Update()
    {
        if (Target == null)
        {
            SearchPlayer();
        }
        else if (Target)
        {
            transform.LookAt(Target.transform);
            WalkMovement();
        }
    }



    private void SearchPlayer()
    {
        int layerMask = LayerMask.GetMask("Player");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f, layerMask);
        foreach (Collider collider in hitColliders)
        {
            Target = collider.gameObject;
        }
    }

    private void WalkMovement()
    {
        animator.SetBool("isWalk", true);
        transform.position += transform.TransformDirection(Vector3.forward) * WalkSpeed * Time.deltaTime;
    }














    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 20f);
    }
}
