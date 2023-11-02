using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Skilljudgment: MonoBehaviour
{
    private GameObject player;

    private void Awake()
    {
       player =  GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        Jugement_Far();

    }

    private float GetDistance()
    {
        float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
        return distance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 8f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 15f);


    }

    private void Jugement_Far()
    {
        //플레이어 Layer를 만들어서 검출하는 방식으로 바꿉시다.

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 15f);
        if (hitColliders.Length > 0)
        {
            // 플레이어랑 충돌시 패턴 실행 ?
            foreach (Collider col in hitColliders)
            {
                print("제일 먼놈 : " +GetTopParentTag(col.gameObject));
                //print("거리 :" + distance);
                //print("player와의 거리 : "+ GetDistance());
                //print(GetTopParentTag(col.gameObject));
                //print(gameObject.name);
            }
        }
    }

    private string GetTopParentTag(GameObject obj)
    {
        Transform currentTransform = obj.transform;
        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;
        }

        return currentTransform.gameObject.tag;
    }

}

