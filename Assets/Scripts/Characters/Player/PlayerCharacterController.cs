using UnityEngine;
using Graveyard.Combat;
using Graveyard.Abilities;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

namespace Graveyard.CharacterSystem.Player
{
    public class PlayerCharacterController : CharacterHandler
    {
        private InputManager _inputManager;
        
        #region Properties
        public CapsuleCollider AttachedCapsuleCollider { get { return _capsuleCollider; } }
        private AbilityManager _abilityManager;
        public AbilityManager AbilityManager { get { return _abilityManager; } }
        private AttackHandler _attackHandler;
        public AttackHandler CharacterAttackHandler { get { return _attackHandler; } }
        public bool IsDamagable { get; set; }
        #endregion

        #region const values
        public readonly float Walking_Treshhold = 0.2f;
        #endregion

        private CapsuleCollider _capsuleCollider;

        public override void GetCollider<T>()
        {
            base.GetCollider<CapsuleCollider>();
            _capsuleCollider = (CapsuleCollider)AttachedCollider;
        }

        protected override void Start()
        {
            base.Start();
            _inputManager = InputManager.GetInstance();
            _abilityManager = GetComponent<AbilityManager>();
            _attackHandler = GetComponent<AttackHandler>();

            IsDamagable = true;

            OnCharacterSetup();

            SetInputListeners();

            CurrentOrientationMethod = ApplyStandardRotation;

            CanMove = true;
            CanRotate = true;
        }

        private void SetInputListeners()
        {
            InputActions.InGameActions ingameMap = _inputManager.inputActions.InGame;

            ingameMap.Jump.performed += CallJump;
            ingameMap.Jump.canceled += CancelJump;

            ingameMap.Sprint.performed += CallSprint;
            ingameMap.Sprint.canceled += CancelSprint;

            ingameMap.DashRoll.performed += CallDodge;
        }

        protected override void FixedUpdate()
        {
            if (CharacterAnimator.applyRootMotion) return;

            DesiredTargetDirection = _inputManager.MoveInput;
            base.FixedUpdate();
        }

        public override void SwitchPhysicsMode(PhysicsMode physicsMode)
        {
            base.SwitchPhysicsMode(physicsMode);

            //Schreibs um!
            if (shellDetection != null)
                shellDetection.detectionObject.transform.localPosition = Vector3.zero + Vector3.up * _capsuleCollider.radius - Vector3.up * ShellRadius;
        }

        protected override void UpdatePhysicsDetections()
        {
            base.UpdatePhysicsDetections();

            Detections.UpdateRadialDetection(CalculateRelativeMovement().normalized, ref topStepCheck);
            Detections.UpdateRadialDetection(CalculateRelativeMovement().normalized, ref bottomStepCheck);
        }

        protected override void ComputeVelocity()
        {
            base.ComputeVelocity();
            ApplyStandardMovement();
        }

        #region Rotation methods
        public void ApplyStandardRotation()
        {
            if (!CanUpdateRotation()) return;

            if (_inputManager.MoveInput.magnitude > 0)
                SmoothRotateTowards(CalculateRelativeMovement());
        }

        public void ApplyCameraDrivenRotation()
        {
            if (!CanUpdateRotation()) return;

            SmoothRotateTowards(GetCameraForwardDirection());
        }
        #endregion

        #region Relative movement
        public Vector3 GetCameraForwardDirection()
        {
            return Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        }

        public Vector3 CalculateRelativeMovement()
        {
            return _inputManager.MoveInput.y * GetCameraForwardDirection() + _inputManager.MoveInput.x * Camera.main.transform.right;
        }
        #endregion

        public void ApplyStandardMovement()
        {
            if (!CanUpdateMovement()) return;

            Vector3 velocity = AttachedRigidbody.isKinematic ? Velocity : AttachedRigidbody.velocity;
            float interpolation = _inputManager.MoveInput.magnitude > Walking_Treshhold ? Stats.GroundAcceleration : Stats.GroundDeceleration;
            Velocity = new Vector3(Mathf.Lerp(Velocity.x, ProjectForceOnWall(Stats.MovementSpeed * SpeedMultiplier * CalculateRelativeMovement()).x, Time.fixedDeltaTime * interpolation), velocity.y, Mathf.Lerp(Velocity.z, ProjectForceOnWall(Stats.MovementSpeed * SpeedMultiplier * CalculateRelativeMovement()).z, Time.fixedDeltaTime * interpolation));

            if (_inputManager.MoveInput.magnitude > Walking_Treshhold) HandleStairs();
        }

        #region Projection, Gravity and slopes
        protected override bool UseGravity()
        {
            return base.UseGravity() && !ShellCollision || IsGrounded && _inputManager.MoveInput.magnitude > 0;
        }

        protected override bool UsePlaneProjection()
        {
            return base.UsePlaneProjection() && _inputManager.MoveInput.magnitude > Walking_Treshhold && IsGrounded; //|| OverlapHitNormal.y < MinVerticalGroundNormal && !(IsGrounded && Velocity.y < 0 && inputManager.MoveInput.magnitude == 0)
        }

        public override void SlopeLimitMethod()
        {
            //base.SlopeLimitMethod();
        }
        #endregion

        #region Control binding methods
        private void CallDodge(InputAction.CallbackContext ctx)
        {
            _abilityManager.CallAbilityExecution("Dash");
        }

        private void CallJump(InputAction.CallbackContext ctx)
        {
            _abilityManager.CallAbilityExecution("Jump");
        }

        private void CancelJump(InputAction.CallbackContext ctx)
        {
            _abilityManager.CallAbilityCancellation("Jump");
        }

        private void CallSprint(InputAction.CallbackContext ctx)
        {
            _abilityManager.CallAbilityExecution("Sprint");
        }

        private void CancelSprint(InputAction.CallbackContext ctx)
        {
            _abilityManager.CallAbilityCancellation("Sprint");
        }
        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputActions.InGameActions ingameMap = _inputManager.inputActions.InGame;

            _inputManager.DisposeAllActions(ingameMap);

            ingameMap.Jump.performed -= CallJump;
            ingameMap.Jump.canceled -= CancelJump;

            ingameMap.Sprint.performed -= CallSprint;
            ingameMap.Sprint.canceled -= CancelSprint;

            ingameMap.DashRoll.performed -= CallDodge;

            _inputManager.Initialized = false;
        }

        #region Debugging
        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
#if UNITY_EDITOR
            if (Velocity != Vector3.zero && Detections.DebugMode)
                ExtensionMethods.DrawArrow(transform.position + Vector3.up, Quaternion.LookRotation(Velocity), 1, Color.white);
#endif
        }
        #endregion
    }
}