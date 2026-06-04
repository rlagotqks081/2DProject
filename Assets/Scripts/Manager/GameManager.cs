using Unity.VisualScripting;
using UnityEngine;

public enum GameState { Start, Monster, Shop, Random, Boss, WinBattle, LoseBattle, Map }
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }



    [Header("Current State")]
    public GameState currentState;

    [Header("References")]
    public Player player;
    public Monster currentMonster;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        player = Player.Instance;
        ChangeState(GameState.Start);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Start:
                SetupGame();

                ChangeState(GameState.Monster); // 테스트용 바로 전투돌입
                break;
            case GameState.Monster:
                Debug.Log("[GameManager] 전투 시작]");
                SetupBattle();
                player.OnStartTurn();
                break;
            case GameState.Shop:
                break;
            case GameState.Random:
                // 승리 결과 
                break;
            case GameState.Boss:
                // 패배 결과
                break;
            case GameState.WinBattle:
                Destroy(currentMonster.gameObject);
                currentMonster = SpawnManager.Instance.SpawnMonster("10001").GetComponent<Monster>();
                ChangeState(GameState.Monster);
                break;
            case GameState.LoseBattle:
                break;
            case GameState.Map:
                break;
        }
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
     public void SetupGame()
    {
        player.ResetStats();
        HandManager.Instance.SetupHand();
        CardManager.Instance.BaseCardSetup();
        BuffManager.Instance.AddBuffObj(player.gameObject);
    }

    private void SetupBattle()
    {
        Debug.Log("GameManager - SetupBattle 실행");
        BattleFlowManager.Instance.ResetSetting();
        CardManager.Instance.SetupCards();
        BuffManager.Instance.ClearTargetBuffs(player.gameObject);
        // 여기서 몬스터 랜덤소환 or 몬스터 소환

        // BuffManager.Instance.AddBuffObj(currentMonster.gameObject);
        // BuffManager.Instance.ClearTargetBuffs(currentMonster.gameObject);
        currentMonster.UpdateNextActionIcon();


    }



    //public void ProcessMonsterAction(GameObject monsterObj, MonsterAction action)
    //{
    //    foreach (SubEffect effect in action.subEffects)
    //    {
    //        switch (effect.effectType)
    //        {
    //            case "Attack":
    //                break;
    //            case "Defend":
    //                break;
    //            case "Buff_Vulnerable":
    //                break;
    //            case "Buff_Strength":
    //                break;

    //        }
    //    }

    //}
}
