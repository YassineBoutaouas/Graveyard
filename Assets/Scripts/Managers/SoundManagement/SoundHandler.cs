using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    public List<Sound> Sounds = new List<Sound>();

    private void Start()
    {
        foreach (Sound sound in Sounds)
        {
            sound.CreateSound(gameObject.AddComponent<AudioSource>());
            ChangeVolume(sound);

            AudioManager.Instance.OnVolumesChanged += ChangeSoundVolumes;
        }

        GlobalHUDManager.Instance.OnHUDStateChanged += OnHUDChanged;
    }

    public void OnHUDChanged(GlobalHUDManager.HUDStates state)
    {
        if (state != GlobalHUDManager.HUDStates.None)
        {
            foreach (Sound sound in Sounds)
                if(sound.SoundType != Sound.AudioType.UI)
                    MuteSound(sound.Name, true);
        }
        else
        {
            foreach (Sound sound in Sounds)
                MuteSound(sound.Name, false);
        }
    }

    public Sound GetSound(string name, out Sound sound)
    {
        sound = Sounds.Find(s => s.Name == name);
        return sound;
    }

    public void ChangeSoundVolumes()
    {
        foreach (Sound sound in Sounds)
            ChangeVolume(sound);
    }

    public void ChangeVolume(string name)
    {
        if (GetSound(name, out Sound sound) == null) return;

        ChangeVolume(sound);
    }

    public void ChangeVolume(Sound sound)
    {
        if (sound == null) return;

        float multiplier = 1;

        if (sound.SoundType == Sound.AudioType.SoundFX)
            multiplier = AudioManager.Instance.Settings.SoundFXVolume;
        else if (sound.SoundType == Sound.AudioType.Music)
            multiplier = AudioManager.Instance.Settings.MusicVolume;
        else if(sound.SoundType == Sound.AudioType.Rhythm)
            multiplier = AudioManager.Instance.Settings.RhythmTrackVolume;

        sound.Source.volume = sound.Volume * AudioManager.Instance.Settings.MasterVolume * multiplier;
    }

    public void PlaySound(string soundName)
    {
        if (GetSound(soundName, out Sound sound) == null) return;
        if (sound.Source == null) return;

        AudioManager.Instance.OnPlaySound(soundName, this);
        sound.Source.Play();
    }

    public void StopSound(string soundName)
    {
        if (GetSound(soundName, out Sound sound) == null) return;

        AudioManager.Instance.OnStopSound(soundName, this);
        sound.Source.Stop();
    }

    public bool CheckIsPlaying(string soundName)
    {
        if (GetSound(soundName, out Sound sound) == null) return false;
        return sound.Source.isPlaying;
    }

    public void MuteSound(string soundName, bool muted)
    {
        if (GetSound(soundName, out Sound sound) == null) return;
        sound.Source.mute = muted;
    }

    public void PauseAudio(string soundName, bool isPaused)
    {
        if (GetSound(soundName, out Sound sound) == null) return; 

        if (isPaused)
        {
            AudioManager.Instance.OnPauseSound(soundName, this);
            sound.Source.Pause();
        }
        else
        {
            AudioManager.Instance.OnResumeSound(soundName, this);
            sound.Source.UnPause();
        }
    }

    private void OnDestroy()
    {
        GlobalHUDManager.Instance.OnHUDStateChanged -= OnHUDChanged;
    }
}
