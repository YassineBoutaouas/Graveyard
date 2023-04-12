using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioSpectrumManager : MonoBehaviour
{
    public static AudioSpectrumManager Instance;
    public event Action OnBeatPlay;
    public enum BeatEvaluation { Perfect, Good, Bad };

    #region Public variables
    public string CurrentAudioClipName;

    [Header("Spectrum parameters")]
    [Space(5)]
    public int BeatsPerMinute = 128;
    [Tooltip("Multiplier to denormalize the spectrum data, might vary from one audio source to another")]
    public float SpectrumMultiplier = 1000f;
    [Tooltip("Array size determines the resolution of the values that are picked up")]
    public int SpectrumResolution = 128;
    public FFTWindow fftWindow;

    [Header("Detection Parameters")]
    [Space(5)]
    [Tooltip("Beat recognition threshhold")]
    public float Bias;
    [Tooltip("Update rate of beat detection, the smaller, the more often it updates")]
    public float TimeStep;
    [Tooltip("Speed to interpolate back to 0")]
    public float RestingTime;

    [Header("Interpolation Curves")]
    //public AnimationCurve ToBeatCurve;
    [Tooltip("A value of 1 corresponds to a bad score while 0 is perfect")]
    public AnimationCurve RestingCurve;

    public BeatEvaluation CurrentBeatEvaluation = BeatEvaluation.Bad;
    [Tooltip("Threshholds for the performance/ on beat evaluation - at what threshhold will a certain grade be triggered?")]
    [NonReorderable] public float[] PerformanceThreshholds = new float[Enum.GetValues(typeof(BeatEvaluation)).Length - 1];
    #endregion

    public float NormalizedScale { get { return _normalizedAudioScale; } }
    public float CurrentBeatValue { get { return _currentBeatValue; } }
    public bool IsOnBeat { get { return isOnBeat; } }

    #region Non-public variables
    private SoundHandler _soundHandler;
    private IEnumerator _beatInterpolation;
    private AudioSource _audioSource;
    private float _spectrumValue;
    private float[] _audioSpectrum;

    private float _audioValue;
    private float _previousAudioValue;
    private float _timer;

    private float _normalizedAudioScale;
    [ReadOnly] public float _currentBeatValue;

    protected bool isOnBeat;
    #endregion

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        GameObject audioObject = new GameObject("AudioObject", typeof(AudioSource));
        audioObject.transform.SetParent(transform);
        _audioSource = audioObject.GetComponent<AudioSource>();
        _audioSource.loop = true;

        ChangeMasterTrack(CurrentAudioClipName);

        _audioSpectrum = new float[SpectrumResolution];
        _currentBeatValue = PerformanceThreshholds[(int)BeatEvaluation.Bad - 1];

        _beatInterpolation = InterpolateBeatScale();
    }

    private void Start()
    {
        StartCoroutine(WaitForAudioManager());
    }

    private IEnumerator WaitForAudioManager()
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);
        _soundHandler = GetComponent<SoundHandler>();

        _soundHandler.PlaySound("SoundTrack");
        _soundHandler.PlaySound("Crickets");
        _soundHandler.PlaySound("Crowd");
        _audioSource.volume = AudioManager.Instance.Settings.RhythmTrackVolume;

        AudioManager.Instance.OnVolumesChanged += ChangeRhythmVolume;
    }

    private void ChangeRhythmVolume()
    {
        _audioSource.volume = AudioManager.Instance.Settings.RhythmTrackVolume;
    }

    private void Update()
    {
        if (!_audioSource.isPlaying) return;

        _audioSource.pitch = Time.timeScale;
        _audioSource.GetSpectrumData(_audioSpectrum, 0, fftWindow);

        if (_audioSpectrum.Length > 0)
            _spectrumValue = _audioSpectrum[0] * SpectrumMultiplier;

        _previousAudioValue = _audioValue;
        _audioValue = _spectrumValue;

        if (_previousAudioValue <= Bias && _audioValue > Bias)
            if (_timer > TimeStep)
                OnBeat();

        _timer += Time.deltaTime;
    }

    private IEnumerator InterpolateBeatScale()
    {
        _normalizedAudioScale = 1;

        float t = 0;

        while (_normalizedAudioScale != 0)
        {
            t += Time.deltaTime;
            _normalizedAudioScale = Mathf.Lerp(1, 0, RestingCurve.Evaluate(t / RestingTime));

            if (_normalizedAudioScale >= PerformanceThreshholds[(int)BeatEvaluation.Good - 1])
            {
                CurrentBeatEvaluation = BeatEvaluation.Perfect;
                _currentBeatValue = 1;
            }

            if (_normalizedAudioScale < PerformanceThreshholds[(int)BeatEvaluation.Good - 1])
            {
                CurrentBeatEvaluation = BeatEvaluation.Good;
                _currentBeatValue = PerformanceThreshholds[(int)BeatEvaluation.Good - 1];
            }

            if (_normalizedAudioScale < PerformanceThreshholds[(int)BeatEvaluation.Bad - 1])
            {
                CurrentBeatEvaluation = BeatEvaluation.Bad;
                _currentBeatValue = PerformanceThreshholds[(int)BeatEvaluation.Bad - 1];
            }

            yield return null;
        }

        isOnBeat = false;
    }

    private void OnBeat()
    {
        _timer = 0;
        isOnBeat = true;
        OnBeatPlay?.Invoke();

        StopCoroutine(_beatInterpolation);
        _beatInterpolation = null;
        _beatInterpolation = InterpolateBeatScale();
        StartCoroutine(_beatInterpolation);
    }

    public void ChangeMasterTrack(string audioClipName)
    {
        _audioSource.clip = Resources.Load(audioClipName) as AudioClip;
        _audioSource.Play();
    }

    internal void PlayImpact()
    {
        if(_soundHandler != null)
            _soundHandler.PlaySound("BeatImpact");
    }
}