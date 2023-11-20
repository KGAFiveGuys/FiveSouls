    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class NPCTalk : MonoBehaviour
{
    private NPCID npcid;
    private PlayerController player;
    private ShowDialogueIcon dialogueIcon;
    private Animator NPC_Anim;
    private NavMeshAgent npc_agent;
    private NPCMove npcmove;
    [SerializeField]private bool MoveCharacter = true;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        dialogueIcon = GetComponent<ShowDialogueIcon>();
        npcid = GetComponent<NPCID>();
        npcmove = GetComponent<NPCMove>();
        npc_agent = GetComponent<NavMeshAgent>();
        NPC_Anim = GetComponent<Animator>();
        if(npc_agent == null)
        {
            MoveCharacter = false;
            return;
        }
    }

    [SerializeField] private Vector3 dialogueCameraOffset = new Vector3(0, 10f, 10f);
    public bool IsTalking { get; set; } = false;
    public void TalkNpc()
    {
        if (!IsTalking)
        {
            IsTalking = true;
            player.ToggleDialogueCamera(true, gameObject);
        }

        if (XmlTest.instance.DialogueBox.activeSelf)
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
        }
        var targetPos = player.transform.position;
        var lookAtPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        transform.LookAt(lookAtPos);
        if (!NPC_Anim.GetCurrentAnimatorStateInfo(0).IsName("Talk"))
        {
            NPC_Anim.SetTrigger("Talk");
        }
        XmlTest.instance.DisplayDialogue(npcid.CharacterID);
        
        player.MoveDirection = Vector3.zero;
        player.gameObject.transform.LookAt(gameObject.transform);
        player.ControlState = ControlState.Uncontrollable;
    }
    public void EndTalkNpc()
    {
        if (IsTalking)
        {
            IsTalking = false;
            player.ToggleDialogueCamera(false);
        }

        XmlTest.instance.dialogueindex = 0;
        if(MoveCharacter)
        {
            npc_agent.speed = 10;
        }
        XmlTest.instance.DialogueBox.SetActive(false);
        dialogueIcon.EndDialogue();

        player.ControlState = ControlState.Controllable;
    }

    private bool isSubscribed = false;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")
            && !isSubscribed)
        {
            isSubscribed = true;
            player.OnTalkToNPC += TalkNpc;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")
            && isSubscribed)
        {
            isSubscribed = false;
            player.OnTalkToNPC -= TalkNpc;
        }
    }
}
