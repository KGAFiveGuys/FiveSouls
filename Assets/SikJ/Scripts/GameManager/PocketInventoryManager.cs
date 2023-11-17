using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PocketInventoryManager : MonoBehaviour
{
    private static PocketInventoryManager _instance = null;
    public PocketInventoryManager Instance => _instance;
    public bool IsVisible { get; private set; } = false;

    [SerializeField] private Image currentPocket;
    [SerializeField] private TextMeshProUGUI currentPocketCount;
    [SerializeField] private TextMeshProUGUI currentPocketName;

    private PocketInventory playerPocketInventory;

    public InputAction select;
    public InputAction use;

    public event Action OnUseItem;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else if (_instance != this)
        {
            Destroy(this);
        }

        playerPocketInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PocketInventory>();
    }

    private void OnEnable()
    {
        select.performed += OnSelectPerformed;
        select.Enable();

        use.performed += OnUsePerformed;
        use.Enable();
    }

    private void OnDisable()
    {
        select.performed -= OnSelectPerformed;
        select.Disable();

        use.performed -= OnUsePerformed;
        use.Disable();
    }

    public void ChangePocketInfo(ItemSO itemInfo, int count)
    {
        currentPocket.sprite = itemInfo.image;
        currentPocketCount.text = $"{count:#0}";
        currentPocketName.text = itemInfo.Name;
    }

    private void OnSelectPerformed(InputAction.CallbackContext obj)
    {
        var direction = obj.ReadValue<float>();
        SelectItem(direction);
    }
    private void SelectItem(float direction)
    {
        playerPocketInventory.ChangeSelection((int)direction);
    }

    private void OnUsePerformed(InputAction.CallbackContext obj)
    {
        UseItem();
    }
    private void UseItem()
    {
        playerPocketInventory.UseCurrentItem();
        OnUseItem?.Invoke();
    }
}
