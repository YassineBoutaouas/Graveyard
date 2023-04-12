using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineController : MonoBehaviour
{
    public static TimelineController Instance;

    public PlayableDirector TimelineDirector;

    public void Awake()
    {
        Instance = this;
    }

    public void Play()
    {
        TimelineDirector.Play();
    }
}
