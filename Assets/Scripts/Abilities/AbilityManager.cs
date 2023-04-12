using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Graveyard.CharacterSystem;

namespace Graveyard.Abilities
{
    public class AbilityManager : MonoBehaviour
    {
        public List<Ability> AbilityList = new List<Ability>();

        public Dictionary<string, Ability> Abilities = new Dictionary<string, Ability>();
        public event Action<string> OnPassiveAbilityTriggered;
        public void TriggerPassiveAbility(string name) => OnPassiveAbilityTriggered?.Invoke(name);

        protected CharacterHandler characterHandler;

        public Ability GetAbilityByName(string name)
        {
            if (Abilities.ContainsKey(name))
                return Abilities[name];
            return null;
        }

        protected virtual void OnEnable()
        {
            characterHandler = GetComponent<CharacterHandler>();
        }

        protected virtual void Start()
        {
            if (AbilityList.Count > 0)
                foreach (Ability ability in AbilityList)
                    AddAbility(ability);
        }

        private void FixedUpdate()
        {
            if (Abilities.Count > 0)
                foreach (Ability ability in Abilities.Values)
                    ability.UpdateAbility();
        }

        public void CallAbilityExecution(string abilityName)
        {
            GetAbilityByName(abilityName).TryExecuteAbility();
        }

        public void CallAbilityCancellation(string abilityName)
        {
            GetAbilityByName(abilityName).CancelAbility();
        }

        public void AddAbility(Ability ability)
        {
            ability.InitializeAbility(characterHandler);
            Abilities.Add(ability.AbilityName, ability);
        }
    }
}