using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Player;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event Action OnCameraValuesChanged;
    public void OnCameraChanged() { OnCameraValuesChanged?.Invoke(); }

    public InGameSettings Settings;

    [ReadOnly] public PlayerCharacterController PlayerController;
    [ReadOnly] public Cinemachine.CinemachineFreeLook PlayerCamera;

    private InputManager _inputManager;
    private HUDManager _globalHUDManager;

    public float SlowedTime = 0.3f;
    public Timer LevelTimer;
    public float GameOverDelayTime = 3f;

    private float _physicsStep;
    private bool _isSlowedDown = false;

    public static GameManager GetInstance() { return Instance; }

    private void Awake()
    {
        Instance = this;

        _inputManager = InputManager.GetInstance();
        _inputManager.Initialize();

        _physicsStep = Time.fixedDeltaTime;

        //_inputManager.EnableActionMap(_inputManager.inputActions.Debug);
        //_inputManager.inputActions.Debug.SlowMotion.performed += _ => ToggleSlowMotion();

        PlayerController = FindObjectOfType<PlayerCharacterController>();
        PlayerCamera = FindObjectOfType<Cinemachine.CinemachineFreeLook>();

        Application.targetFrameRate = 100;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

    public void OnTimelineStart()
    {
        foreach (Renderer renderer in PlayerController.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }

        PlayerController.SwitchPhysicsMode(Graveyard.CharacterSystem.CharacterHandler.PhysicsMode.rootmotion);
        _inputManager.DisableActionMap(_inputManager.inputActions.InGame);

        PlayerController.CanMove = false;
        PlayerController.CanRotate = false;
        PlayerController.Velocity = Vector3.zero;

        GlobalHUDManager.Instance.EnableHUDElement("ComboCounter", false);
        GlobalHUDManager.Instance.EnableHUDElement("BeatInformation", false);
        GlobalHUDManager.Instance.EnableHUDElement("BeatBarProcedural", false);
        GlobalHUDManager.Instance.EnableHUDElement("RaverIndicator", false);
    }

    public void OnTimelineEnd()
    {
        foreach (Renderer renderer in PlayerController.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = true;
        }

        Cinemachine.CinemachineFreeLook freeLook = (Cinemachine.CinemachineFreeLook)CameraManager.Instance.GetVirtualCamera("FreeLook");
        freeLook.m_XAxis.Value = 45f;
        CameraManager.Instance.GetVirtualCamera("DollyCamera").gameObject.SetActive(false);

        PlayerController.SwitchPhysicsMode(Graveyard.CharacterSystem.CharacterHandler.PhysicsMode.dynamic);

        PlayerController.CanMove = true;
        PlayerController.CanRotate = true;
        _inputManager.EnableActionMap(_inputManager.inputActions.InGame);

        _globalHUDManager = GlobalHUDManager.Instance;
        LevelTimer.Initialize();
        _globalHUDManager.EnableHUDElement("BeatBarProcedural", true);
        _globalHUDManager.EnableHUDElement("Black_FadeIn", false);

        LevelTimer.StartTimerAnimation();
        LevelTimer.OnTimeOut += () => GameOver();

        ChangeCamera();

        OnCameraValuesChanged += ChangeCamera;

        if (Settings.HasshownTutorial == false)
        {
            _globalHUDManager.EnableHUDElement("Tutorial", true);
            Settings.HasshownTutorial = true;
        }

        GlobalHUDManager.Instance.EnableHUDElement("ComboCounter", true);
        GlobalHUDManager.Instance.EnableHUDElement("BeatInformation", true);
        GlobalHUDManager.Instance.EnableHUDElement("BeatBarProcedural", true);
        GlobalHUDManager.Instance.EnableHUDElement("RaverIndicator", true);
    }

    public void ChangeCamera()
    {
        PlayerCamera.m_XAxis.m_MaxSpeed = Settings.CameraSpeed_X;
        PlayerCamera.m_YAxis.m_MaxSpeed = Settings.CameraSpeed_Y;
        PlayerCamera.m_YAxis.m_InvertInput = Settings.InvertCamera;
    }

    public void ToggleSlowMotion()
    {
        _isSlowedDown = !_isSlowedDown;
        float t = _isSlowedDown ? SlowedTime : 1f;
        Time.fixedDeltaTime = _physicsStep * t;
        Time.timeScale = t;
    }

    private void GameOver()
    {
        _globalHUDManager.EnableHUDElement("GameOver", true);
    }
}

[System.Serializable]
public class Timer
{
    public event Action<float> OnClockDamage;
    public void OnClockDamaged(float damage) { OnClockDamage?.Invoke(damage); AddSeconds(-damage); RumbleManager.Instance.PulseRumble(1, 1, 0.25f); }
    public event Action OnReset;
    public event Action OnTimerStopped;
    public event Action<float> OnAddSeconds;
    public event Action<float> OnAddMinutes;

    public event Action OnStart;
    public event Action OnTimeOut;

    #region Public variables
    public float MaxMinutes = 1;
    public float MaxSeconds = 12;

    [HideInInspector] public bool IsPaused;
    [HideInInspector] public bool IsRunning;

    [Header("Pop out animation values")]
    public Vector2 StartingScale;
    public float AnimationDuration;
    public float DisplayDuration;
    public AnimationCurve InterpolationCurve;
    #endregion

    #region private Variables
    private TMPro.TextMeshProUGUI _timerUI;
    private Vector2 _targetPosition;
    private float _startTime;
    private float _currentTime;
    private float _timePast;
    private float _currentMinutes;
    private float _currentSeconds;
    private float _relativeTimeLeft;
    #endregion

    #region Properties
    public float RelativeTimeLeft { get { return _relativeTimeLeft; } }
    public float CurrentMinutes { get { return _currentMinutes; } }
    public float CurrentSeconds { get { return _currentSeconds; } }
    public float StartingTime { get { return _startTime; } }
    public float CurrentTime { get { return _currentTime; } }
    #endregion

    public void Initialize()
    {
        GlobalHUDManager.Instance.EnableHUDElement("BeatInformation", true);
        _timerUI = GlobalHUDManager.Instance.GetHUDElement("BeatInformation").TextElements["Timer"];
    }

    public void StartTimer() { StartTimer(MaxMinutes, MaxSeconds); }

    public void StartTimer(float minutes, float seconds)
    {
        _currentSeconds = seconds;
        _currentMinutes = minutes;
        _currentTime = _currentMinutes * 60 + MaxSeconds;
        _startTime = _currentTime;

        OnStart?.Invoke();
        IsRunning = true;
        GameManager.Instance.StartCoroutine(UpdateTimer());
    }

    public float TimePast() { return _startTime - _currentTime; }

    public void StartTimerAnimation()
    {
        _timerUI.text = string.Format("{0:00}:{1:00}", MaxMinutes, MaxSeconds);

        GameManager.Instance.PlayerController.CanMove = true;
        GameManager.Instance.PlayerController.CanRotate = true;
        StartTimer();
    }

    public IEnumerator UpdateTimer()
    {
        while (_currentTime > 0)
        {
            yield return null;
            if (!IsPaused)
            {
                _currentTime -= Time.deltaTime;
                _relativeTimeLeft = _currentTime / _startTime;

                _currentMinutes = Mathf.FloorToInt(_currentTime / 60);
                _currentSeconds = Mathf.FloorToInt(_currentTime % 60);
                _timerUI.text = string.Format("{0:00}:{1:00}", _currentMinutes, _currentSeconds);
            }
        }

        OnTimeOut?.Invoke();

        IsRunning = false;
        _timerUI.text = "00:00";
        _currentTime = 0;
    }

    public void ResetTimer()
    {
        OnReset?.Invoke();
        _currentTime = _startTime;

        GameManager.Instance.StopCoroutine(UpdateTimer());
        GameManager.Instance.StartCoroutine(UpdateTimer());
    }

    public void StopTimer()
    {
        IsPaused = true;
        GameManager.Instance.StopCoroutine(UpdateTimer());
        OnTimerStopped?.Invoke();
    }

    public void AddSeconds(float seconds)
    {
        OnAddSeconds?.Invoke(seconds);
        _currentTime += seconds;
    }

    public void AddMinutes(float minutes)
    {
        OnAddMinutes?.Invoke(minutes);
        _currentMinutes += minutes;
    }
}
