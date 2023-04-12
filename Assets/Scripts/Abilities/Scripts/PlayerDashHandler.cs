using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem.Player;
using Graveyard.CharacterSystem;
using System;

namespace Graveyard.Abilities
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities / Ability / Dash / PlayerDash")]
    public class PlayerDashHandler : Ability
    {
        public int BeatDuration = 4;

        private PlayerCharacterController _playerController;
        private GameObject _bachHead;
        public Renderer[] characterRenderers;

        public override void InitializeAbility(CharacterHandler character)
        {
            base.InitializeAbility(character);
            _playerController = (PlayerCharacterController)character;

            _bachHead = Instantiate(Resources.Load("Johnny_Head") as GameObject);
            _bachHead.transform.parent = ObjectPoolerManager.Instance.transform.Find("Particles");
            _bachHead.SetActive(false);

            ExecutionTime = BeatDuration.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute) - DelayTime;
            characterRenderers = controller.transform.GetComponentsInChildren<Renderer>();
        }

        public override void StartDelay()
        {
            base.StartDelay();
            controller.CanMove = false;
            controller.CanRotate = false;
            controller.Velocity = Vector3.zero;

            _playerController.IsDamagable = false;
            _playerController.CharacterAttackHandler.CanAttack = false;
        }

        public override void StartAbility()
        {
            base.StartAbility();
            ObjectPoolerManager.Instance.SpawnFromPool("Ground_Dodge", controller.transform.position, Quaternion.identity);

            foreach (Renderer renderer in characterRenderers)
                renderer.enabled = false;

            _bachHead.transform.SetPositionAndRotation(controller.transform.position, controller.transform.rotation);
            _bachHead.SetActive(true);
            _playerController.CharacterSoundHandler.PlaySound("DirtIn");

            RumbleManager.Instance.PulseRumble(0.5f, 0.5f, 0.2f);
        }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            controller.CanMove = false;
            controller.CanRotate = false;
            controller.Velocity = Vector3.zero;
        }

        public override void EndAbility()
        {
            base.EndAbility();
            controller.CanMove = true;
            controller.CanRotate = true;
            _playerController.CharacterAttackHandler.CanAttack = true;
            controller.Velocity = Vector3.zero;

            ObjectPoolerManager.Instance.SpawnFromPool("Ground_Dodge_Out", controller.transform.position, Quaternion.identity);
            RumbleManager.Instance.PulseRumble(0.5f, 0.5f, 0.2f);


            foreach (Renderer renderer in characterRenderers)
                renderer.enabled = true;

            _bachHead.SetActive(false);

            _playerController.IsDamagable = true;
            _playerController.CharacterSoundHandler.PlaySound("DirtOut");
        }

        public override bool CanExecuteAbility()
        {
            return base.CanExecuteAbility() && !isDelaying;
        }
    }
}