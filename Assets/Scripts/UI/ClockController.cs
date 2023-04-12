using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ClockController : MonoBehaviour
{
    [ReadOnly] public int NightPhase = 1;

    [Header("Damage clock animation values")]
    public float FadeDuration = 0.2f;
    public AnimationCurve FadingCurve;
    public float DamageAnimationDuration;

    private Animator _animator;
    private float _phaseDuration;
    private bool _started;
    private HUDElementController _clockDamage;
    private Volume _hitVolume;
    private TMPro.TextMeshProUGUI _clockText;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        GameManager.Instance.LevelTimer.OnStart += () => { _phaseDuration = GameManager.Instance.LevelTimer.StartingTime / 4; _started = true; };

        _clockDamage = GlobalHUDManager.Instance.GetHUDElement("ClockDamage");
        _hitVolume = GlobalHUDManager.Instance.GetHUDElement("HitVolume").GetComponent<Volume>();
        _clockText = _clockDamage.TextElements["DamageText"];
        Animator textAnimator = _clockText.GetComponent<Animator>();

        GameManager.Instance.LevelTimer.OnClockDamage += (ctx) =>
        { 
            _clockText.text = "- " + ctx.ToString();
            _clockText.gameObject.SetActive(true);
            textAnimator.Play("DamagedClock");
            StartCoroutine(FadeClock());
            StartCoroutine(DeactivateText());
        };

    }

    private IEnumerator FadeClock()
    {
        float t = 0;

        while (t < FadeDuration)
        {
            yield return null;
            t += Time.deltaTime;

            _hitVolume.weight = Mathf.Lerp(0, 1, FadingCurve.Evaluate(t / FadeDuration));
        }

        float e = 0;
        _hitVolume.weight = 1;

        while (e < FadeDuration)
        {
            yield return null;
            e += Time.deltaTime;

            _hitVolume.weight = Mathf.Lerp(1, 0, FadingCurve.Evaluate(e / FadeDuration));
        }

        _hitVolume.weight = 0f;
    }

    private IEnumerator DeactivateText()
    {
        yield return new WaitForSeconds(DamageAnimationDuration);
        _clockText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_started) {return; }
 

        if (GameManager.Instance.LevelTimer.CurrentTime <= _phaseDuration && NightPhase == 3)
        {
            NightPhase = 4;
            RumbleManager.Instance.PulseRumble(1f, 1f, 1f);
        }

        if (GameManager.Instance.LevelTimer.CurrentTime <= _phaseDuration * 2 && NightPhase == 2)
        {
            NightPhase = 3;
            RumbleManager.Instance.PulseRumble(1f, 1f, 1f);
        }

        if (GameManager.Instance.LevelTimer.CurrentTime <= _phaseDuration * 3 && NightPhase == 1)
        {
            NightPhase = 2;
            RumbleManager.Instance.PulseRumble(1f, 1f, 1f);
        }

        _animator.SetInteger("NightPhase", NightPhase);
    }
}
