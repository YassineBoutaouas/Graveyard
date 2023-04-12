using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(EnemyGroupController))]
public class EnemyGroupEditor : Editor
{
    private EnemyGroupController _enemyGroup;
    private EnemyManager _enemyManager;
    private LayerMask _layerMask;

    private void OnEnable()
    {
        _enemyGroup = target as EnemyGroupController;
        _enemyManager = FindObjectOfType<EnemyManager>();

        _layerMask = LayerMask.GetMask("Default", "Ground");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Spawn Enemies"))
        {
            for (int i = 0; i < _enemyGroup.EnemyCount; i++)
                AddEnemy();
        
            _enemyManager.FetchEnemies();
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add new enemy"))
            AddEnemy();

        serializedObject.ApplyModifiedProperties();
    }

    private void AddEnemy()
    {
        GameObject enemyObject = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load("Enemy"));
        enemyObject.name = "Enemy (" + _enemyGroup.transform.childCount + ")";

        EnemyCharacterHandler enemyHandler = enemyObject.GetComponent<EnemyCharacterHandler>();

        Vector2 randomPosition = Random.insideUnitCircle * _enemyGroup.Radius;
        enemyObject.transform.position = new Vector3(randomPosition.x + _enemyGroup.transform.position.x, _enemyGroup.transform.position.y, randomPosition.y + _enemyGroup.transform.position.z);
        enemyObject.transform.localRotation = Quaternion.LookRotation(Vector3.Scale(_enemyGroup.transform.position - enemyObject.transform.position, Vector3.right + Vector3.forward));

        if (Physics.Raycast(enemyObject.transform.position, Vector3.down, out RaycastHit hit, 3f, _layerMask, QueryTriggerInteraction.Ignore))
            enemyObject.transform.position = hit.point;

        enemyHandler.GroupID = _enemyGroup.ID;

        enemyHandler.transform.parent = _enemyGroup.transform;
    }
}
#endif