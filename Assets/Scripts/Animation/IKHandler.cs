using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Detections;
using Graveyard.CharacterSystem.Player;
using Graveyard.Combat;

namespace Graveyard.CharacterSystem.Animations.Procedural
{
    [System.Serializable]
    public class IKHandler
    {
        private CharacterHandler _characterHandler;
        private Animator _animator;
        public List<IKSolverObject> IKSolvers = new List<IKSolverObject>();

        [Header("Pelvis values")]
        private float _lastPelvisPosition_Y;
        public float PelvisOffset = 0f;
        [Range(0f, 1f)]
        public float PelvisVerticalSpeed = 0.28f;

        private IKSolverObject _leftLegIK;
        private IKSolverObject _rightLegIK;

        public IKSolverObject GetIKSolverByName(string name)
        {
            IKSolverObject ik = null;
            foreach (IKSolverObject iKSolver in IKSolvers)
                if (iKSolver.Name == name) ik = iKSolver;

            return ik;
        }

        public void Initialize(Animator animator, CharacterHandler character)
        {
            _characterHandler = character;
            _animator = animator;

            _leftLegIK = GetIKSolverByName("Left_Leg");
            _rightLegIK = GetIKSolverByName("Right_Leg");
        }

        public void UpdateIKs()
        {
            if (_characterHandler.CharacterAnimator.applyRootMotion == true) return;

            AdjustBoneTarget(_rightLegIK, ref _rightLegIK.BonePosition);
            AdjustBoneTarget(_leftLegIK, ref _leftLegIK.BonePosition);

            BonePositionSolver(_rightLegIK, ref _rightLegIK.IKPosition, ref _rightLegIK.IKRotation);
            BonePositionSolver(_leftLegIK, ref _leftLegIK.IKPosition, ref _leftLegIK.IKRotation);
        }

        public void AnimatorIK()
        {
            if ((_leftLegIK.Enabled == false || _rightLegIK.Enabled == false) || _characterHandler.CharacterAnimator.applyRootMotion == true) return;

            MovePelvisHeight();

            _animator.SetIKPositionWeight(_rightLegIK.IKGoal, 1);
            _animator.SetIKPositionWeight(_leftLegIK.IKGoal, 1);

            _animator.SetIKRotationWeight(_rightLegIK.IKGoal, _animator.GetFloat(_rightLegIK.AnimatorCurveName));
            _animator.SetIKRotationWeight(_leftLegIK.IKGoal, _animator.GetFloat(_leftLegIK.AnimatorCurveName));

            MoveBoneToIKPoint(_rightLegIK, _rightLegIK.IKGoal, _rightLegIK.IKPosition, _rightLegIK.IKRotation, ref _rightLegIK.LastBonePosition_Y);
            MoveBoneToIKPoint(_leftLegIK, _leftLegIK.IKGoal, _leftLegIK.IKPosition, _leftLegIK.IKRotation, ref _leftLegIK.LastBonePosition_Y);
        }

        private void MovePelvisHeight()
        {
            if (_rightLegIK.IKPosition == Vector3.zero || _leftLegIK.IKPosition == Vector3.zero || _lastPelvisPosition_Y == 0)
            {
                _lastPelvisPosition_Y = _animator.bodyPosition.y;
                return;
            }

            float leftOffsetPos = _leftLegIK.IKPosition.y - _characterHandler.transform.position.y;
            float rightOffsetPos = _rightLegIK.IKPosition.y - _characterHandler.transform.position.y;

            float totalOffSet = (leftOffsetPos < rightOffsetPos) ? leftOffsetPos : rightOffsetPos;

            Vector3 newPelvisPosition = _animator.bodyPosition + Vector3.up * totalOffSet;
            newPelvisPosition.y = Mathf.Lerp(_lastPelvisPosition_Y, newPelvisPosition.y, PelvisVerticalSpeed);

            _animator.bodyPosition = newPelvisPosition;

            _lastPelvisPosition_Y = _animator.bodyPosition.y;
        }

        private void MoveBoneToIKPoint(IKSolverObject ik, AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPosY)
        {
            Vector3 targetIKPosition = _animator.GetIKPosition(foot);

            if (positionIKHolder != Vector3.zero)
            {
                targetIKPosition = _characterHandler.transform.InverseTransformPoint(targetIKPosition);
                positionIKHolder = _characterHandler.transform.InverseTransformPoint(positionIKHolder);

                float interpolatedPosition = Mathf.Lerp(lastFootPosY, positionIKHolder.y, ik.BoneToIKPositionSpeed);
                targetIKPosition.y += interpolatedPosition;

                lastFootPosY = interpolatedPosition;

                targetIKPosition = _characterHandler.transform.TransformPoint(targetIKPosition);

                _animator.SetIKRotation(foot, rotationIKHolder);
            }

            _animator.SetIKPosition(foot, targetIKPosition);
        }

        private void BonePositionSolver(IKSolverObject ikSolver, ref Vector3 boneIKPosition, ref Quaternion boneIKRotation)
        {
            if (ikSolver.Enabled == false) return;

            if (Physics.Raycast(ikSolver.BonePosition, Vector3.down, out RaycastHit hitInfo, ikSolver.RaycastDistance + ikSolver.HeightFromGroundRaycast, ikSolver.LayerMask))
            {
                ikSolver.HitInfo = hitInfo;
                boneIKPosition = ikSolver.BonePosition;
                boneIKPosition.y = hitInfo.point.y + PelvisOffset;
                boneIKRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * _characterHandler.transform.rotation;

                if (ikSolver.DebugMode)
                    Debug.DrawRay(ikSolver.BonePosition, Vector3.down * (ikSolver.RaycastDistance + ikSolver.HeightFromGroundRaycast), Color.cyan);

                return;
            }

            boneIKPosition = Vector3.zero;
        }

        private void AdjustBoneTarget(IKSolverObject ikSolver, ref Vector3 bonePosition)
        {
            if (ikSolver.Enabled == false) return;

            bonePosition = _animator.GetBoneTransform(ikSolver.BodyBone).position;
            bonePosition.y = _characterHandler.transform.position.y + ikSolver.HeightFromGroundRaycast;
        }
    }

    [System.Serializable]
    public class IKSolverObject
    {
        public string Name;

        public bool Enabled;
        public bool DebugMode = false;

        public AvatarIKGoal IKGoal;
        public HumanBodyBones BodyBone;

        [HideInInspector] public Vector3 BonePosition;
        [HideInInspector] public Vector3 IKPosition;
        [HideInInspector] public Quaternion IKRotation;
        [HideInInspector] public float LastBonePosition_Y;

        public string AnimatorCurveName = "";

        public LayerMask LayerMask;

        [Range(0f, 1f)]
        public float BoneToIKPositionSpeed = 0.5f;
        public float HeightFromGroundRaycast = 1.14f;
        public float RaycastDistance = 1.5f;

        public RaycastHit HitInfo;
    }
}