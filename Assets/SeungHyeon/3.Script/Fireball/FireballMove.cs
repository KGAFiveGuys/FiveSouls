using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireBallStatus
{
    Chase = 0,
    Find
}

public class FireballMove : MonoBehaviour
{
    Vector3[] FireBall_Points = new Vector3[4];


    GameObject Targetplayer;
    Vector3 DistVector;
    Vector3 DirVector;

    private float FireBall_timerMax = 0;
    private float FireBall_timerCurrent = 0;
    private float FireBall_speed;
    private float DistanceOfTarget;
    private FireBallStatus status = FireBallStatus.Chase;
    public void Init(Transform _startTr, Transform _endTr, float _speed, float _newPointDistanceFromStartTr, float _newPointDistanceFromEndTr)
    {
        FireBall_speed = _speed;

        FireBall_timerMax = 5f;
            //Random.Range(0.8f, 1.0f);

        FireBall_Points[0] = _startTr.position;

        // ���� ������ �������� ���� ����Ʈ ����.
        FireBall_Points[1] = _startTr.position +
            (_newPointDistanceFromStartTr * Random.Range(-1.0f, 1.0f) * _startTr.right) + // X (��, �� ��ü)
            (_newPointDistanceFromStartTr * Random.Range(-0.15f, 1.0f) * _startTr.up) + // Y (�Ʒ��� ����, ���� ��ü)
            (_newPointDistanceFromStartTr * Random.Range(-1.0f, -0.8f) * _startTr.forward); // Z (�� �ʸ�)

        // ���� ������ �������� ���� ����Ʈ ����.
        FireBall_Points[2] = _endTr.position +
            (_newPointDistanceFromEndTr * Random.Range(-1.0f, 1.0f) * _endTr.right) + // X (��, �� ��ü)
            (_newPointDistanceFromEndTr * Random.Range(-1.0f, 1.0f) * _endTr.up) + // Y (��, �Ʒ� ��ü)
            (_newPointDistanceFromEndTr * Random.Range(0.8f, 1.0f) * _endTr.forward); // Z (�� �ʸ�)

    }
    private void Start()
    {
        Targetplayer = FindObjectOfType<FireBallSpawner>().target_obj;
    }
    private void Update()
    {
        FireBall_timerCurrent += Time.deltaTime * FireBall_speed;

        DistanceOfTarget = Vector3.Distance(Targetplayer.transform.position, transform.position);
        if(DistanceOfTarget <= 3f && status.Equals(FireBallStatus.Chase))
        {
            status = FireBallStatus.Find;
            DistVector = Targetplayer.transform.position - transform.position;
            DirVector = DistVector.normalized;
        }
        if (status.Equals(FireBallStatus.Chase))
        {   
            FireBall_Points[3] = Targetplayer.transform.position;
            FireBall_Points[3].y += 1; 
            //������ ����� X,Y,Z ��ǥ ���
            transform.position = new Vector3(
            CubicBezierCurve(FireBall_Points[0].x, FireBall_Points[1].x, FireBall_Points[2].x, FireBall_Points[3].x),
            CubicBezierCurve(FireBall_Points[0].y, FireBall_Points[1].y, FireBall_Points[2].y, FireBall_Points[3].y),
            CubicBezierCurve(FireBall_Points[0].z, FireBall_Points[1].z, FireBall_Points[2].z, FireBall_Points[3].z));
        }
        else
        {
            transform.position += DirVector * FireBall_timerCurrent * (Time.deltaTime * FireBall_speed*1.5f);
        }
    }
    private float CubicBezierCurve(float a, float b, float c, float d)
    {
        // (0~1)�� ���� ���� ������ � ���� ���ϱ� ������, ������ ���� �ð��� ���ߴ�.
        float t = FireBall_timerCurrent / FireBall_timerMax; // (���� ��� �ð� / �ִ� �ð�)


        float ab = Mathf.Lerp(a, b, t);
        float bc = Mathf.Lerp(b, c, t);
        float cd = Mathf.Lerp(c, d, t);

        float abbc = Mathf.Lerp(ab, bc, t);
        float bccd = Mathf.Lerp(bc, cd, t);

        return Mathf.Lerp(abbc, bccd, t);
    }
}
