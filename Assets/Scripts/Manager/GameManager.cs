using Unity.VisualScripting;
using UnityEngine;

public enum GameState { Main, Start, Monster, Shop, Random, Boss, WinBattle, LoseBattle, Map }
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    public bool isFirstMapSelect = true;
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
            SpriteDatabase.LoadAllBuffs();
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
                UIManager.Instance.MainMenuUI.SetActive(true);
                break;
            case GameState.Monster:
                Debug.Log("[GameManager] 전투 시작]");
                SetupBattle();
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
                ChangeState(GameState.Map);
                break;
            case GameState.LoseBattle:
                break;
            case GameState.Map:
                UIManager.Instance.OnClickMapOpenButton();
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
        if(currentMonster != null)
        {
            Destroy(currentMonster.gameObject);
        }
        Debug.Log("GameManager - SetupBattle 실행");
        BattleFlowManager.Instance.ResetSetting();
        player.OnStartTurn();
        CardManager.Instance.SetupCards();
        BuffManager.Instance.ClearTargetBuffs(player.gameObject);
        // 여기서 몬스터 랜덤소환 or 몬스터 소환
        currentMonster = SpawnManager.Instance.SpawnRandomMonster().GetComponent<Monster>();

        currentMonster.UpdateNextActionIcon();
        InputManager.Instance.UpdateCurrentState(InputState.Idle, true);
        UIManager.Instance.ResetAllUI();
    }




}
