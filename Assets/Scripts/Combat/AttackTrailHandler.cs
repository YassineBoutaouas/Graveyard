using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graveyard.Combat
{
    public class AttackTrailHandler : MonoBehaviour
    {
        public float MaxTime = 0.2f;

        private TrailRenderer _trailRenderer;
        private AttackHandler _attackHandler;

        private void Start()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            _trailRenderer.enabled = false;
            StartCoroutine(WaitForAttackHandler());
        }

        private IEnumerator WaitForAttackHandler()
        {
            yield return new WaitUntil(() => GameManager.Instance.PlayerController != null);
            _attackHandler = GameManager.Instance.PlayerController.CharacterAttackHandler;
            _attackHandler.Instrument.OnValidAttack += OnHit;
        }

        private void OnHit()
        {
            StopAllCoroutines();
            StartCoroutine(InterpolateLine());
        }

        private IEnumerator InterpolateLine()
        {
            float t = 0;
            _trailRenderer.time = MaxTime;
            _trailRenderer.enabled = true;

            while (t < MaxTime)
            {
                t += Time.deltaTime;

                _trailRenderer.time = Mathf.Lerp(MaxTime, 0, (t / MaxTime));
                yield return null;
            }

            _trailRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            _attackHandler.Instrument.ResetValidAttack();
        }
    }
}