using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.Abilities
{
    [CreateAssetMenu(fileName = "New Dash", menuName = "Abilities / Ability / Dash / Dash")]
    public class DashHandler : Ability
    {
        [Space(10)]
        [Header("Dash Ability")]
        public float DashSpeed = 20f;
        public float GravityMultiplier = 1.3f;

        private float _initialSpeedMultiplier;
        private float _initialGravityMultiplier;

        public override void StartAbility()
        {
            base.StartAbility();
            _initialSpeedMultiplier = controller.SpeedMultiplier;
            _initialGravityMultiplier = controller.GravityMultiplier;
            controller.GravityMultiplier = GravityMultiplier;
        }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            controller.CanRotate = false;
            controller.CanMove = false;

            ExecuteDashRoll();
        }

        public virtual void ExecuteDashRoll()
        {
            controller.AddVelocity(DashSpeed * Time.fixedDeltaTime * controller.transform.forward.normalized);
        }

        public override void EndAbility()
        {
            base.EndAbility();
            controller.CanRotate = true;
            controller.CanMove = true;
            //controller.Velocity = Vector3.zero;
            controller.SpeedMultiplier = _initialSpeedMultiplier;
            controller.GravityMultiplier = _initialGravityMultiplier;
        }

        public override bool CanExecuteAbility()
        {
            return base.CanExecuteAbility() && controller.IsGrounded;
        }

        public override bool CanCoolDownAbility()
        {
            return base.CanCoolDownAbility() && controller.IsGrounded;
        }
    }
}