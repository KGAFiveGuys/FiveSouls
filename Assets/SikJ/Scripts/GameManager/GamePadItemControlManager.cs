using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePadItemControlManager : MonoBehaviour
{
    private static GamePadItemControlManager _instance = null;
    public GamePadItemControlManager Instance => _instance;

    public bool IsVisible { get; private set; } = false;

    public InputAction select;
    public InputAction use;

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

    private void OnSelectPerformed(InputAction.CallbackContext obj)
    {
        var direction = obj.ReadValue<float>();
        SelectNextItem(direction);
    }
    private void SelectNextItem(float direction)
    {
        Debug.Log(direction);
    }

    private void OnUsePerformed(InputAction.CallbackContext obj)
    {
        UseCurrentItem();
    }
    private void UseCurrentItem()
    {
        Debug.Log("UseCurrentItem");
    }
}
