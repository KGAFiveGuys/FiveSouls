using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnPoint : MonoBehaviour
{
    [field:SerializeField] public bool IsLockedOn { get; set; } = false;
    [field: SerializeField] public Collider LockOnCollider { get; set; }

    [Header("Linked LockOnPoint")]
    [SerializeField] private LockOnPoint leftPoint;
    [SerializeField] private LockOnPoint rightPoint;
    [SerializeField] private LockOnPoint upPoint;
    [SerializeField] private LockOnPoint downPoint;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private IEnumerator transitionCheck;
    public void StartTransitionCheck()
    {

    }

    public void StopTransitionCheck()
    {

    }

    //private IEnumerator CheckTransition()
    //{
    //    while (IsLockedOn)
    //    {
    //        var desiredX = _playerController.DesiredRotate.x;
    //        var desiredY = _playerController.DesiredRotate.y;

    //        Vector2 desiredDirection = desiredX < desiredY ? Vector2.up * desiredY : Vector2.right * desiredX;
    //        desiredDirection = desiredDirection.normalized;

    //        // Up/Down
    //        if (desiredDirection == Vector2.up
    //            && upPoint != null)
    //        {

    //            IsLockedOn = false;
    //        }
    //        else if (desiredDirection == Vector2.down
    //                 && downPoint != null)
    //        {

    //            IsLockedOn = false;
    //        }

    //        // Right/Left
    //        if (desiredDirection == Vector2.right
    //            && rightPoint != null)
    //        {

    //            IsLockedOn = false;
    //        }
    //        else if (desiredDirection == Vector2.left
    //                 && leftPoint != null)
    //        {

    //            IsLockedOn = false;
    //        }

    //        yield return null;
    //    }
    //}
}
