using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;

namespace Graveyard.AI
{
    public class BaseState : IState
    {
        protected CharacterHandler characterController;

        public string StateName = "New State";

        public virtual void OnInitialize(CharacterHandler character)
        {
            this.characterController = character;
        }

        public virtual void OnStateEnter()
        {
            if (characterController == null) return;
        }

        public virtual void OnStateExit()
        {
            if (characterController == null) return;
        }

        public virtual void OnStateUpdate()
        {
            if (characterController == null) return;
        }
    }
}