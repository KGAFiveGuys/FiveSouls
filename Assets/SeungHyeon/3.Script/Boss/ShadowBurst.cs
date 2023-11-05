using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBurst : MonoBehaviour
{
    [SerializeField]private ParticleSystem ps;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("¿€µø");
            var collisionModule = ps.collision;
            collisionModule.enabled =   false;
        }
    }
}
