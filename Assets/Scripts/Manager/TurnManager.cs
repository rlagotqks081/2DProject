using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public bool isPlayerTurn = true;
    public int curTurn = 0;

    [SerializeField] private Button endTurnButton;


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
    private void OnEnable()
    {
        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
    }

    public void OnEndTurnButtonClicked()
    {
        // 턴 상태 체크 및 버튼 비활성화 (클릭 중복 방지)
        StartCoroutine(TurnTransitionRoutine());
    }

    private IEnumerator TurnTransitionRoutine()
    {
        // 플레이어 입력 잠금
        InputManager.Instance.UpdateCurrentState(InputState.Processing);
        curTurn++;

        yield return CardManager.Instance.DiscardAllCards();
        BuffManager.Instance.TurnEndChangeBuffs();

        yield return BattleManager.Instance.StartMonsterTurn();

        BattleManager.Instance.StartPlayerTurn();
        InputManager.Instance.UpdateCurrentState(InputState.Idle);
        endTurnButton.interactable = true;
    }

}
