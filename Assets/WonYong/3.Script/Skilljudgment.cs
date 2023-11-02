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
        //�÷��̾� Layer�� ���� �����ϴ� ������� �ٲ߽ô�.

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 15f);
        if (hitColliders.Length > 0)
        {
            // �÷��̾�� �浹�� ���� ���� ?
            foreach (Collider col in hitColliders)
            {
                print("���� �ճ� : " +GetTopParentTag(col.gameObject));
                //print("�Ÿ� :" + distance);
                //print("player���� �Ÿ� : "+ GetDistance());
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

