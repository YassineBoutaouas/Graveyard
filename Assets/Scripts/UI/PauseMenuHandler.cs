using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuHandler : HUDElementController
{
    public InGameSettings CurrentGameSettings;

    private InputManager _inputManager;

    private HUDElementController _pauseMenuMain;
    private HUDElementController _settingsMenuMain;
    private HUDElementController _controlsSettings;
    private HUDElementController _cameraSettings;
    private HUDElementController _audioSettings;
    private HUDElementController _areYouSureMenu;

    private GlobalHUDManager _globalHudManager;

    public override void OnEnable()
    {
        base.OnEnable();

        _inputManager = InputManager.GetInstance();
        _inputManager.SwitchActionMap(_inputManager.inputActions.UI, _inputManager.inputActions.InGame);

        GameManager.Instance.PlayerController.Velocity = Vector3.zero;
        GameManager.Instance.PlayerController.CanMove = false;
        GameManager.Instance.PlayerController.CanRotate = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _globalHudManager = GlobalHUDManager.Instance;
        _globalHudManager.ChangeHUDState(GlobalHUDManager.HUDStates.PauseMenu);

        #region Get hud elements
        _pauseMenuMain = _globalHudManager.GetHUDElement("PauseMenuMain");
        _settingsMenuMain = _globalHudManager.GetHUDElement("SettingsMain");
        _cameraSettings = _globalHudManager.GetHUDElement("CameraSettings");
        _audioSettings = _globalHudManager.GetHUDElement("AudioSettings");
        _controlsSettings = _globalHudManager.GetHUDElement("ControlSettings");
        _areYouSureMenu = _globalHudManager.GetHUDElement("Quit?");
        #endregion

        _globalHudManager.EnableHUDElement("PauseMenuMain", true);
        EventSystem.current.SetSelectedGameObject(_pauseMenuMain.ButtonElements["Resume_Button"].gameObject);

        #region set starting values
        _cameraSettings.ToggleElements["InvertCameraToggle"].isOn = CurrentGameSettings.InvertCamera;

        _cameraSettings.SliderElements["CameraSpeed_X_Slider"].value = CurrentGameSettings.CameraSpeed_X;
        _cameraSettings.SliderElements["CameraSpeed_Y_Slider"].value = CurrentGameSettings.CameraSpeed_Y;

        _audioSettings.SliderElements["MasterVolume_Slider"].value = CurrentGameSettings.MasterVolume * 100;
        _audioSettings.SliderElements["RhythmVolume_Slider"].value = CurrentGameSettings.RhythmTrackVolume * 100;
        _audioSettings.SliderElements["MusicVolume_Slider"].value = CurrentGameSettings.MusicVolume * 100;
        _audioSettings.SliderElements["SoundFXVolume_Slider"].value = CurrentGameSettings.SoundFXVolume * 100;

        _controlsSettings.ToggleElements["AutoTargetingToggle"].isOn = CurrentGameSettings.AutoTargeting;
        _controlsSettings.ToggleElements["RumbleToggle"].isOn = CurrentGameSettings.Rumble;
        #endregion

        #region Set Current values
        _cameraSettings.TextElements["CameraSpeed_X_CurrentValue"].text = (CurrentGameSettings.CameraSpeed_X).ToString();
        _cameraSettings.TextElements["CameraSpeed_Y_CurrentValue"].text = (CurrentGameSettings.CameraSpeed_Y).ToString();

        _audioSettings.TextElements["MasterVolume_CurrentValue"].text = (CurrentGameSettings.MasterVolume * 100).ToString();
        _audioSettings.TextElements["RhythmVolume_CurrentValue"].text = (CurrentGameSettings.RhythmTrackVolume * 100).ToString();
        _audioSettings.TextElements["MusicVolume_CurrentValue"].text = (CurrentGameSettings.MusicVolume * 100).ToString();
        _audioSettings.TextElements["SoundFXVolume_CurrentValue"].text = (CurrentGameSettings.SoundFXVolume * 100).ToString();
        #endregion
    }

    public override void Start()
    {
        base.Start();

        #region pause menu main events
        _pauseMenuMain.ButtonElements["Resume_Button"].onClick.AddListener(() => gameObject.SetActive(false));
        _pauseMenuMain.ButtonElements["Settings_Button"].onClick.AddListener(() => SwitchMenuElement("PauseMenuMain", "SettingsMain", "Controls_Button"));
        _pauseMenuMain.ButtonElements["BackToMainMenu_Button"].onClick.AddListener(() => { SceneManager.LoadScene(0); });
        _pauseMenuMain.ButtonElements["PauseQuit_Button"].onClick.AddListener(() => SwitchMenuElement("PauseMenuMain", "Quit?", "NoQuit_Button"));

        _pauseMenuMain.ButtonElements["Tutorial_Button"].onClick.AddListener(() => { GlobalHUDManager.Instance.EnableHUDElement("Tutorial", true); });
        #endregion

        #region Settings events
        _settingsMenuMain.ButtonElements["SettingsToPauseMenu_Button"].onClick.AddListener(() => SwitchMenuElement("SettingsMain", "PauseMenuMain", "Resume_Button"));
        _settingsMenuMain.ButtonElements["Camera_Button"].onClick.AddListener(() => SwitchMenuElement("SettingsMain", "CameraSettings", "CameraSpeed_X", "CameraSpeed_X_Slider"));
        _settingsMenuMain.ButtonElements["Audio_Button"].onClick.AddListener(() => SwitchMenuElement("SettingsMain", "AudioSettings", "MasterVolume", "MasterVolume_Slider"));
        _settingsMenuMain.ButtonElements["Controls_Button"].onClick.AddListener(() => SwitchMenuElement("SettingsMain", "ControlSettings", "RumbleToggle"));
        #endregion

        #region Camera settings events
        _cameraSettings.ToggleElements["InvertCameraToggle"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.InvertCamera = ctx; GameManager.Instance.OnCameraChanged(); });

        _cameraSettings.SliderElements["CameraSpeed_X_Slider"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.CameraSpeed_X = ctx; _cameraSettings.TextElements["CameraSpeed_X_CurrentValue"].text = ctx.ToString(); GameManager.Instance.OnCameraChanged(); });
        _cameraSettings.SliderElements["CameraSpeed_Y_Slider"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.CameraSpeed_Y = ctx; _cameraSettings.TextElements["CameraSpeed_Y_CurrentValue"].text = ctx.ToString(); GameManager.Instance.OnCameraChanged(); });

        _cameraSettings.ButtonElements["CameraToSettingsMenu_Button"].onClick.AddListener(() => SwitchMenuElement("CameraSettings", "SettingsMain", "Controls_Button"));
        #endregion

        #region Audio settings events
        _audioSettings.SliderElements["MasterVolume_Slider"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.MasterVolume = ctx / 100; _audioSettings.TextElements["MasterVolume_CurrentValue"].text = ctx.ToString(); AudioManager.Instance.OnChangedVolume(); });
        _audioSettings.SliderElements["RhythmVolume_Slider"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.RhythmTrackVolume = ctx / 100; _audioSettings.TextElements["RhythmVolume_CurrentValue"].text = ctx.ToString(); AudioManager.Instance.OnChangedVolume(); });
        _audioSettings.SliderElements["MusicVolume_Slider"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.MusicVolume = ctx / 100; _audioSettings.TextElements["MusicVolume_CurrentValue"].text = ctx.ToString(); AudioManager.Instance.OnChangedVolume(); });
        _audioSettings.SliderElements["SoundFXVolume_Slider"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.SoundFXVolume = ctx / 100; _audioSettings.TextElements["SoundFXVolume_CurrentValue"].text = ctx.ToString(); AudioManager.Instance.OnChangedVolume(); });

        _audioSettings.ButtonElements["AudioToSettingsMenu_Button"].onClick.AddListener(() => SwitchMenuElement("AudioSettings", "SettingsMain", "Controls_Button"));
        #endregion

        #region Controls settins
        _controlsSettings.ButtonElements["ControlsToSettingsMenu_Button"].onClick.AddListener(() => SwitchMenuElement("ControlSettings", "SettingsMain", "Controls_Button"));
        _controlsSettings.ToggleElements["AutoTargetingToggle"].onValueChanged.AddListener((ctx) => CurrentGameSettings.AutoTargeting = ctx);
        _controlsSettings.ToggleElements["RumbleToggle"].onValueChanged.AddListener((ctx) => { CurrentGameSettings.Rumble = ctx; RumbleManager.Instance.OnRumbleEnabled(ctx); });
        #endregion

        #region Quit game events
        _areYouSureMenu.ButtonElements["NoQuit_Button"].onClick.AddListener(() => SwitchMenuElement("Quit?", "PauseMenuMain", "Resume_Button"));
        _areYouSureMenu.ButtonElements["YesQuit_Button"].onClick.AddListener(() => Application.Quit());
        #endregion
    }

    public override void OnDisable()
    {
        GameManager.Instance.PlayerController.CanMove = true;
        GameManager.Instance.PlayerController.CanRotate = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Time.timeScale = 1f;
        base.OnDisable();

        if(_inputManager != null)
            _inputManager.SwitchActionMap(_inputManager.inputActions.InGame, _inputManager.inputActions.UI);
        _globalHudManager.ChangeHUDState(GlobalHUDManager.HUDStates.None);

        #region Disable all menus
        _globalHudManager.EnableHUDElement("SettingsMain", false);
        _globalHudManager.EnableHUDElement("CameraSettings", false);
        _globalHudManager.EnableHUDElement("AudioSettings", false);
        _globalHudManager.EnableHUDElement("ControlSettings", false);
        _globalHudManager.EnableHUDElement("Quit?", false);
        #endregion
    }

    public void SwitchMenuElement(string disabledHudElement, string enabledHudElement, string selectedObject)
    {
        _globalHudManager.EnableHUDElement(disabledHudElement, false);
        _globalHudManager.EnableHUDElement(enabledHudElement, true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_globalHudManager.GetHUDElement(enabledHudElement).transform.Find(selectedObject).gameObject);
    }

    public void SwitchMenuElement(string disabledHudElement, string enabledHudElement, string selectedObject, string subObject)
    {
        _globalHudManager.EnableHUDElement(disabledHudElement, false);
        _globalHudManager.EnableHUDElement(enabledHudElement, true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_globalHudManager.GetHUDElement(enabledHudElement).transform.Find(selectedObject).Find(subObject).gameObject);
    }
}