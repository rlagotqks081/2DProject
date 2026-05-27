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
        endTurnButton.interactable = false; 

        yield return CardManager.Instance.DiscardAllCards();

        // 3. 적 턴으로 전환 (필요 시)
        // yield return StartCoroutine(EnemyTurnRoutine());

        InputManager.Instance.UpdateCurrentState(InputState.Idle);
        endTurnButton.interactable = true;
    }

}
