using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "In Game settings", fileName = "InGameSettings")]
public class InGameSettings : ScriptableObject
{
    [Header("Camera settings")]
    [Space(5)]
    public bool InvertCamera = false;
    [Range(100, 500)]
    public float CameraSpeed_X;
    [Range(2, 50)]
    public float CameraSpeed_Y;

    [Header("Audio settings")]
    [Space(5)]
    [Range(0, 1)]
    public float MasterVolume = 1f;
    [Range(0.5f, 1)]
    public float RhythmTrackVolume = 1f;
    [Range(0, 1)]
    public float MusicVolume = 1f;
    [Range(0, 1)]
    public float SoundFXVolume = 1f;

    [Header("Gameplay settings")]
    [Space(5)]
    public bool AutoTargeting = true;
    public bool Rumble = true;

    [Header("Tutorial")]
    [Space(5)]
    public bool HasshownTutorial = false;
}
