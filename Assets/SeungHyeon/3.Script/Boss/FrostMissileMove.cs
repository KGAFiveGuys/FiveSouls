using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostMissileMove : MonoBehaviour
{
    [SerializeField] private float Speed = 2f;
    Transform TargetTransform;
    Vector3 MoveVector;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        TargetTransform = FindObjectOfType<PlayerController>().transform;
        MoveVector = TargetTransform.position - this.transform.position;
        MoveVector.y += 1f;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += MoveVector * Speed * Time.deltaTime;
    }
}
