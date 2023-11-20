using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionReseter : MonoBehaviour
{
    private void OnDisable()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
