using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CharacterHandler), true), CanEditMultipleObjects]
public class CharacterHandlerEditor : Editor
{
    protected CharacterHandler characterHandler;

    public CharacterHandler.PhysicsMode PhysicsMode;

    public SerializedProperty DetectionObjectsProp;
    public SerializedProperty DetectionDebugProp;
    public SerializedProperty RadialDetectionsProp;

    protected virtual void OnEnable()
    {
        characterHandler = target as CharacterHandler;
        characterHandler.GetComponent<Rigidbody>().hideFlags = HideFlags.HideInInspector;

        DetectionDebugProp = serializedObject.FindProperty("Detections").FindPropertyRelative("DebugMode");
        DetectionObjectsProp = serializedObject.FindProperty("Detections").FindPropertyRelative("DetectionObjects");
        RadialDetectionsProp = serializedObject.FindProperty("Detections").FindPropertyRelative("RadialDetections");

        characterHandler.GetComponent<Rigidbody>().hideFlags = HideFlags.HideInInspector;
        characterHandler.GetCollider<CapsuleCollider>();
        characterHandler.GetRigidBody();
        characterHandler.GetAnimator();

        PhysicsMode = characterHandler.CurrentPhysicsMode;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PrefabUtility.RecordPrefabInstancePropertyModifications(characterHandler);

        DrawHead();

        DrawPropertiesExcluding(serializedObject, "Detections");

        DrawProperties();

        serializedObject.ApplyModifiedProperties();

        if (PhysicsMode != characterHandler.CurrentPhysicsMode)
        {
            PhysicsMode = characterHandler.CurrentPhysicsMode;
            SwitchPhysicsMode();
        }
    }

    protected virtual void DrawHead() { }

    protected virtual void DrawProperties()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detections", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(DetectionDebugProp);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(DetectionObjectsProp);
        EditorGUILayout.PropertyField(RadialDetectionsProp);
    }

    protected virtual void SwitchPhysicsMode()
    {
        characterHandler.CurrentPhysicsMode = PhysicsMode;

        switch (characterHandler.CurrentPhysicsMode)
        {
            case CharacterHandler.PhysicsMode.dynamic:
                characterHandler.AttachedRigidbody.isKinematic = false;
                characterHandler.AttachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                characterHandler.AttachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                characterHandler.AttachedRigidbody.interpolation = RigidbodyInterpolation.None;
                
                characterHandler.CharacterAnimator.applyRootMotion = false;
                break;

            case CharacterHandler.PhysicsMode.kinematic:
                characterHandler.AttachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                characterHandler.AttachedRigidbody.isKinematic = true;
                characterHandler.AttachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                characterHandler.CharacterAnimator.applyRootMotion = false;
                break;

            case CharacterHandler.PhysicsMode.rootmotion:
                characterHandler.CharacterAnimator.applyRootMotion = true;
                break;

            default:
                break;
        }
    }
}
#endif