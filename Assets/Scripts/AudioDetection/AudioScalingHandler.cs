using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioScalingHandler : MonoBehaviour
{
    public UnityEvent OnBeat;

    private void Start()
    {
        AudioSpectrumManager.Instance.OnBeatPlay += () => OnBeat?.Invoke();
    }
}