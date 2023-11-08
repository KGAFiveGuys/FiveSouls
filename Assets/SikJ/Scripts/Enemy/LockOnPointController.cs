using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class LockOnPointController : MonoBehaviour
{
    [field:SerializeField] public LockOnPoint StartPoint { get; set; }
    [SerializeField] private List<LockOnPoint> lockOnPointList = new List<LockOnPoint>();
    private Health _health;

    private void Awake()
    {
        TryGetComponent(out _health);
    }

    private void OnEnable()
    {
        _health.OnDead += TurnOffLockOnColliders;
    }

    private void OnDisable()
    {
        _health.OnDead -= TurnOffLockOnColliders;
    }

    private void TurnOffLockOnColliders()
    {
        foreach (var lockOnPoint in lockOnPointList)
        {
            lockOnPoint.LockOnCollider.enabled = false;
        }
    }
}
