using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Graveyard.Combat
{
    [CreateAssetMenu(menuName = "Instruments", fileName = "New instrument")]
    public class InstrumentController : ScriptableObject
    {
        public event Action<int> OnAttackStart;
        public event Action OnValidAttack;
        public event Action OnAttackEnd;

        public void ResetValidAttack() { OnValidAttack = null; }
        public void ResetOnAttack() { OnAttackStart = null; }
        public void ResetOnAttackEnd() { OnAttackEnd = null; }

        private GameObject _instrumentObject;

        [Header("Attack Animations")]
        [Space(5)]
        [NonReorderable] public string[] AttackAnimations;
        [Space(5)]
        public bool Enabled = true;

        public void OnStartAttack(int index) { OnAttackStart?.Invoke(index); }
        public void OnEndAttack() { OnAttackEnd?.Invoke(); }
        public void OnValidAttackStart() { OnValidAttack?.Invoke(); }

        public string Name = "New instrument";

        public float Cooldown;
        public AnimationCurve InterpolationCurve;

        public GameObject InstrumentPrefab;
        [Header("Transform")]
        [Space(5)]
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;
        public Vector3 LocalScale;

        protected AttackHandler attackHandler;

        public virtual void Initialize(AttackHandler attackHandler)
        {
            this.attackHandler = attackHandler;
        }

        public GameObject SpawnInstrument(bool active = true)
        {
            _instrumentObject = Instantiate(InstrumentPrefab, attackHandler.gameObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand));

            _instrumentObject.transform.localPosition = LocalPosition;
            _instrumentObject.transform.localRotation = Quaternion.Euler(LocalRotation);
            _instrumentObject.transform.localScale = LocalScale;

            _instrumentObject.SetActive(active);
            return _instrumentObject;
        }

        public void EnableInstrument(bool enabled) { _instrumentObject.SetActive(enabled); }

        public virtual IEnumerator OnAttack() { yield return null; }

        public virtual void ExecuteAttack()
        {
            attackHandler.StartCoroutine(OnAttack());
        }

        public virtual bool CanAttack() { return Enabled && attackHandler.CanAttack; }
    }
}