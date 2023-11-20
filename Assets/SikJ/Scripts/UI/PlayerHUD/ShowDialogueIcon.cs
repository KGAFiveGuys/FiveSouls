using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowDialogueIcon : MonoBehaviour
{
    public static GameObject CurrentFocusedNPC { get; private set; } = null;
    
    private PlayerController playerController;
    private PlayerHUDController playerHUDController;
    private static GameObject dialogueIcon;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerHUDController = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<PlayerHUDController>();

        if (dialogueIcon == null)
            dialogueIcon = playerHUDController.dialogueIcon;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (IsNearestFromPlayer())
            UpdateDialogueIconPosition();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CurrentFocusedNPC == gameObject)
        {
            CurrentFocusedNPC = null;
            DisableDialogue();
            dialogueIcon.SetActive(false);
        }
    }

    private bool IsNearestFromPlayer()
    {
        // 현재 Focus된 대상이 나 자신인 경우
        if (CurrentFocusedNPC == gameObject)
            return true;

        // 현재 Focus된 대상이 없는 경우
        if (CurrentFocusedNPC == null)
        {
            CurrentFocusedNPC = gameObject;
            playerController.OnTalkToNPC += StartDialogue;
            dialogueIcon.SetActive(true);
            return true;
        }

        // 현재 Focus된 대상이 나보다 먼 경우
        var focusedItemDistance = Vector3.Distance(
            playerController.gameObject.transform.position, 
            CurrentFocusedNPC.gameObject.transform.position
        );
        var thisItemDistance = Vector3.Distance(
            playerController.gameObject.transform.position, 
            transform.position
        );
        if (thisItemDistance < focusedItemDistance)
        {
            CurrentFocusedNPC = gameObject;
            playerController.OnTalkToNPC += StartDialogue;
            var currentFocusedIcon = CurrentFocusedNPC.GetComponent<ShowDialogueIcon>();
            currentFocusedIcon.DisableDialogue();
            return true;
        }

        return false;
    }

    public void DisableDialogue()
    {
        playerController.OnPickUpItem -= StartDialogue;
    }

    private void UpdateDialogueIconPosition()
    {
        var pos = Camera.main.WorldToScreenPoint(CurrentFocusedNPC.transform.position + offset);
        var rectTransform = dialogueIcon.GetComponent<RectTransform>();
        var scale = rectTransform.localScale;
        var width = rectTransform.rect.width;
        var height = rectTransform.rect.height;

        var xOffset = scale.x * (width / 2);
        var yOffset = scale.y * (height / 2);

        dialogueIcon.transform.position = new Vector3(pos.x - xOffset, pos.y + yOffset, pos.z);
    }

    public void StartDialogue()
    {
        dialogueIcon.SetActive(false);
    }

    public void EndDialogue()
    {
        dialogueIcon.SetActive(true);
        CurrentFocusedNPC = null;
        DisableDialogue();
    }
}
