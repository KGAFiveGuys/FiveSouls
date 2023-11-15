using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{
    public Transform[] points;
    private int destPoint = 0;
    private NavMeshAgent npc_agent;

    private void Start()
    {
        npc_agent = GetComponent<NavMeshAgent>();

        GotoNextPoint();
    }
    void GotoNextPoint()
    {
        if (points.Length == 0)
            return;
        npc_agent.speed = 10;
        npc_agent.destination = points[destPoint].position;
        destPoint = (destPoint + 1) % points.Length;
    }
    private void Update()
    {
        if (!npc_agent.pathPending && npc_agent.remainingDistance < 0.5f)
            GotoNextPoint();
    }
    public void TalkNpc()
    {
        npc_agent.speed = 0;
        //모션바꾸는거
        //쳐다보게 하고 return id
    }

}
