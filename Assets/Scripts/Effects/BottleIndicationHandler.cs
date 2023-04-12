using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleIndicationHandler : MonoBehaviour
{
    public int BeatDuration = 4;
    public AnimationCurve IndicationCurve;

    private Renderer _indicationRenderer;
    private float _timeStep;

    private void OnEnable()
    {
        _indicationRenderer = GetComponent<Renderer>();
        _timeStep = 1.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
        StartCoroutine(PulseCircle());
    }

    private IEnumerator PulseCircle()
    {
        float t = 0;

        while (t < BeatDuration)
        {
            yield return null;
            t += Time.deltaTime;

            _indicationRenderer.sharedMaterial.SetColor("_Color", new Color(1,1,1, IndicationCurve.Evaluate(t / BeatDuration)));
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
