using UnityEngine;


public enum GameOverType
{
    PlayerDead,
    AllMonsterDead
}
public class BattleFlowManager : MonoBehaviour
{
    public static BattleFlowManager Instance;

    // 죽음 이벤트 정의
    public event System.Action<GameOverType> OnGameOver;
    public bool IsGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerGameOver(GameOverType type)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        OnGameOver?.Invoke(type);

        //UIManager.Instance.ShowResultScreen();
    }
    public void ChackBattleState()
    {
        if(Player.Instance.CurrentHp <= 0)
        {
            TriggerGameOver(GameOverType.PlayerDead);
            return;
        }

        if(BattleManager.Instance.activeMonsters.TrueForAll(m => m.currentHp <= 0))
        {
            TriggerGameOver(GameOverType.AllMonsterDead);
        }
    }
}