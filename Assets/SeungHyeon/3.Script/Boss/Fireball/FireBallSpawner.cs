using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallSpawner : MonoBehaviour
{
    public GameObject[] FireBall_prefabs;
    public GameObject target_obj;
    public GameObject FireBallobjects;

    [Header("파이어볼 관련")]
    public float FireBall_Speed = 2f;
    [Space(10f)]
    public float FireBall_distanceStart = 10.0f;//시작 지점을 기준으로 얼마나 꺾일지
    public float FireBall_distanceEnd = 3.0f;//도착 지점을 기준으로 얼마나 꺾일지
    [Space(10f)]
    public int FireBall_count = 7;
    [Range(0, 1)] public float FireBall_interval = 0.15f;
    public int m_shotCountEveryInterval = 1;

    private void Start()
    {
        target_obj = transform.parent.GetComponent<WizardControl>().wizardinfo.ChaseTarget.gameObject;
    }
    public IEnumerator CreateFireBall()
    {
        int _shotcount = FireBall_count;
        while(_shotcount > 0)
        {
            for(int i =0;i < m_shotCountEveryInterval;i++)
            {
                int rand = Random.Range(0, 4);
                GameObject Fireball = Instantiate(FireBall_prefabs[rand],gameObject.transform.position,Quaternion.identity, FireBallobjects.transform);
                Fireball.GetComponent<FireballMove>().Init(this.gameObject.transform, target_obj.transform, FireBall_Speed, FireBall_distanceStart, FireBall_distanceEnd);

                _shotcount--;
            }
            yield return new WaitForSeconds(FireBall_interval);
        }
        yield return null;
    }
}
