using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCTalk : MonoBehaviour
{
    private NPCID npcid;
    private PlayerController player;
    private NavMeshAgent npc_agent;
    private NPCMove npcmove;
    [SerializeField]private bool MoveCharacter = true;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        npcid = this.GetComponent<NPCID>();
        npcmove = this.GetComponent<NPCMove>();
        npc_agent = this.GetComponent<NavMeshAgent>();
        if(npc_agent == null)
        {
            MoveCharacter = false;
            return;
        }
    }
    public void TalkNpc()
    {
        if(XmlTest.instance.DialogueBox.activeSelf)
        {
            XmlTest.instance.dialogueindex++;
            if (XmlTest.instance.dialogueindex > XmlTest.instance.dialogues.Count - 1)
            {
                EndTalkNpc();
                return;
            }
        }
        if (MoveCharacter)
        {
            npc_agent.speed = 0;
            XmlTest.instance.DisplayDialogue(npcid.CharacterID);
        }
    }
    public void EndTalkNpc()
    {
        XmlTest.instance.dialogueindex = 0;
        if(MoveCharacter)
        {
            npc_agent.speed = 10;
            XmlTest.instance.DialogueBox.SetActive(false);
            player.OnTalkToNPC -= TalkNpc;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.OnTalkToNPC += TalkNpc;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.OnTalkToNPC -= TalkNpc;
        }
    }
}
