using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FeedbackManager : MonoBehaviour
{
    public event Action OnFeedbackPulse;
    public event Action OnFeedbackRepeated;
    public event Action OnFeedbackInterpolated;
    public event Action OnFeedbackStop;

    public virtual void Awake() { }

    protected virtual IEnumerator FeedbackPulse(float duration)
    {
        //Set whatever value
        OnFeedbackPulse?.Invoke();
        yield return new WaitForSeconds(duration);
        //Reset all values
        OnFeedbackStop?.Invoke();
    }

    protected virtual IEnumerator FeedbackRepeated(float duration, int repetitions, float waitTime)
    {
        int currentRepetition = 0;
        while (currentRepetition < repetitions)
        {
            //Set whatever value
            OnFeedbackRepeated?.Invoke();

            yield return new WaitForSeconds(duration);
            //Reset whatever value

            yield return new WaitForSeconds(waitTime);
            currentRepetition++;
        }
    }

    protected virtual IEnumerator FeedbackInterpolated(float duration)
    {
        float t = 0;
        while(t < duration)
        {
            yield return null;
            t += Time.deltaTime;
            //Interpolate values and set them
        }

        //Set value to target
        OnFeedbackInterpolated?.Invoke();
    }

    public virtual void FeedBackPulse(float duration)
    {
        StartCoroutine(FeedbackPulse(duration));
    }

    public virtual void FeedBackRepeated(float duration, int repetitions, float waitTime)
    {
        StartCoroutine(FeedbackRepeated(duration, repetitions, waitTime));
    }

    public virtual void SmoothFeedBack(float duration)
    {
        StartCoroutine(FeedbackInterpolated(duration));
    }

    public virtual void StopFeedback()
    {
        OnFeedbackStop?.Invoke();
    }

    public virtual void OnDisable()
    {
        StopFeedback();
    }
}
