using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSword : MonoBehaviour
{
    [SerializeField] Transform Target;
    [SerializeField] Collider AttackCollider;
    private bool isStop = false;

    void Update()
    {
        if (!isStop)
        {
            transform.position += transform.TransformDirection(Vector3.forward) * 400 * Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        AttackCollider.enabled = true;
        isStop = false;
        float RanX = Random.Range(500f, -500f);
        float RanY = Random.Range(500f, -500f);
        transform.position = new Vector3(RanX, 1000f, RanY);
        transform.LookAt(Target.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        int Ground = LayerMask.NameToLayer("Ground");
        if (other.gameObject.layer == Ground)
        {
            isStop = true;
            AttackCollider.enabled = false;
        }
    }

}
