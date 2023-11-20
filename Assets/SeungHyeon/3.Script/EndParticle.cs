using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndParticle : MonoBehaviour
{
    private ParticleSystem particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!particleSystem.IsAlive())
        {
            gameObject.SetActive(false);
        }
    }
}
