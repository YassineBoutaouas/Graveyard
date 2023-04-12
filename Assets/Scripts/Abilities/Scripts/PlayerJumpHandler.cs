using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;

namespace Graveyard.Abilities
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities / Ability / Jump / PlayerJump")]
    public class PlayerJumpHandler : JumpHandler
    {
        [Header("Input window")]
        [Space(10)]
        public float JumpInputTime;
        public float GroundedTime;

        private InputManager _inputManager;

        private float _jumpPressRemTime;
        private float _groundedRemTime;

        public override void InitializeAbility(CharacterHandler c)
        {
            _inputManager = InputManager.GetInstance();
            base.InitializeAbility(c);
        }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();

            _jumpPressRemTime = 0f;
            _groundedRemTime = 0f;
        }

        public override bool CanExecuteAbility()
        {
            return ((currentExecutionAmount > 0 && !isExecuting) || (_jumpPressRemTime > 0 && _groundedRemTime > 0 && currentExecutionAmount > 0 && !isExecuting)) && controller.CanMove;
        }

        public override void UpdateAbility()
        {
            base.UpdateAbility();

            _jumpPressRemTime -= Time.deltaTime;
            _groundedRemTime -= Time.deltaTime;

            if (currentExecutionAmount == ExecutionAmount && _groundedRemTime <= 0)
                currentExecutionAmount = ExecutionAmount - 1;

            if (_inputManager.inputActions.InGame.Jump.triggered)
                _jumpPressRemTime = JumpInputTime;

            if (controller.IsGrounded)
                _groundedRemTime = GroundedTime;
        }
    }
}