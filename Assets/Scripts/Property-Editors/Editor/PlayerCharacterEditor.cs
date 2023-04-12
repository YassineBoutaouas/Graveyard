using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Player;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PlayerCharacterController), true)]
public class PlayerCharacterEditor : CharacterHandlerEditor
{
    private PlayerCharacterController _playerController;

    public bool IsFoldedOut;

    protected override void OnEnable()
    {
        base.OnEnable();
        _playerController = (PlayerCharacterController)characterHandler;
    }
}
#endif