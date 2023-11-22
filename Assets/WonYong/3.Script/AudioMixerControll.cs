using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class AudioMixerControll : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider Master;
    [SerializeField] private Slider BGM;
    [SerializeField] private Slider SFX;
    [SerializeField] private GameObject AudioSourcePrefab;
    [SerializeField] private GameObject Sound_UI;
    private PlayerController playerController;
    private PlayerHUDController playerHUD;
    public bool IsSetting { get; set; } = false;

    private List<Slider> sliderList = new List<Slider>();

    [SerializeField] private InputAction selectSlider;
    [SerializeField] private InputAction changeValue;

    public static AudioMixerControll instance = null;
    private void Awake()
    {
        Master.onValueChanged.AddListener(SetMasterVolume);
        BGM.onValueChanged.AddListener(SetMusicVolume);
        SFX.onValueChanged.AddListener(SetSFXVolume);
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != null && instance != this)
        {
            Destroy(this);
        }

        playerController = FindObjectOfType<PlayerController>();
        playerHUD = FindObjectOfType<PlayerHUDController>();
    }

    private void Start()
    {
        Cursor.visible = false;

        for (int i = 0; i < Sound_UI.transform.childCount; i++)
        {
            var slider = Sound_UI.transform.GetChild(i).GetComponent<Slider>();
            if (slider != null)
                sliderList.Add(slider);
        }
    }

    private void OnEnable()
    {
        selectSlider.performed += SelectSlider;
        selectSlider.Enable();

        changeValue.performed += ChangeValue;
        changeValue.canceled += CancelValue;
        changeValue.Enable();

        playerController.OnToggleSetting += toggle_Sound;
    }

    private void OnDisable()
    {
        selectSlider.performed -= SelectSlider;
        selectSlider.Disable();

        changeValue.performed -= ChangeValue;
        changeValue.Disable();

        playerController.OnToggleSetting -= toggle_Sound;
    }

    private void Update()
    {
        ControlVolume();
    }

    private void ControlVolume()
    {
        sliderList[currnetSliderIndex].value += desiredChange * valueChangeModifier * Time.deltaTime;
    }

    private int currnetSliderIndex = 0;
    private void SelectSlider(InputAction.CallbackContext context)
    {
        if (!IsSetting)
            return;

        ChangeSliderColor(Color.white);

        var isDown = context.ReadValueAsButton();

        if (isDown)
            currnetSliderIndex--;
        else
            currnetSliderIndex++;

        if (currnetSliderIndex > 0)
            currnetSliderIndex = currnetSliderIndex % sliderList.Count;
        else if (currnetSliderIndex < 0)
            currnetSliderIndex += sliderList.Count;

        ChangeSliderColor(Color.green);
    }

    [SerializeField] private float valueChangeModifier = 1f;
    private float desiredChange = 0f;
    private void ChangeValue(InputAction.CallbackContext context)
    {
        if (!IsSetting)
            return;

        if (context.ReadValue<Vector2>().x < 0)
            desiredChange = -1f;
        else if (context.ReadValue<Vector2>().x > 0)
            desiredChange = 1f;
    }

    private void CancelValue(InputAction.CallbackContext context)
    {
        if (!IsSetting)
            return;

        desiredChange = 0f;
    }

    private void toggle_Sound()
    {
        IsSetting = !IsSetting;
        Cursor.visible = IsSetting;

        if (IsSetting)
        {
            currnetSliderIndex = 0;
            ChangeSliderColor(Color.green);

            playerController.MoveDirection = Vector3.zero;
            playerController.ControlState = ControlState.Uncontrollable;
            playerHUD.FadeOutPlayerHUD();
        }
        else
        {
            ChangeSliderColor(Color.white);

            playerHUD.FadeInPlayerHUD();
            playerController.ControlState = ControlState.Controllable;
        }

        Sound_UI.SetActive(IsSetting);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    public void ChangeSliderColor(Color color)
    {
        if (Gamepad.current == null)
            return;

        ColorBlock temp = sliderList[currnetSliderIndex].colors;
        temp.normalColor = color;
        sliderList[currnetSliderIndex].colors = temp;
    }
}
