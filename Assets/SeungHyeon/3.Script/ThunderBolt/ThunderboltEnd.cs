using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderboltEnd : MonoBehaviour
{
    private ParticleSystem particle;
    private ParticleSystem parentparticle;
    private void OnEnable()
    {
        TryGetComponent(out particle);
        parentparticle = transform.parent.gameObject.GetComponent<ParticleSystem>();
    }
    private void Update()
    {
        if(particle.isPlaying)
        {
            parentparticle.Stop();
        }
        if(!particle.IsAlive())
        {
            gameObject.SetActive(false);
            parentparticle.gameObject.SetActive(false);
        }
    }
}
