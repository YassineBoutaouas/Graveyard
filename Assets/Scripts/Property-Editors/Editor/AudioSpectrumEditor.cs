using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(AudioSpectrumManager), true)]
public class AudioSpectrumEditor : Editor
{
    private AudioSpectrumManager _audioSpectrumManager;
    private int _beats;
    private float _ms;
    private float _s;
    private bool _showThreshholds;
    private string[] _beatEvaluations;

    private void OnEnable()
    {
        _audioSpectrumManager = target as AudioSpectrumManager;
        _beatEvaluations = Enum.GetNames(typeof(AudioSpectrumManager.BeatEvaluation));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _audioSpectrumManager.RestingTime = Mathf.Clamp(_audioSpectrumManager.RestingTime, 0, 1.ConvertBeatsToSeconds(_audioSpectrumManager.BeatsPerMinute));

        EditorGUILayout.Space();
        if (_showThreshholds = EditorGUILayout.Foldout(_showThreshholds, "Normalized Performance Threshholds", true))
        {
            EditorGUILayout.FloatField("Perfect", 1 - _audioSpectrumManager.PerformanceThreshholds[0]);
            EditorGUILayout.FloatField("Good", 1 - _audioSpectrumManager.PerformanceThreshholds[1]);
            EditorGUILayout.FloatField("Bad", 1 - 0);
        }

        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        _beats = EditorGUILayout.IntField("BPM", _beats);
        if(GUILayout.Button("Calculate BPM in seconds"))
            CalculateBPMInSeconds();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.FloatField("MS", _ms);
        EditorGUILayout.FloatField("S", _s);
    }

    private void CalculateBPMInSeconds()
    {
        _ms = _beats.ConvertBeatsToMilliseconds(_audioSpectrumManager.BeatsPerMinute);
        _s = _beats.ConvertBeatsToSeconds(_audioSpectrumManager.BeatsPerMinute);
    }
}
#endif