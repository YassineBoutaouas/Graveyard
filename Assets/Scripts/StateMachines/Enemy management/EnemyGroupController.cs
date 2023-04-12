using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem.Player;
using UnityEngine.Rendering;
using System.Linq;

public class EnemyGroupController : MonoBehaviour
{
    #region Events
    public event Action<EnemyGroupController> OnEngaged;
    public event Action<EnemyGroupController> OnDisEngaged;
    public event Action<EnemyGroupController, EnemyCharacterHandler> OnGroupFinished;
    #endregion

    #region Public variables
    public bool Debugging;

    public int ID = -1;
    public float Radius = 1;
    public int EnemyCount = 2;
    public float AggroTime = 5f;
    public float ChoirDistance = 4f;
    [ReadOnly] public bool Active = true;
    public int FadeOutDurationInBeats = 8;
    public int FadeInDurationInBeats = 2;
    #endregion

    #region Properties
    public List<EnemyCharacterHandler> Enemies { get { return _enemies; } }
    public List<EnemyCharacterHandler> ChasingEnemies { get { return _currentChasingEnemies; } }
    public bool EngagedInFight { get { return _engagedInFight; } set { _engagedInFight = value; } }
    public int CurrentActiveEnemies { get { return _currentActiveEnemies; } set { _currentActiveEnemies = value; } }
    public List<Vector3> ReturnPositions { get { return _randomPositions; } }
    public List<Vector3> ChoirPositions { get { return _choirPositions; } }
    #endregion

    #region Non_Public variables
    private List<EnemyCharacterHandler> _enemies = new List<EnemyCharacterHandler>();
    private List<EnemyCharacterHandler> _currentChasingEnemies = new List<EnemyCharacterHandler>();

    private Volume _postProcessVolume;
    private SoundHandler _groupSoundHandler;
    private SphereCollider _sphereCollider;
    private List<Vector3> _startingPositions = new List<Vector3>();
    private List<Vector3> _randomPositions = new List<Vector3>();
    private List<Vector3> _choirPositions = new List<Vector3>();

    private int _currentActiveEnemies;
    private float _fadeOutDuration;
    private float _fadeInDuration;

    private bool _engagedInFight;

    private float _initialChoirVolume;

    private Sound _choirSound;

    private IEnumerator _aggroRoutine;
    private IEnumerator _postProcessFade;
    #endregion

    private void Awake()
    {
        _sphereCollider = gameObject.AddComponent<SphereCollider>();
        _groupSoundHandler = GetComponent<SoundHandler>();
        _postProcessVolume = GetComponent<Volume>();

        _sphereCollider.isTrigger = true;
        _sphereCollider.radius = Radius;
    }

    private void Start()
    {
        _fadeOutDuration = FadeOutDurationInBeats.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);
        _fadeInDuration = FadeInDurationInBeats.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);

        _enemies.Clear();
        _startingPositions.Clear();
        foreach (EnemyCharacterHandler enemy in EnemyManager.Instance.AllEnemies)
            if (enemy.GroupID == ID)
            {
                _enemies.Add(enemy);
                _startingPositions.Add(enemy.transform.position);
            }

        _currentActiveEnemies = _enemies.Count;

        _aggroRoutine = ResetAggro();

        _postProcessFade = FadeVolume(true);
        StartCoroutine(WaitForAudioManager());
    }

    private void Update() { if(_choirSound != null) _choirSound.Source.pitch = Time.timeScale; }

    private IEnumerator WaitForAudioManager()
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);
        _groupSoundHandler.GetSound("Choir", out _choirSound);
        _initialChoirVolume = _choirSound.Volume;
        _choirSound.Volume = 0f;
        _groupSoundHandler.ChangeVolume(_choirSound);
        _groupSoundHandler.PlaySound("Choir");
    }

    #region Chasing methods
    public void AddChaser(EnemyCharacterHandler character)
    {
        if (!_currentChasingEnemies.Contains(character))
            _currentChasingEnemies.Add(character);
    }

    public void RemoveChaser(EnemyCharacterHandler character)
    {
        if (!_currentChasingEnemies.Contains(character))
            _currentChasingEnemies.Remove(character);
    }

    public void SetChoirActive(EnemyCharacterHandler lastEnemy)
    {
        _engagedInFight = false;

        for (int i = 0; i < _enemies.Count; i++)
            _choirPositions.Add(transform.position + ChoirDistance * i * transform.right);

        GameManager.Instance.StartCoroutine(PlayChoirSound());
        _groupSoundHandler.PlaySound("LastHit");

        Active = false;
        OnGroupFinished?.Invoke(this, lastEnemy);

        GlobalHUDManager.Instance.EnableHUDElement("FX_GroupFinished", true);
        GlobalHUDManager.Instance.GetHUDElement("FX_GroupFinished").GetComponent<Animator>().Play("GroupFinishedPrompt");

        transform.Find("FX_Choir").gameObject.SetActive(true);
    }

    private IEnumerator PlayChoirSound()
    {
        bool allTrue = (_enemies.All(e => e._convincedState.ReachedTarget == true));

        while (allTrue == false)
        {
            yield return null;
            allTrue = (_enemies.All(e => e._convincedState.ReachedTarget == true));
        }

        _choirSound.Volume = _initialChoirVolume;
        _groupSoundHandler.ChangeVolume(_choirSound);
    }
    #endregion

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerCharacterController _)) return;

        if (_engagedInFight == true)
        {
            StopCoroutine(_aggroRoutine);
            _aggroRoutine = null;

            _aggroRoutine = ResetAggro();
            StartCoroutine(_aggroRoutine);
        }

        ///if (!_engagedInFight)
        ///{
        ///    StopCoroutine(_postProcessFade);
        ///    _postProcessFade = null;
        ///    _postProcessFade = FadeVolume(false);
        ///    StartCoroutine(_postProcessFade);
        ///}
    }

    public void StartAggroTimer()
    {
        StopCoroutine(_aggroRoutine);
        _aggroRoutine = null;

        _engagedInFight = true;
        OnEngaged?.Invoke(this);

        _aggroRoutine = ResetAggro();
        StartCoroutine(_aggroRoutine);
    }

    #region Aggro and fade routines
    private IEnumerator ResetAggro()
    {
        yield return new WaitForSeconds(AggroTime);

        if (Vector3.Distance(GameManager.Instance.PlayerController.transform.position, transform.position) > Radius)
        {
            _randomPositions = _startingPositions.ShuffleList();

            yield return null;
            _engagedInFight = false;
            OnDisEngaged?.Invoke(this);
        }
    }

    private IEnumerator FadeVolume(bool active)
    {
        float target = active ? _fadeInDuration : _fadeOutDuration;
        float start = active ? 0 : 1;
        float end = 1 - start;

        float t = 0;
        while (t < target)
        {
            yield return null;
            t += Time.deltaTime;
            _postProcessVolume.weight = Mathf.Lerp(start, end, t / target);
        }

        _postProcessVolume.weight = end;
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!Debugging) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}