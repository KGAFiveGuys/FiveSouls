using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPickUpIcon : MonoBehaviour
{
    public static Item CurrentFocusedItem { get; private set; } = null;
    
    private Item itemSelf;
    private PlayerController playerController;
    private PocketInventory playerPocketInventory;
    private PlayerHUDController playerHUDController;
    private static GameObject pickUpIcon;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private void Awake()
    {
        TryGetComponent(out itemSelf);
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerPocketInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PocketInventory>();
        playerHUDController = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<PlayerHUDController>();

        if (pickUpIcon == null)
            pickUpIcon = playerHUDController.pickUpIcon;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (IsNearestFromPlayer())
            UpdatePickUpIconPosition();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CurrentFocusedItem == itemSelf)
        {
            CurrentFocusedItem = null;
            DisablePickUp();
            pickUpIcon.SetActive(false);
        }
    }

    private bool IsNearestFromPlayer()
    {
        // 현재 Focus된 대상이 나 자신인 경우
        if (CurrentFocusedItem == itemSelf)
            return true;

        // 현재 Focus된 대상이 없는 경우
        if (CurrentFocusedItem == null)
        {
            CurrentFocusedItem = itemSelf;
            playerController.OnPickUpItem += PickUp;
            pickUpIcon.SetActive(true);
            return true;
        }

        // 현재 Focus된 대상이 나보다 먼 경우
        var focusedItemDistance = Vector3.Distance(
            playerController.gameObject.transform.position, 
            CurrentFocusedItem.gameObject.transform.position
        );
        var thisItemDistance = Vector3.Distance(
            playerController.gameObject.transform.position, 
            transform.position
        );
        if (thisItemDistance < focusedItemDistance)
        {
            CurrentFocusedItem = itemSelf;
            playerController.OnPickUpItem += PickUp;
            var currentFocusedIcon = CurrentFocusedItem.GetComponent<ShowPickUpIcon>();
            currentFocusedIcon.DisablePickUp();
            return true;
        }

        return false;
    }

    public void DisablePickUp()
    {
        playerController.OnPickUpItem -= PickUp;
    }

    private void UpdatePickUpIconPosition()
    {
        var pos = Camera.main.WorldToScreenPoint(CurrentFocusedItem.transform.position + offset);
        var rectTransform = pickUpIcon.GetComponent<RectTransform>();
        var scale = rectTransform.localScale;
        var width = rectTransform.rect.width;
        var height = rectTransform.rect.height;

        var xOffset = scale.x * (width / 2);
        var yOffset = scale.y * (height / 2);

        pickUpIcon.transform.position = new Vector3(pos.x - xOffset, pos.y + yOffset, pos.z);
    }

    private void PickUp()
    {
        playerPocketInventory.StoreItem(itemSelf.itemInfo);
        pickUpIcon.SetActive(false);
        CurrentFocusedItem = null;
        DisablePickUp();
        Destroy(gameObject);
    }
}
