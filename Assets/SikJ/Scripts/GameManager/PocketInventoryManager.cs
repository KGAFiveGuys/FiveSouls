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

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        playerPocketInventory = playerObj.GetComponent<PocketInventory>();
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

        var originPocketColor = currentPocket.color;
        var originPocketCount = currentPocketCount.color;
        var originPocketName = currentPocketName.color;
        if (count > 0)
		{
            currentPocket.color = new Color(originPocketColor.r, originPocketColor.g, originPocketColor.b, 1);
            currentPocketCount.color = new Color(originPocketColor.r, originPocketColor.g, originPocketColor.b, 1);
            currentPocketName.color = new Color(originPocketName.r, originPocketName.g, originPocketName.b, 1);
        }
		else
		{
            currentPocket.color = new Color(originPocketColor.r, originPocketColor.g, originPocketColor.b, .25f);
            currentPocketCount.color = new Color(originPocketColor.r, originPocketColor.g, originPocketColor.b, .25f);
            currentPocketName.color = new Color(originPocketName.r, originPocketName.g, originPocketName.b, .25f);
        }
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
        OnUseItem?.Invoke();
    }
}
