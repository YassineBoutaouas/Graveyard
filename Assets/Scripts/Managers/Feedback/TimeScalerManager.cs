using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeScalerManager : MonoBehaviour
{
    public static TimeScalerManager Instance;

    private float _fixedDeltaTime;

    public event Action OnTimeScalePulse;
    public event Action OnTimeScaleRepeated;
    public event Action OnTimeScaleInterpolated;
    public event Action OnTimeScaleStop;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
            Instance = this;

        _fixedDeltaTime = Time.fixedDeltaTime;
    }

    private void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = _fixedDeltaTime * timeScale;
    }

    #region Enumerators
    private IEnumerator ScalePulse(float timeScale, float duration)
    {
        SetTimeScale(timeScale);
        OnTimeScalePulse?.Invoke();

        yield return new WaitForSeconds(duration);
        SetTimeScale(1f);
    }

    private IEnumerator ScaleRepeated(float timeScale, int repetitions, float duration, float waitTime)
    {
        int currentRepetition = 0;

        while(currentRepetition < repetitions)
        {
            SetTimeScale(timeScale);
            OnTimeScaleRepeated?.Invoke();

            yield return new WaitForSeconds(duration);
            SetTimeScale(1f);

            yield return new WaitForSeconds(waitTime);
            currentRepetition++;
        }
    }

    private IEnumerator ScaleInterpolated(float timeScaleEnd, float interpolationTime)
    {
        float t = 0;
        float timeScaleStart = Time.timeScale;
        float currentTimeScale = timeScaleStart;

        while (t < interpolationTime)
        {
            yield return null;
            t += Time.deltaTime;

            currentTimeScale = Mathf.Lerp(timeScaleStart, timeScaleEnd, t / interpolationTime);

            SetTimeScale(currentTimeScale);
        }

        SetTimeScale(timeScaleEnd);
        OnTimeScaleInterpolated?.Invoke();
    }
    #endregion

    public void TimeScalePulse(float timeScale, float duration)
    {
        StartCoroutine(ScalePulse(timeScale, duration));
    }

    public void TimeScaleRepeated(float timeScale, int repetitions, float duration, float waitTime)
    {
        StartCoroutine(ScaleRepeated(timeScale, repetitions, duration, waitTime));
    }

    public void SmoothTimeScale(float timeScaleEnd, float interpolationTime)
    {
        StartCoroutine(ScaleInterpolated(timeScaleEnd, interpolationTime));
    }

    public void SmoothNormalizeTimeScale(float duration)
    {
        SmoothTimeScale(1f, duration);
    }

    public void NormalizeTimeScale()
    {
        OnTimeScaleStop?.Invoke();
        SetTimeScale(1f);
    }

    private void OnDisable()
    {
        NormalizeTimeScale();
    }
}
