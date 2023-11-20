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
        npcid = this.GetComponent<NPCID>();
        npcmove = this.GetComponent<NPCMove>();
        npc_agent = this.GetComponent<NavMeshAgent>();
        NPC_Anim = this.GetComponent<Animator>();
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
            player.ToggleTargetGroupCamera(true, gameObject);
            var transposer = player.VC_LockOn
                            .GetComponent<CinemachineVirtualCamera>()
                            .GetCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset += dialogueCameraOffset;
            player.TargetGroup.m_Targets[0].radius = 20f;
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
        player.ControlState = ControlState.Uncontrollable;
    }
    public void EndTalkNpc()
    {
        if (IsTalking)
        {
            IsTalking = false;
            var transposer = player.VC_LockOn
                            .GetComponent<CinemachineVirtualCamera>()
                            .GetCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset -= dialogueCameraOffset;
            player.TargetGroup.m_Targets[0].radius = 50f;
            player.ToggleTargetGroupCamera(false, gameObject);
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
