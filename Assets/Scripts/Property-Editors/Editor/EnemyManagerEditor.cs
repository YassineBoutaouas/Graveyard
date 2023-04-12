using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(EnemyManager))]
public class EnemyManagerEditor : Editor
{
    private EnemyManager _enemyManager;

    private void OnEnable()
    {
        _enemyManager = target as EnemyManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif