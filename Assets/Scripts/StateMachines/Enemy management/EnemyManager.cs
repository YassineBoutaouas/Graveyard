using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Graveyard.CharacterSystem;
using Graveyard.CharacterSystem.Enemy;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public event Action<int> OnGroupCounterChange;

    public List<EnemyCharacterHandler> AllEnemies = new List<EnemyCharacterHandler>();
    public List<EnemyGroupController> EnemyGroups = new List<EnemyGroupController>();

    public int MaxAggroEnemies = 5;
    public int GlobalCoolDown = 4;

    [ReadOnly] public int CurrentActiveGroups;

    [ReadOnly] public EnemyCharacterHandler CurrentAttackingEnemy;
    public List<EnemyCharacterHandler> CurrentAggroEnemies;
    public List<EnemyCharacterHandler> CurrentSelectedEnemies = new List<EnemyCharacterHandler>();

    private float _coolDown = 1f;

    public static EnemyManager GetInstance() { return Instance; }

    private TMPro.TextMeshProUGUI _enemyCounter;
    private int _totalEnemyGroups;
    private int _finishedGroups;

    private void Awake()
    {
        Instance = this;

        FetchEnemyGroups();
        FetchEnemies();

        CurrentAggroEnemies = new List<EnemyCharacterHandler>(MaxAggroEnemies);
    }

    public void Start()
    {
        GlobalHUDManager.Instance.EnableHUDElement("RaverIndicator", true);
        _enemyCounter = GlobalHUDManager.Instance.GetHUDElement("RaverIndicator").TextElements["RaverIndicator"];
        _coolDown = GlobalCoolDown.ConvertBeatsToSeconds(AudioSpectrumManager.Instance.BeatsPerMinute);

        foreach (EnemyGroupController groupController in EnemyGroups)
            groupController.OnGroupFinished += (arg1, arg2) => OnEnemyGroupsChange();

        _enemyCounter.text = 0 + " /" + _totalEnemyGroups.ToString();

        StartCoroutine(ExecuteAttack());
    }

    private IEnumerator ExecuteAttack()
    {
        while (enabled)
        {
            yield return null;

            if (CurrentAttackingEnemy == null && CurrentAggroEnemies.Find(e => e.AttackHandler.IsAttacking) == null)
            {
                CurrentSelectedEnemies = CurrentAggroEnemies.FindAll(e => e.AttackHandler.IsCoolingDown == false);

                if (CurrentSelectedEnemies.Count > 0)
                {
                    EnemyCharacterHandler randomAggroEnemy = CurrentSelectedEnemies[UnityEngine.Random.Range(0, CurrentSelectedEnemies.Count)];
                    //Debug.Log("Selected enemy: " + randomAggroEnemy.name + ", is in attack: " + randomAggroEnemy.AttackHandler.IsAttacking + ", cool down: " + randomAggroEnemy.AttackHandler.IsCoolingDown);

                    CurrentAttackingEnemy = randomAggroEnemy;
                    CurrentAttackingEnemy.AttackHandler.IsAttacking = true;

                    yield return new WaitForSeconds(_coolDown);
                }
            }
        }

        CurrentSelectedEnemies.Clear();
    }

    #region Modify enemy lists
    public void AddEnemyToAggroList(EnemyCharacterHandler enemy)
    {
        if (CurrentAggroEnemies.Count < MaxAggroEnemies && !CurrentAggroEnemies.Contains(enemy))
        {
            CurrentAggroEnemies.Add(enemy);
            enemy.AttackHandler.CanAttack = true;
        }
    }

    public void RemoveEnemyFromAggroList(EnemyCharacterHandler enemy)
    {
        if (CurrentAggroEnemies.Contains(enemy))
        {
            CurrentAggroEnemies.Remove(enemy);
            enemy.AttackHandler.CanAttack = false;
        }
    }

    private void OnEnemyGroupsChange()
    {
        _finishedGroups = EnemyGroups.FindAll(g => g.Active == false).Count;
        CurrentActiveGroups = EnemyGroups.FindAll(g => g.Active).Count;

        OnGroupCounterChange?.Invoke(_finishedGroups);

        _enemyCounter.text = _finishedGroups.ToString() + " /" + _totalEnemyGroups.ToString();

    }
    #endregion

    #region Enemy group methods
    public void FetchEnemyGroups()
    {
        EnemyGroups.Clear();
        EnemyGroups = FindObjectsOfType<EnemyGroupController>().ToList();
        CurrentActiveGroups = EnemyGroups.Count;
        _totalEnemyGroups = CurrentActiveGroups;
    }

    public void FetchEnemies()
    {
        AllEnemies.Clear();
        AllEnemies = FindObjectsOfType<EnemyCharacterHandler>().ToList();
    }

    public EnemyGroupController GetEnemyGroup(int group)
    {
        return EnemyGroups.Find(g => g.ID == group);
    }
    #endregion
}