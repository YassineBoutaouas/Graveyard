using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;

namespace Graveyard.AI
{
    public interface IState
    {
        public void OnInitialize(CharacterHandler characterHandler);
        public void OnStateEnter();
        public void OnStateUpdate();
        public void OnStateExit();
    }
}