using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.CharacterSystem.Stats
{
    [CreateAssetMenu(fileName = "New character stats", menuName = "Character stats")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Character informations")]
        [Space(10)]
        public string Name;
        [TextArea]
        public string Description;

        [Header("Physics")]
        [Space(10)]
        public float Drag;
        public float Weight = 1;

        [Header("Movement")]
        [Space(10)]
        public float MovementSpeed;
        public float TurningSpeed;
        [Range(0f, 1f)]
        public float SpeedMultiplier;

        public float GroundAcceleration;
        public float GroundDeceleration;
        [Space(5)]
        public float InAirAcceleration;
        public float InAirDeceleration;

        [Header("Health")]
        [Space(10)]
        public float MaxHealth;
        public float CurrentHealth;
        [Range(0.1f, 10f)]
        public float CurrentHealthMultiplier;
        [Range(0.1f, 10f)]
        public float CurrentMaxHealthMultiplier;

        [Header("Stun")]
        [Space(10)]
        public float StunResistance;
        public float CurrentStun;
        [Range(0.1f, 1f)]
        public float CurrentStunMultiplier;
        [Range(0.1f, 1f)]
        public float StunResistanceMultiplier;

        [Header("Combat")]
        [Space(10)]
        public float BadHitStrength;
        public float GoodHitStrength;
        public float PerfectHitStrength;
        [Range(0f, 1f)]
        public float StrengthMultiplier;
    }
}