using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public enum InputState
{
    None,
    SelectingTarget
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("현재 입력 상태")]
    public InputState currentState = InputState.None;

    [Header("레이캐스트 설정")]
    [SerializeField] private LayerMask targetLayer;

    private CardUI selectedCardUI;

    private void Awake()
    {
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void Update()
    {
        if(currentState == InputState.SelectingTarget)
        {
            HandleTargetSelection();
        }
    }

    /// <summary>
    /// CardUI 스크립트에서 클릭 이벤트(IPointerClickHandler 등)를 받았을 때 
    /// 인풋 매니저에게 나를 사용해달라고 요청하는 함수
    /// </summary>
    public void TrySelectCard(CardUI card)
    {
        if (currentState != InputState.None) return;

        selectedCardUI = card;
        currentState = InputState.SelectingTarget;
        Debug.Log($"[InputManager] 카드 선택됨: {card.name}. 타겟을 선택하세요.");
    }

    /// <summary>
    /// 타겟(몬스터)을 조준하고 클릭하거나 취소하는 로직
    /// </summary>
    private void HandleTargetSelection()
    {
        if(Input.GetMouseButtonDown(1))
        {
            CancelSelection();
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, targetLayer);

            if (hit.collider != null)
            {
                Monster targetMonster = hit.collider.GetComponent<Monster>();
                if(targetMonster != null)
                {
                    Debug.Log($"[InputManager] {selectedCardUI.name} 카드를 {targetMonster.name}에게 사용합니다!");

                    //카드 사용 로직
                    selectedCardUI.OnCardUsed(targetMonster.gameObject);
                    selectedCardUI = null;
                    currentState = InputState.None;
                }
            }
        }
    }



    /// <summary>
    /// 선택했던 카드를 취소하고 다시 손패로 안전하게 되돌리는 함수 (우클릭 등)
    /// </summary>
    public void CancelSelection()
    {
        if (currentState == InputState.None) return;

        Debug.Log("[InputManager] 카드 선택이 취소되었습니다.");

        if (selectedCardUI != null)
        {
            CardUIEffect hoverEffect = selectedCardUI.GetComponent<CardUIEffect>();
            if (hoverEffect != null)
            {
                hoverEffect.ResetToOriginalState();
            }
        }

        // 변수 및 상태 초기화
        selectedCardUI = null;
        currentState = InputState.None;
    }
}
