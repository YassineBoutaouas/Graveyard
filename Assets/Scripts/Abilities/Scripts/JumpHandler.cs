using UnityEngine;

namespace Graveyard.Abilities
{
    [CreateAssetMenu(fileName = "New Jump", menuName = "Abilities / Ability / Jump / Jump")]
    public class JumpHandler : Ability
    {
        [Space(10)]
        [Header("Jump Ability")]
        public float JumpForce;
        public float LowJumpMultiplier;

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            controller.Velocity = new Vector3(controller.Velocity.x, JumpForce, controller.Velocity.z);
        }

        public override bool CanExecuteAbility()
        {
            return base.CanExecuteAbility() && (controller.IsGrounded || currentExecutionAmount < ExecutionAmount) && controller.CanMove == true;
        }

        public override bool CanCoolDownAbility()
        {
            return base.CanCoolDownAbility() && controller.IsGrounded;
        }

        public override void CancelAbility()
        {
            base.CancelAbility();
            if (controller.Velocity.y > 0)
                controller.Velocity += controller.AttachedRigidbody.mass * controller.GravityMultiplier * LowJumpMultiplier * Physics.gravity.y * Time.fixedDeltaTime * Vector3.up;
        }
    }
}