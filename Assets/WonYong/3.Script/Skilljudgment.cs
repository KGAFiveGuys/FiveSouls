using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skilljudgment: MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 3f);


    }


}
