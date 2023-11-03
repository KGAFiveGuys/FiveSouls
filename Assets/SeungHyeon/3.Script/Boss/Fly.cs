using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
    [SerializeField]private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }




}
