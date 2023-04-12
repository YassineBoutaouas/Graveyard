using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Graveyard.CharacterSystem.Enemy;
using Graveyard.CharacterSystem.Player;
using Graveyard.CharacterSystem.Detections;
using Graveyard.CharacterSystem.Stats;
using Graveyard.Health;

namespace Graveyard.Combat
{
    public class AttackHandler : MonoBehaviour
    {
        #region Events
        public event Action<float, AudioSpectrumManager.BeatEvaluation> OnBeatAttack;
        public event Action<float, AudioSpectrumManager.BeatEvaluation> OnBeatInput;
        public event Action OnBadHit;
        public event Action OnBadHitRecover;
        public event Action OnBadHitReset;

        public void ResetOnBadHit() { OnBadHit = null; }
        public void ResetOnBadHitRecover() { OnBadHitRecover = null; }
        public void ResetOnBadHitReset() { OnBadHitReset = null; }
        #endregion

        #region Public variables
        [Space(5)]
        public bool Debugging;

        [Space(5)]
        public float InputTimeOut = 0.15f;

        [Header("Obstacle detections")]
        [Space(10)]
        public float DetectionHeightOffset;
        public DetectionObject EnemyDetection;
        public InstrumentController Instrument;

        public float HitStunDuration;
        public float AnimationStunDelay;

        [Header("Auto targeting")]
        [Space(10)]
        public float MinAngle;
        public float MaxAngle;

        [Header("Manual targeting")]
        [Space(10)]
        public float MaxRange;

        [Header("Last hit values")]
        [Space(5)]
        public float SlowMotionDuration = 1;
        public float SlowMotionAmount = 0.5f;

        [Space(10)]
        public ComboHandler ComboManager;

        [HideInInspector] public HealthHandler CurrentTarget;
        [HideInInspector] public bool CanAttack;
        [HideInInspector] public AudioSpectrumManager.BeatEvaluation CurrentBeatEvaluation;
        [HideInInspector] public float CurrentBeatValue;

        public PlayerCharacterController PlayerController { get { return _playerCharacter; } }
        public bool IsAttacking { get { return _isAttacking; } set { _isAttacking = value; } }
        public bool IsFinishing { get { return _isFinishing; } }
        #endregion

        #region Non-Public variables
        private InputManager _inputManager;
        private PlayerCharacterController _playerCharacter;
        private CharacterStats _playerCharacterStats;

        private GameObject _cameraFocus;
        private Cinemachine.CinemachineVirtualCameraBase _freeLookCamera;
        private Cinemachine.CinemachineVirtualCameraBase _finisherCamera;

        private DetectionObject _enemyCrowdDetection;
        private List<HealthHandler> _detectedEnemies = new List<HealthHandler>();

        private float _accumulatedDamage;
        private float _comboMultiplier = 1f;

        private bool _isAttacking = false;
        private bool _isFinishing = false;
        private bool _cooldownInput;

        private string[] _damageParticleTags;
        private Animator _missedPromptAnimator;
        #endregion

        private void Awake()
        {
            Instrument.Initialize(this);
            Instrument.SpawnInstrument(true);
        }

        private void Start()
        {
            _damageParticleTags = Enum.GetNames(typeof(AudioSpectrumManager.BeatEvaluation));
            CanAttack = true;

            for (int i = _damageParticleTags.Length - 1; i >= 0; i--)
                _damageParticleTags[i] += "-Hit_Impact";

            #region Get Components
            _playerCharacter = GetComponent<PlayerCharacterController>();
            _inputManager = InputManager.GetInstance();
            _playerCharacterStats = _playerCharacter.Stats;
            _enemyCrowdDetection = _playerCharacter.Detections.GetDetectionObject("Enemy");
            #endregion

            #region Camera references
            _freeLookCamera = CameraManager.Instance.GetVirtualCamera("FreeLook");
            _finisherCamera = CameraManager.Instance.GetVirtualCamera("FinishCamera");
            _cameraFocus = CameraManager.Instance.GetTarget("EnemyFinishTarget");
            #endregion

            #region Combat bindings
            foreach (EnemyGroupController enemyGroup in EnemyManager.Instance.EnemyGroups)
                enemyGroup.OnGroupFinished += (ctx, e) => StartCoroutine(CloseInCamera(e));

            _inputManager.inputActions.InGame.NormalAttack.performed += CallAttackExecution;
            #endregion

            _comboMultiplier = ComboManager.StartComboMultiplier;
            ComboManager.OnInitialize();
            ComboManager.OnComboChange += (_, ctx) => IncreaseDamage(ctx);
            ComboManager.OnComboReset += _ => ResetDamage();

            _missedPromptAnimator = GlobalHUDManager.Instance.GetHUDElement("FX_Missed").GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            CurrentTarget = _enemyCrowdDetection.detectedCollisions.Length < 1 ? null : CurrentTarget;
        }

        private void IncreaseDamage(int meter)
        {
            for (int i = ComboManager.ComboMultipliers.Count - 1; i >= 0; i--)
            {
                if (meter > ComboManager.ComboMultipliers[i].Threshhold)
                {
                    _comboMultiplier = ComboManager.ComboMultipliers[i].Multiplier;

                    RumbleManager.Instance.PulseRumble(0.2f, 0.2f, 0.3f);
                    
                    ComboManager.ComboCounter.color = ComboManager.ComboMultipliers[i].ComboColor;
                    ComboManager.ComboCounter.gameObject.transform.localScale = Vector3.one * ComboManager.ComboMultipliers[i].Scale;

                    break;
                }
            }
        }

        private void ResetDamage()
        {
            _comboMultiplier = ComboManager.StartComboMultiplier;
            ComboManager.ComboCounter.color = ComboManager.DefaultColor;
            ComboManager.ComboCounter.gameObject.transform.localScale = Vector3.one * ComboManager.StartScale;
        }

        #region Attacking methods
        private void CallAttackExecution(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            if (CanAttack)
            {
                if (!_cooldownInput)
                {
                    StartCoroutine(CoolDownInput());

                    AudioSpectrumManager.Instance.PlayImpact();
                    SyncBeatEvaluation();
                    Instrument.ExecuteAttack();
                    OnBeatInput?.Invoke(CurrentBeatValue, CurrentBeatEvaluation);
                }
            }
        }

        private IEnumerator CoolDownInput()
        {
            _cooldownInput = true;
            yield return new WaitForSeconds(InputTimeOut);
            _cooldownInput = false;
        }

        private IEnumerator CloseInCamera(EnemyCharacterHandler enemy)
        {
            RumbleManager.Instance.PulseRumble(1f, 1f, 2f);

            TimeScalerManager.Instance.TimeScalePulse(SlowMotionAmount, SlowMotionDuration);

            _playerCharacter.Velocity = Vector3.zero;
            _playerCharacter.CanMove = false;
            _playerCharacter.CanRotate = false;
            _cameraFocus.transform.position = enemy.transform.position + Vector3.up * 2;
            _isFinishing = true;

            CameraManager.Instance.ChangeCameras(_freeLookCamera, _finisherCamera);
            yield return new WaitForSeconds(SlowMotionDuration);
            _playerCharacter.CanMove = true;
            _playerCharacter.CanRotate = true;
            _isFinishing = false;
            CurrentTarget = null;

            CameraManager.Instance.ChangeCameras(_finisherCamera, _freeLookCamera);
        }
        #endregion

        public void EnableCharacterMovement(bool enabled)
        {
            _isAttacking = !enabled;
            _playerCharacter.Velocity = enabled == false ? Vector3.zero : _playerCharacter.Velocity;
        }

        //called through animation event
        public void DealDamage()
        {
            if (CurrentTarget == null) return;

            EnemyCharacterHandler enemy = CurrentTarget.GetComponent<EnemyCharacterHandler>();
            if (!CurrentTarget.IsDamagable) return;

            CameraShakerManager.Instance.PulseShake(10f * CurrentBeatValue, 1f, 0.2f);

            if (enemy.AttackHandler.Invincible)
            {
                MissHit();
                return;
            }

            if (enemy._blockingState.CanAttackHit())
            {
                enemy._blockingState.AddHits(CurrentBeatEvaluation);
                RumbleManager.Instance.PulseRumble(0.4f, 0.4f, 0.1f);

                return;
            }

            CalculateDamage();

            CurrentTarget.TakeDamage(_accumulatedDamage, CurrentBeatValue, gameObject);

            ObjectPoolerManager.Instance.SpawnFromPool(_damageParticleTags[(int)CurrentBeatEvaluation], CurrentTarget.transform.position + Vector3.up, Quaternion.identity);
            ObjectPoolerManager.Instance.SpawnFromPool("Note-Hit_Impact", CurrentTarget.transform.position + Vector3.up * 1.5f, Quaternion.LookRotation(CurrentTarget.transform.up));

            SyncBeatEvaluation();
        }

        public void MissHit()
        {
            _missedPromptAnimator.gameObject.SetActive(true);
            _missedPromptAnimator.Play("MissedPrompt");
        }

        public void SyncBeatEvaluation()
        {
            CurrentBeatEvaluation = AudioSpectrumManager.Instance.CurrentBeatEvaluation;
            CurrentBeatValue = AudioSpectrumManager.Instance.CurrentBeatValue;
        }

        #region Target methods
        public float CalculateDamage()
        {
            switch (CurrentBeatEvaluation)
            {
                case AudioSpectrumManager.BeatEvaluation.Perfect:
                    _accumulatedDamage = _playerCharacterStats.PerfectHitStrength * _playerCharacterStats.StrengthMultiplier * _comboMultiplier;
                    ComboManager.AddToComboMeter(2);
                    ComboManager.AddPerfectHit();

                    RumbleManager.Instance.PulseRumble(0.5f, 0.5f, 0.2f);

                    break;
                case AudioSpectrumManager.BeatEvaluation.Good:
                    _accumulatedDamage = _playerCharacterStats.GoodHitStrength * _playerCharacterStats.StrengthMultiplier * _comboMultiplier;
                    ComboManager.AddToComboMeter(1);
                    ComboManager.AddGoodHit();

                    RumbleManager.Instance.PulseRumble(0.3f, 0.3f, 0.15f);

                    break;
                case AudioSpectrumManager.BeatEvaluation.Bad:
                    OnBadHitExecuted();

                    RumbleManager.Instance.PulseRumble(0.8f, 0.8f, 0.3f);

                    break;
                default:
                    break;
            }

            OnBeatAttack?.Invoke(CurrentBeatValue, CurrentBeatEvaluation);
            ComboManager.AddTotalHit();

            return _accumulatedDamage;
        }

        public void OnBadHitExecuted()
        {
            _accumulatedDamage = _playerCharacterStats.BadHitStrength * _playerCharacterStats.StrengthMultiplier * _comboMultiplier;
            ComboManager.ResetCombo();
            ComboManager.AddBadHit();
            MissHit();

            StartCoroutine(ExecuteBadHit());
        }

        private IEnumerator ExecuteBadHit()
        {
            OnBadHit?.Invoke();
            PlayerController.CanMove = false;
            PlayerController.CanRotate = false;
            CanAttack = false;
            ObjectPoolerManager.Instance.SpawnFromPool("FX_Shockwave", transform.position + Vector3.up, Quaternion.identity);
            yield return new WaitForSeconds(HitStunDuration);
            OnBadHitRecover?.Invoke();
            yield return new WaitForSeconds(AnimationStunDelay);
            OnBadHitReset?.Invoke();
            PlayerController.CanMove = true;
            PlayerController.CanRotate = true;
            CanAttack = true;
        }

        public bool GetValidTarget()
        {
            Vector3 direction = _inputManager.MoveInput.magnitude > 0 ? _playerCharacter.CalculateRelativeMovement().normalized : _playerCharacter.transform.forward;

            HealthHandler enemyTarget = null;

            _detectedEnemies.Clear();
            foreach (Collider collider in Physics.OverlapSphere(transform.position, EnemyDetection.radius, EnemyDetection.layerMask, EnemyDetection.TriggerInteraction))
                _detectedEnemies.Add(collider.GetComponent<HealthHandler>());

            //Auto targeting
            if (GameManager.Instance.Settings.AutoTargeting)
            {
                float minAngle = Mathf.Infinity;
                foreach (HealthHandler healthHandler in _detectedEnemies)
                {
                    float currentAngle = Vector3.Angle(Vector3.Scale(healthHandler.transform.position - transform.position, Vector3.right + Vector3.forward).normalized, direction);
                    bool detectedObstacle = Physics.Raycast(transform.position + Vector3.up * DetectionHeightOffset, (healthHandler.transform.position + Vector3.up * DetectionHeightOffset - transform.position + Vector3.up * DetectionHeightOffset).normalized, out RaycastHit hitInfo, EnemyDetection.radius, ~EnemyDetection.layerMask, EnemyDetection.TriggerInteraction);

                    if (currentAngle > MinAngle && currentAngle < MaxAngle)
                        if (currentAngle < minAngle && !detectedObstacle)
                        {
                            minAngle = currentAngle;
                            enemyTarget = healthHandler;
                        }
                }
            }
            else
            {
                //Manual targeting
                if (Physics.SphereCast(transform.position - transform.forward * MaxRange, MaxRange, transform.forward, out RaycastHit hitInfo, MaxRange * 2, EnemyDetection.layerMask, EnemyDetection.TriggerInteraction))
                    if (hitInfo.collider.TryGetComponent(out HealthHandler h))
                        enemyTarget = h;
            }

            if (enemyTarget == null) return false;
            else if (!enemyTarget.TryGetComponent(out CurrentTarget)) return false;

            return CurrentTarget.IsDamagable;
        }
        #endregion

        private void OnDestroy()
        {
            _inputManager.inputActions.InGame.NormalAttack.performed -= CallAttackExecution;
        }

        #region Debugging
        private void OnDrawGizmos()
        {
            if (!Debugging) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position - transform.forward * MaxRange, MaxRange);
            Gizmos.DrawWireSphere(transform.position + transform.forward * MaxRange, MaxRange);

            if (CurrentTarget == null) return;

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(transform.position, 0.3f);

            Debug.DrawLine(transform.position, CurrentTarget.transform.position, Color.black);

            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(CurrentTarget.transform.position, 0.3f);
        }
        #endregion
    }

    [Serializable]
    public class ComboHandler
    {
        public event Action<int, int> OnComboChange;
        public event Action<int> OnComboReset;

        [Serializable]
        public class ComboMultiplier { public int Threshhold; public float Multiplier = 1; public Color ComboColor; public float Scale; }
        [Serializable]
        public class Grade { public string GradeName; public int GradingThreshhold; }

        public TMPro.TextMeshProUGUI ComboCounter { get { return _comboCounter; } }

        public float TimeOut = 2f;
        [ReadOnly] public int CurrentCombo;
        [HideInInspector] public int PreviousCombo;

        #region Combo multiplication
        [Header("Combo multiplication")]
        [Space(5)]
        public Color DefaultColor;
        public float StartComboMultiplier = 0.5f;
        public float StartScale = 0.8f;
        public List<ComboMultiplier> ComboMultipliers = new List<ComboMultiplier>();
        #endregion

        public List<Grade> Grades = new List<Grade>();

        private IEnumerator _comboTimeOut;

        #region UI references
        private TMPro.TextMeshProUGUI _comboCounter;
        private GlobalHUDManager _hudManager;
        private WinningScreen _winningScreenHUD;
        #endregion

        #region Grading values
        private float _timePassed;

        private int _totalHits;

        private int _totalBadHits;
        private int _totalGoodHits;
        private int _totalPerfectHits;
        private float _hitAverage;

        private int _highestCombo;

        private float _totalGrade;
        #endregion

        public void OnInitialize()
        {
            _hudManager = GlobalHUDManager.Instance;
            _hudManager.EnableHUDElement("ComboCounter", true);
            _comboCounter = _hudManager.GetHUDElement("ComboCounter").TextElements["ComboCounter"];
            _winningScreenHUD = (WinningScreen)_hudManager.GetHUDElement("WinningScreen");

            _comboTimeOut = TimeOutCombo();

            _comboCounter.gameObject.transform.localScale = Vector3.one * StartScale;

            EnemyManager.Instance.OnGroupCounterChange += (_) => DisplayWinningScreen();
        }

        #region Combo counter methods
        public void AddToComboMeter(int addition)
        {
            GameManager.Instance.StopCoroutine(_comboTimeOut);
            _comboTimeOut = null;
            _comboTimeOut = TimeOutCombo();

            PreviousCombo = CurrentCombo;
            CurrentCombo += addition;

            OnComboChange?.Invoke(PreviousCombo, CurrentCombo);
            DisplayCombo();
            SetHighestCombo();

            GameManager.Instance.StartCoroutine(_comboTimeOut);
        }

        public void ResetCombo()
        {
            OnComboReset?.Invoke(PreviousCombo);
            CurrentCombo = 0;
            PreviousCombo = 0;
            DisplayCombo();
        }

        private IEnumerator TimeOutCombo()
        {
            yield return new WaitForSeconds(TimeOut);
            ResetCombo();
        }

        private void DisplayCombo()
        {
            _comboCounter.text = "x " + CurrentCombo.ToString();
        }
        #endregion

        #region Grading methods
        public void AddTotalHit() { _totalHits++; }
        public void AddBadHit() { _totalBadHits++; }
        public void AddGoodHit() { _totalGoodHits++; }
        public void AddPerfectHit() { _totalPerfectHits++; }
        public void SetHighestCombo() { _highestCombo = Mathf.Max(_highestCombo, CurrentCombo); }

        private void DisplayWinningScreen()
        {
            if (EnemyManager.Instance.CurrentActiveGroups != 0) return;

            GameManager.Instance.LevelTimer.StopTimer();
            GameManager.Instance.StartCoroutine(DisplayAllStats());
        }

        public IEnumerator DisplayAllStats()
        {
            yield return new WaitForSeconds(_winningScreenHUD.DelayTime);
            GlobalHUDManager.Instance.ChangeHUDState(GlobalHUDManager.HUDStates.WinningScreen);
            _hudManager.EnableHUDElement("WinningScreen", true);

            _timePassed = GameManager.Instance.LevelTimer.TimePast();

            float relativeTime = 1 - (_timePassed / GameManager.Instance.LevelTimer.StartingTime);

            DisplayStat("TimePast", Mathf.Floor(_timePassed / 60).ToString("00") + " : " + Mathf.FloorToInt(_timePassed % 60).ToString("00"));

            //Hit average
            _hitAverage = ((_totalPerfectHits) + (_totalGoodHits * AudioSpectrumManager.Instance.PerformanceThreshholds[(int)AudioSpectrumManager.BeatEvaluation.Good - 1]) + (_totalBadHits * 0.1f)) / _totalHits;
            string hitAverageEvaluation = AudioSpectrumManager.BeatEvaluation.Perfect.ToString();
            for (int i = 0; i < AudioSpectrumManager.Instance.PerformanceThreshholds.Length; i++)
            {
                if (_hitAverage <= AudioSpectrumManager.Instance.PerformanceThreshholds[i])
                    hitAverageEvaluation = Enum.GetValues(typeof(AudioSpectrumManager.BeatEvaluation)).GetValue(i + 1).ToString();
            }

            DisplayStat("HitAverage", hitAverageEvaluation);

            DisplayStat("TotalPerfectHits", _totalPerfectHits.ToString());
            DisplayStat("TotalGoodHits", _totalGoodHits.ToString());
            DisplayStat("TotalBadHits", _totalBadHits.ToString());

            DisplayStat("HighestCombo", _highestCombo.ToString());


            string total = AudioSpectrumManager.BeatEvaluation.Perfect.ToString();
            _totalGrade = (_hitAverage + relativeTime) / 2;

            for (int i = 0; i < AudioSpectrumManager.Instance.PerformanceThreshholds.Length; i++)
            {
                if (_totalGrade <= AudioSpectrumManager.Instance.PerformanceThreshholds[i])
                    total = Enum.GetValues(typeof(AudioSpectrumManager.BeatEvaluation)).GetValue(i + 1).ToString();
            }

            DisplayStat("TotalGrade", total);
        }

        public void DisplayStat(string textElement, string stat)
        {
            _winningScreenHUD.TextElements[textElement].text += " " + stat;
        }
        #endregion
    }
}