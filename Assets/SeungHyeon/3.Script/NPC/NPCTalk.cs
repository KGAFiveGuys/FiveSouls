    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class NPCTalk : MonoBehaviour
{
    private NPCID npcid;
    private PlayerController playerController;
    private PlayerHUDController PlayerHUD;
    private ShowDialogueIcon dialogueIcon;
    private Animator NPC_Anim;
    private NavMeshAgent npc_agent;
    private NPCMove npcmove;
    [SerializeField]private bool MoveCharacter = true;
    private void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        PlayerHUD = FindObjectOfType<PlayerHUDController>();
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
            PlayerHUD.FadeOutPlayerHUD();
            playerController.ToggleDialogueCamera(true, gameObject);
        }

        if (XmlTest.instance.DialogueBox.activeSelf)
        {
            XmlTest.instance.dialogueindex++;
            if (XmlTest.instance.dialogueindex > XmlTest.instance.dialogues[npcid.CharacterID].Texts.Count - 1)
            {
                EndTalkNpc();
                return;
            }
        }
        if (MoveCharacter)
        {
            npc_agent.speed = 0;
        }
        var targetPos = playerController.transform.position;
        var lookAtPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        transform.LookAt(lookAtPos);
        if (!NPC_Anim.GetCurrentAnimatorStateInfo(0).IsName("Talk"))
        {
            NPC_Anim.SetTrigger("Talk");
        }
        XmlTest.instance.DisplayDialogue(npcid.CharacterID);
        
        playerController.MoveDirection = Vector3.zero;
        playerController.gameObject.transform.LookAt(gameObject.transform);
        playerController.ControlState = ControlState.Uncontrollable;
    }
    public void EndTalkNpc()
    {
        XmlTest.instance.dialogueindex = 0;
        if (IsTalking)
        {
            IsTalking = false;
            playerController.ToggleDialogueCamera(false);
            PlayerHUD.FadeInPlayerHUD();
        }
        if(MoveCharacter)
        {
            npc_agent.speed = 10;
        }
        XmlTest.instance.DialogueBox.SetActive(false);
        dialogueIcon.EndDialogue();

        playerController.ControlState = ControlState.Controllable;
    }

    private bool isSubscribed = false;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")
            && !isSubscribed)
        {
            isSubscribed = true;
            playerController.OnTalkToNPC += TalkNpc;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")
            && isSubscribed
            && !IsTalking)
        {
            isSubscribed = false;
            playerController.OnTalkToNPC -= TalkNpc;
        }
    }
}
