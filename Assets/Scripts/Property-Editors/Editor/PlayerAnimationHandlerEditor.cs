using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Animations.Player;
using Graveyard.CharacterSystem.Animations.Procedural;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
[CustomEditor(typeof(AnimationHandlerPlayer), true)]
public class PlayerAnimationHandlerEditor : AnimationHandlerEditor
{
    private AnimationHandlerPlayer _animationHandler;
    private bool _foldOut;

    private SerializedProperty _pelvisOffsetProp;
    private SerializedProperty _pelvisVerticalSpeedProp;
    private SerializedProperty _ikHandlerProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        _animationHandler = target as AnimationHandlerPlayer;

        _pelvisOffsetProp = serializedObject.FindProperty("IkHandler").FindPropertyRelative("PelvisOffset");
        _pelvisVerticalSpeedProp = serializedObject.FindProperty("IkHandler").FindPropertyRelative("PelvisVerticalSpeed");
        _ikHandlerProp = serializedObject.FindProperty("IkHandler").FindPropertyRelative("IKSolvers");
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "IkHandler");

        serializedObject.Update();
        PrefabUtility.RecordPrefabInstancePropertyModifications(_animationHandler);

        _foldOut = EditorGUILayout.Foldout(_foldOut, "IK Handler", true);

        if (_foldOut)
        {
            if (GUILayout.Button("Create IKs"))
                SetUpLegIKs();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_pelvisOffsetProp);
            EditorGUILayout.PropertyField(_pelvisVerticalSpeedProp);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_ikHandlerProp);
        }

        //DrawAnimatorExtension();

        serializedObject.ApplyModifiedProperties();
    }

    private void SetUpLegIKs()
    {
        if (_animationHandler.IkHandler.GetIKSolverByName("Left_Leg") == null)
        {
            IKSolverObject iksolver = new IKSolverObject
            {
                Name = "Left_Leg",
                Enabled = true,
                IKGoal = AvatarIKGoal.LeftFoot,
                BodyBone = HumanBodyBones.LeftFoot,
                AnimatorCurveName = "LeftFootIKCurve",
                BoneToIKPositionSpeed = 0.5f,
                HeightFromGroundRaycast = 0.8f,
                RaycastDistance = 0.8f
            };

            _animationHandler.IkHandler.IKSolvers.Add(iksolver);
        }

        if(_animationHandler.IkHandler.GetIKSolverByName("Right_Leg") == null)
        {
            IKSolverObject iksolver = new IKSolverObject
            {
                Name = "Right_Leg",
                Enabled = true,
                IKGoal = AvatarIKGoal.RightFoot,
                BodyBone = HumanBodyBones.RightFoot,
                AnimatorCurveName = "RightFootIKCurve",
                BoneToIKPositionSpeed = 0.5f,
                HeightFromGroundRaycast = 0.8f,
                RaycastDistance = 0.8f
            };

            _animationHandler.IkHandler.IKSolvers.Add(iksolver);
        }
    }
}
#endif