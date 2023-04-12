using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public event Action<string, SoundHandler> OnSoundPlay;
    public event Action<string, SoundHandler> OnSoundStop;
    public event Action<string, SoundHandler> OnSoundPause;
    public event Action<string, SoundHandler> OnSoundResume;
    public event Action OnVolumesChanged;

    public InGameSettings Settings;

    public void OnChangedVolume() { OnVolumesChanged?.Invoke(); }
    public void OnPlaySound(string name, SoundHandler handler) { OnSoundPlay?.Invoke(name, handler); }
    public void OnStopSound(string name, SoundHandler handler) { OnSoundStop?.Invoke(name, handler); }
    public void OnPauseSound(string name, SoundHandler handler) { OnSoundPause?.Invoke(name, handler); }
    public void OnResumeSound(string name, SoundHandler handler) { OnSoundResume?.Invoke(name, handler); }

    public List<SoundHandler> SoundHandlers = new List<SoundHandler>();

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        foreach (SoundHandler handler in FindObjectsOfType<SoundHandler>())
            SoundHandlers.Add(handler);
    }
}