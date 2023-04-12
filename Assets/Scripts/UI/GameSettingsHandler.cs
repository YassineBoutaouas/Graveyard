using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameSettingsHandler : MonoBehaviour
{
    public InGameSettings CurrentGameSettings;
    
    [Header("UI references")]
    [Space(10)]
    public Toggle InvertCameraToggle;
    public Toggle AutoTargetingToggle;
    public Toggle RumbleToggle;

    [Space(10)]
    public Slider Camera_X;
    public TextMeshProUGUI Camera_X_Text;

    [Space(10)]
    public Slider Camera_Y;
    public TextMeshProUGUI Camera_Y_Text;

    [Space(10)]
    public Slider MasterVolume;
    public TextMeshProUGUI MasterVolume_Text;

    [Space(10)]
    public Slider RhythmVolume;
    public TextMeshProUGUI RhythmVolume_Text;

    [Space(10)]
    public Slider MusicVolume;
    public TextMeshProUGUI MusicVolume_Text;

    [Space(10)]
    public Slider SoundFXVolume;
    public TextMeshProUGUI SoundFXVolume_Text;

    public GameObject firstSelectedButton;

    private void Start()
    {
        if (!CurrentGameSettings) return;

        InvertCameraToggle.isOn = CurrentGameSettings.InvertCamera;
        AutoTargetingToggle.isOn = CurrentGameSettings.AutoTargeting;
        RumbleToggle.isOn = CurrentGameSettings.Rumble;

        Camera_X.value = CurrentGameSettings.CameraSpeed_X;
        Camera_X_Text.text = (CurrentGameSettings.CameraSpeed_X).ToString();

        Camera_Y.value = CurrentGameSettings.CameraSpeed_Y;
        Camera_Y_Text.text = (CurrentGameSettings.CameraSpeed_Y).ToString();

        MasterVolume.value = CurrentGameSettings.MasterVolume * 100;
        MasterVolume_Text.text = (CurrentGameSettings.MasterVolume * 100).ToString();

        RhythmVolume.value = CurrentGameSettings.RhythmTrackVolume * 100;
        RhythmVolume_Text.text = (CurrentGameSettings.RhythmTrackVolume * 100).ToString();

        MusicVolume.value = CurrentGameSettings.MusicVolume * 100;
        MusicVolume_Text.text = (CurrentGameSettings.MusicVolume * 100).ToString();

        SoundFXVolume.value = CurrentGameSettings.SoundFXVolume * 100;
        SoundFXVolume_Text.text = (CurrentGameSettings.SoundFXVolume * 100).ToString();

        InvertCameraToggle.onValueChanged.AddListener((ctx) => CurrentGameSettings.InvertCamera = ctx);
        AutoTargetingToggle.onValueChanged.AddListener((ctx) => CurrentGameSettings.AutoTargeting = ctx);
        RumbleToggle.onValueChanged.AddListener((ctx) => CurrentGameSettings.Rumble = ctx);

        Camera_X.onValueChanged.AddListener((ctx) => { CurrentGameSettings.CameraSpeed_X = ctx; Camera_X_Text.text = (ctx).ToString(); });
        Camera_Y.onValueChanged.AddListener((ctx) => { CurrentGameSettings.CameraSpeed_Y = ctx; Camera_Y_Text.text = (ctx).ToString(); });

        MasterVolume.onValueChanged.AddListener((ctx) => { CurrentGameSettings.MasterVolume = ctx / 100; MasterVolume_Text.text = (ctx).ToString(); AudioManager.Instance.OnChangedVolume(); });
        RhythmVolume.onValueChanged.AddListener((ctx) => { CurrentGameSettings.RhythmTrackVolume = ctx / 100; RhythmVolume_Text.text = (ctx).ToString(); AudioManager.Instance.OnChangedVolume(); });
        MusicVolume.onValueChanged.AddListener((ctx) => { CurrentGameSettings.MusicVolume = ctx / 100; MusicVolume_Text.text = (ctx).ToString(); AudioManager.Instance.OnChangedVolume(); });
        SoundFXVolume.onValueChanged.AddListener((ctx) => { CurrentGameSettings.SoundFXVolume = ctx / 100; SoundFXVolume_Text.text = (ctx).ToString(); AudioManager.Instance.OnChangedVolume(); });
    }

    public void ToggleObject(GameObject togglingObject)
    {
        bool oldState = togglingObject.activeSelf;
        togglingObject.SetActive(!oldState);

        //activating menu - selecting first button always
        if (!oldState)
        {
            //caro: assigning firstButton of menu

            firstSelectedButton = togglingObject.GetComponentInChildren<Button>().gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Debug.Log(EventSystem.current.currentSelectedGameObject);
        }

        //deactivating menu - selecting first button of default menu
        else
        {
            //coming back from submenu to pause menu - activate first button of this menu
            firstSelectedButton = gameObject.GetComponentInChildren<Button>().gameObject;
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);

            //exception case - if in main menu and pause menu gets deactivated, activate first prio button of main menu
            if (togglingObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }
    }
}