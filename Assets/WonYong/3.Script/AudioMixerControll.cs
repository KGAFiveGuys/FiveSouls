using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioMixerControll : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider Master;
    [SerializeField] private Slider BGM;
    [SerializeField] private Slider SFX;
    [SerializeField] private GameObject AudioSourcePrefab;
    [SerializeField] private GameObject Sound_UI;
    private PlayerController playerController;
    bool isSound = false;

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
    }

    private void OnEnable()
    {
        playerController.OnToggleSetting += toggle_Sound;
    }

    private void OnDisable()
    {
        playerController.OnToggleSetting -= toggle_Sound;
    }

    private void toggle_Sound()
    {
        isSound = !isSound;
        Sound_UI.SetActive(isSound);
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
}
