using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDebugger : MonoBehaviour
{
    public Color[] Indications;
    public float MinSize = 2;
    public float MaxSize = 10;

    private UnityEngine.UI.Image _image;
    private AudioSpectrumManager _audioManager;

    private void Start()
    {
        _audioManager = AudioSpectrumManager.Instance;
        _image = GetComponent<UnityEngine.UI.Image>();
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(Vector3.one * MinSize, Vector3.one * MaxSize, _audioManager.NormalizedScale);
        _image.color = Indications[(int)_audioManager.CurrentBeatEvaluation];
    }
}
