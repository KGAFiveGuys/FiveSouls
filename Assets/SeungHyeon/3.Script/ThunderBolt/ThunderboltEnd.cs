using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderboltEnd : MonoBehaviour
{
    private ParticleSystem particle;
    private GameObject parentObject;
    private void OnEnable()
    {
        TryGetComponent(out particle);
        parentObject = transform.parent.gameObject;
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        if(!particle.IsAlive())
        {
            gameObject.SetActive(false);
            parentObject.SetActive(false);
        }
    }
}
