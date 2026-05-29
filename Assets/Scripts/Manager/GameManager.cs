using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public enum TurnState { Start, PlayerTurn, MonsterTurn, Won, Lost }

    [Header("Current State")]
    public TurnState currentState;

    [Header("References")]
    public Player player;
    public Monster currentMonster;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        player = Player.Instance;
        ChangeState(TurnState.Start);
    }

    public void ChangeState(TurnState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case TurnState.Start:
                SetupBattle();
                break;
            case TurnState.PlayerTurn:
                Debug.Log("Player Turn Start");
                player.OnStartTurn();
                break;
            case TurnState.MonsterTurn:
                Debug.Log("Monster Turn Start");
               // if (currentMonster != null) currentMonster.ExecuteTurn();
                ChangeState(TurnState.PlayerTurn);
                break;
            case TurnState.Won:
                // 승리 결과 
                break;
            case TurnState.Lost:
                // 패배 결과
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
    void SetupBattle()
    {
        Debug.Log("GameManager - SetupBattle 실행");
        
       player.ResetStats();
        HandManager.Instance.SetupHand();
        CardManager.Instance.TestSetup();
        BuffManager.Instance.AddBuffObj(player.gameObject);
        BuffManager.Instance.AddBuffObj(currentMonster.gameObject);
        currentMonster.UpdateNextActionIcon();
        // 여기서 몬스터 랜덤소환 or 몬스터 소환

        ChangeState(TurnState.PlayerTurn);
   
    }


    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PlayerTurn) return;

        // 버프매니저로 플레이어 턴 종료시 독이나 디버프 정산하기
        // 핸드의 카드들 무덤으로 버리기 - HandCardController 혹은 매니저 만들어서 사용
        ChangeState(TurnState.MonsterTurn);
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
