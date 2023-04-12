using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public enum AudioType { SoundFX, Music, UI, Rhythm }
    public AudioType SoundType;

    public string Name;
    public AudioClip Clip;
    public bool IsMuted;
    public bool IsLooped;
    public bool PlayOnAwake;

    [Header("Audio values")]
    [Space(5)]
    [Range(0f, 1f)]
    public float Volume = 1;
    [Range(0.1f, 3f)]
    public float Pitch = 1;
    [Range(-1f, 1f)]
    public float StereoPan = 0;
    [Range(0f, 1f)]
    public float SpatialBlend = 0;

    [Header("3D settings")]
    [Space(5)]
    [Range(0f, 5f)]
    public float DopplerLevel = 1;
    [Range(0, 360)]
    public float Spread = 0;
    public AudioRolloffMode AudioRolloff = AudioRolloffMode.Logarithmic;
    public float MinDistance = 1;
    public float MaxDistance = 500;

    [HideInInspector] public AudioSource Source;

    public void CreateSound(AudioSource source)
    {
        Source = source;
        Source.clip = Clip;
        Source.playOnAwake = PlayOnAwake;
        Source.mute = IsMuted;
        Source.loop = IsLooped;

        Source.volume = Volume;

        Source.pitch = Pitch;
        Source.panStereo = StereoPan;
        Source.spatialBlend = SpatialBlend;

        Source.dopplerLevel = DopplerLevel;
        Source.spread = Spread;
        Source.rolloffMode = AudioRolloff;
        Source.minDistance = MinDistance;
        Source.maxDistance = MaxDistance;
    }
}