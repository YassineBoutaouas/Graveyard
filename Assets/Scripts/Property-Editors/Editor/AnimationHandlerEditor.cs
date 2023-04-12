using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Animations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
[CustomEditor(typeof(AnimationHandler), true)]
public class AnimationHandlerEditor : Editor
{
    protected AnimationHandler animationHandler;
    protected Animator animator;
    protected AnimatorController animatorController;
    protected bool showDictionary;

    protected virtual void OnEnable()
    {
        animationHandler = target as AnimationHandler;
        animator = animationHandler.GetComponent<Animator>();
        animatorController = (AnimatorController) animator.runtimeAnimatorController;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //DrawAnimatorExtension();
    }

    protected void DrawAnimatorExtension()
    {
        EditorGUILayout.HelpBox("Fetch all states and hash strings. Properties are readonly. Note that in order to fetch all states, each one has to have a unique name.", MessageType.Info);

        if (GUILayout.Button("Generate animation hashes"))
        {
            //if (animationHandler.AnimationStates == null) animationHandler.AnimationStates = new Dictionary<string, int>();

            for (int i = 0; i < animator.layerCount; i++)
            {
                Debug.Log("Count layers: " + i);
                foreach (ChildAnimatorState state in animatorController.layers[i].stateMachine.states)
                {
                    Debug.Log("State name: " + state.state.name);
                }
            }
        }
    }
}
#endif