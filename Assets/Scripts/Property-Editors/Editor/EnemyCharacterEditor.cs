using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Enemy;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(EnemyCharacterHandler), true)]
public class EnemyCharacterEditor : CharacterHandlerEditor
{
    private EnemyCharacterHandler _enemyController;
    private EnemyManager _enemyManager;
    private LayerMask _layerMask;
    private EnemyGroupController _group;

    protected override void OnEnable()
    {
        base.OnEnable();
        _enemyController = (EnemyCharacterHandler)characterHandler;
        _enemyManager = FindObjectOfType<EnemyManager>();
        _layerMask = LayerMask.GetMask("Ground", "Default");
    }
}
#endif