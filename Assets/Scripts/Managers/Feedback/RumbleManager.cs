using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager Instance;

    public event Action OnRumblePulse;
    public event Action OnRumbleRepeated;
    public event Action OnRumbleInterpolated;
    public event Action OnRumbleStop;

    public bool Enabled = true;
    public void OnRumbleEnabled(bool ctx) { Enabled = ctx; if (ctx == false) StopRumble(); }

    public Gamepad GetGamepad() { return Gamepad.current; }

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
            Instance = this;
    }

    private void Start()
    {
        Enabled = GameManager.Instance.Settings.Rumble;
    }

    private void SetRumble(float lowMagnitude, float highMagnitude)
    {
        Gamepad g = GetGamepad();
        g?.SetMotorSpeeds(lowMagnitude, highMagnitude);
    }

    #region Enumerators
    private IEnumerator RumblePulse(float low, float high, float duration)
    {
        Gamepad g = GetGamepad();

        SetRumble(low, high);
        OnRumblePulse?.Invoke();
        yield return new WaitForSeconds(duration);
        OnRumbleStop?.Invoke();
        SetRumble(0, 0);
    }

    private IEnumerator RumbleRepeated(float low, float high, float duration, int repetitions, float waitTime)
    {
        Gamepad g = GetGamepad();
        int currentRepetition = 0;

        while (currentRepetition < repetitions)
        {
            SetRumble(low, high);
            OnRumbleRepeated?.Invoke();

            yield return new WaitForSeconds(duration);
            SetRumble(0, 0);

            yield return new WaitForSeconds(waitTime);
            currentRepetition += 1;
        }
    }

    private IEnumerator RumbleInterpolated(float lowStart, float highStart, float lowEnd, float highEnd, float step)
    {
        Gamepad g = GetGamepad();
        float currentlowAmplitude = lowStart;
        float currentHighAmplitude = lowEnd;
        float t = 0;
        while (Mathf.Abs(currentlowAmplitude - lowEnd) > 0.2f && Mathf.Abs(currentHighAmplitude - highEnd) > 0.2f)
        {
            yield return null;
            currentlowAmplitude = Mathf.Lerp(lowStart, lowEnd, t);
            currentHighAmplitude = Mathf.Lerp(highStart, highEnd, t);

            SetRumble(currentlowAmplitude, currentHighAmplitude);
            t += step;
        }

        SetRumble(lowEnd, highEnd);
        OnRumbleInterpolated?.Invoke();
    }
    #endregion

    #region Public methods
    public void PulseRumble(float low, float high, float duration)
    {
        if(Enabled)
            StartCoroutine(RumblePulse(low, high, duration));
    }

    public void RepeatRumble(float low, float high, float duration, int repetitions, float waitTime)
    {
        if (Enabled)
            StartCoroutine(RumbleRepeated(low, high, duration, repetitions, waitTime));
    }

    public void SmoothRumble(float lowStart, float highStart, float lowEnd, float highEnd, float step)
    {
        if (Enabled)
            StartCoroutine(RumbleInterpolated(lowStart, highStart, lowEnd, highEnd, step));
    }

    public void SmoothStopRumble(float lowStart, float highStart, float step)
    {
        if (Enabled)
            StartCoroutine(RumbleInterpolated(lowStart, highStart, 0, 0, step));
    }

    public void StopRumble()
    {
        Gamepad gamepad = GetGamepad();

        StopAllCoroutines();
        SetRumble(0, 0);
        OnRumbleStop?.Invoke();
    }
    #endregion

    private void OnDisable()
    {
        StopRumble();
    }
}