using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;

public class CameraShakerManager : MonoBehaviour
{
    public static CameraShakerManager Instance;

    public CinemachineBrain cinemachineBrain;
    public NoiseSettings ShakeNoise;

    public event Action OnShakePulse;
    public event Action OnShakeRepeated;
    public event Action OnShakeInterpolated;
    public event Action OnShakeStop;

    private GameObject _camObject;
    private NoiseSettings _standardNoise;
    private CinemachineBasicMultiChannelPerlin _channelPerlin;

    private float _defaultStrength;
    private float _defaultFrequency;

    private CinemachineBasicMultiChannelPerlin GetNoise()
    {
        CinemachineBasicMultiChannelPerlin noise = null;

        _camObject = CameraManager.Instance.GetVirtualCamera("FreeLook").VirtualCameraGameObject;

        if (_camObject.TryGetComponent(out CinemachineVirtualCamera vCam))
            noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        else if (_camObject.TryGetComponent(out CinemachineFreeLook freeLook))
            noise = freeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        return noise;
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
            Instance = this;
    }

    private void Start()
    {
        _channelPerlin = GetNoise();
        _standardNoise = GetNoise().m_NoiseProfile;
        _defaultStrength = _channelPerlin.m_AmplitudeGain;
        _defaultFrequency = _channelPerlin.m_FrequencyGain;

        TimelineController.Instance.Play();
    }

    private void SetShakeValues(NoiseSettings n, float s, float f)
    {
        if (_channelPerlin == null) return;

        _channelPerlin.m_NoiseProfile = n;
        _channelPerlin.m_AmplitudeGain = s;
        _channelPerlin.m_FrequencyGain = f;
    }

    #region Enumerators
    private IEnumerator ShakePulse(float strength, float frequency, float duration)
    {
        _channelPerlin = GetNoise();

        SetShakeValues(ShakeNoise, strength, frequency);
        OnShakePulse?.Invoke();

        yield return new WaitForSeconds(duration);
        SetShakeValues(_standardNoise, _defaultStrength, _defaultFrequency);
    }

    private IEnumerator ShakeRepeated(float strength, float frequency, float duration, int repetitions, float waitTime)
    {
        _channelPerlin = GetNoise();
        int currentRepetition = 0;

        while (currentRepetition < repetitions)
        {
            SetShakeValues(ShakeNoise, strength, frequency);
            OnShakeRepeated?.Invoke();

            yield return new WaitForSeconds(duration);
            SetShakeValues(_standardNoise, _defaultStrength, _defaultFrequency);
            OnShakeStop?.Invoke();

            yield return new WaitForSeconds(waitTime);
            currentRepetition++;
        }
    }

    private IEnumerator ShakeInterpolated(float strengthStart, float frequencyStart, float strengthEnd, float frequencyEnd, float step)
    {
        _channelPerlin = GetNoise();
        float currentStrength = strengthStart;
        float currentFrequency = frequencyStart;
        float t = 0f;
        while (Mathf.Abs(currentStrength - strengthEnd) > 0.1f && Mathf.Abs(currentFrequency - frequencyEnd) > 0.1f)
        {
            yield return null;
            currentStrength = Mathf.Lerp(strengthStart, strengthEnd, t);
            currentFrequency = Mathf.Lerp(frequencyStart, frequencyEnd, t);

            SetShakeValues(ShakeNoise, currentStrength, currentFrequency);

            t += step;
        }

        SetShakeValues(ShakeNoise, strengthEnd, frequencyEnd);
        OnShakeInterpolated?.Invoke();
    }
    #endregion

    #region Public methods
    public void PulseShake(float strength, float frequency, float duration)
    {
        StartCoroutine(ShakePulse(strength, frequency, duration));
    }

    public void RepeatedShake(float strength, float frequency, float duration, int repetitions, float waitTime)
    {
        StartCoroutine(ShakeRepeated(strength, frequency, duration, repetitions, waitTime));
    }

    public void SmoothShake(float strengthStart, float frequencyStart, float strengthEnd, float frequencyEnd, float step)
    {
        StartCoroutine(ShakeInterpolated(strengthStart, frequencyStart, strengthEnd, frequencyEnd, step));
    }

    public void SmoothStopShake(float strengthStart, float frequencyStart, float step)
    {
        StartCoroutine(ShakeInterpolated(strengthStart, frequencyStart, _defaultStrength, _defaultFrequency, step));
    }

    public void StopShake()
    {
        _channelPerlin = GetNoise();
        SetShakeValues(_standardNoise, _defaultStrength, _defaultFrequency);

        OnShakeStop?.Invoke();
    }
    #endregion

    private void OnDisable()
    {
        if(cinemachineBrain != null)
            StopShake();
    }
}