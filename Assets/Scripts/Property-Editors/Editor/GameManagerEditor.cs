using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private GameManager _gameManager;

    private static bool _foldedOut;

    private SerializedProperty _minutesProperty;
    private SerializedProperty _secondsProperty;

    private SerializedProperty _startingScaleProperty;
    private SerializedProperty _animationDurationProperty;
    private SerializedProperty _displayDurationProperty;
    private SerializedProperty _interpolationCurveProperty;

    private GUIContent _timerLabel;
    private GUIContent _inbetweenLabel;

    private void OnEnable()
    {
        _gameManager = target as GameManager;

        _minutesProperty = serializedObject.FindProperty(nameof(GameManager.LevelTimer)).FindPropertyRelative(nameof(Timer.MaxMinutes));
        _secondsProperty = serializedObject.FindProperty(nameof(GameManager.LevelTimer)).FindPropertyRelative(nameof(Timer.MaxSeconds));

        _startingScaleProperty = serializedObject.FindProperty(nameof(GameManager.LevelTimer)).FindPropertyRelative(nameof(Timer.StartingScale));
        _animationDurationProperty = serializedObject.FindProperty(nameof(GameManager.LevelTimer)).FindPropertyRelative(nameof(Timer.AnimationDuration));
        _displayDurationProperty = serializedObject.FindProperty(nameof(GameManager.LevelTimer)).FindPropertyRelative(nameof(Timer.DisplayDuration));
        _interpolationCurveProperty = serializedObject.FindProperty(nameof(GameManager.LevelTimer)).FindPropertyRelative(nameof(Timer.InterpolationCurve));

        _timerLabel = new GUIContent("Timer");
        _inbetweenLabel = new GUIContent("");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PrefabUtility.RecordPrefabInstancePropertyModifications(_gameManager);

        DrawPropertiesExcluding(serializedObject, nameof(GameManager.LevelTimer));

        _foldedOut = EditorGUILayout.Foldout(_foldedOut, "Level Timer", true);
        if (_foldedOut)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_minutesProperty, _timerLabel);
            EditorGUILayout.PropertyField(_secondsProperty, _inbetweenLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(_startingScaleProperty);
            EditorGUILayout.PropertyField(_animationDurationProperty);
            EditorGUILayout.PropertyField(_displayDurationProperty);
            EditorGUILayout.PropertyField(_interpolationCurveProperty);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif