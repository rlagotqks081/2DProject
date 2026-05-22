using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public enum InputState
{
    None,
    SelectingTarget,
    SelectingCard,
    SelectedSkillCard
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("현재 입력 상태")]
    public InputState currentState = InputState.None;

    [Header("레이캐스트 설정")]
    [SerializeField] private LayerMask targetLayer;

    [SerializeField] private CardUI selectedCardUI;  // 손의 있는카드를 선택했을때 여기에 저장
    [SerializeField] private Monster hoverTarget;  // 타겟한 몬스터
    [SerializeField] private Camera mainCamera;

    private int maxSelectCount; // 외부에서 전달받은 제한 횟수
    private List<RuntimeCard> selectedCards = new List<RuntimeCard>();  //카드선택화면 에서 선택된 카드들
    private List<CardUI> selectedCardsUI = new List<CardUI>();  // 선택된 카드들의 CardUI
    private System.Action<List<RuntimeCard>> onSelectionConfirmed;  // 카드선택완료시 발동

    [Header("선택된 카드 UI 정렬 설정")]
    [SerializeField] private float cardSpacing = 220f;

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
        switch(currentState)
        {
            case InputState.None:
                break;
            case InputState.SelectingTarget:
                UpdateHoverTarget();
                HandleTargetSelection();
                break;
            case InputState.SelectingCard:
                break;
            case InputState.SelectedSkillCard:
                HandleMousePosition();
                break;
        }
    }
    public void StartSelectingMultipleCards(int maxSelectCount, System.Action<List<RuntimeCard>> onConfirmed)
    {
        this.maxSelectCount = maxSelectCount;
        currentState = InputState.SelectingCard;
        UIManager.Instance.ShowCardSelectUI(true);

        selectedCards.Clear();
        selectedCardsUI.Clear();
        onSelectionConfirmed = onConfirmed;


    }

    /// <summary>
    /// 콜백함수안에서 직접수정하는게 아니면 무조건 이걸통해서 CurrentState를 바꿔야함!!!!!
    /// </summary>
    /// <param name="state"></param>
    public void UpdateCurrentState(InputState state)
    {
        if(currentState == InputState.SelectingCard)
        {
            return;
        }
        currentState = state;
    }

    //SelectingCard 상태 관련 함수는 카드의 UI관련 움직임조작 나중에 CardEffect로 빼야함 - 이것도 UIManager같은곳으로 빼야할지 고민중
    public void ToggleCardSelection(CardUI cardUI)
    {
        if (currentState != InputState.SelectingCard) return;

        RuntimeCard cardData = cardUI.TargetRuntimeCard;
        CardUIEffect cardUIEffect = cardUI.GetComponent<CardUIEffect>();

        if (selectedCards.Contains(cardData))
        {
            selectedCards.Remove(cardData);
            cardUIEffect.SetSelectedState(false);
            selectedCardsUI.Remove(cardUI);
            cardUIEffect.ResetToOriginalState();
            CheckSelectionLimit();
        }
        else
        {
            if (selectedCards.Count >= maxSelectCount) return; // 횟수 제한

            selectedCards.Add(cardData);
            cardUIEffect.SetSelectedState(true);
            cardUIEffect.ResetRotation();
            selectedCardsUI.Add(cardUI);
            CheckSelectionLimit();
        }
        AlignSelectedCards(); 
    }

    private void AlignSelectedCards()
    {
        if (selectedCardsUI.Count == 0) return;

        float startX = -((selectedCardsUI.Count - 1) * cardSpacing / 2f);
        for (int i = 0; i < selectedCardsUI.Count; i++)
        {
            Vector2 targetPos = new Vector2(startX + (i * cardSpacing), 600);
            selectedCardsUI[i].transform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// SelectingCard 진행중 확인버튼을 누르면 실행되는 함수
    /// </summary>
    public void ConfirmSelection()
    {
        if (selectedCards.Count == 0) return;
        onSelectionConfirmed?.Invoke(new List<RuntimeCard>(selectedCards));

        // 초기화
        foreach (var cardUI in selectedCardsUI) cardUI.GetComponent<CardUIEffect>().ResetToOriginalState();
        UIManager.Instance.ShowCardSelectUI(false);
        currentState = InputState.None;
    }

    public void CheckSelectionLimit()
    {
        bool canConfirm = selectedCards.Count == maxSelectCount;

        UIManager.Instance.SetConfirmButtonInteractable(canConfirm);
    }

    /// <summary>
    /// CardUI 스크립트에서 클릭 이벤트(IPointerClickHandler 등)를 받았을 때 
    /// 인풋 매니저에게 나를 사용해달라고 요청하는 함수
    /// </summary>
    public void TrySelectCard(CardUI card)
    {
        switch(currentState)
        {
            case InputState.None:
                selectedCardUI = card;
                if (card.TargetRuntimeCard.GetCardEffectTarget() == EffectTarget.Target)
                {
                    UpdateCurrentState(InputState.SelectingTarget);
                    Debug.Log($"[InputManager] 카드 선택됨: {card.name}. 타겟을 선택하세요.");
                }
                else UpdateCurrentState(InputState.SelectedSkillCard);
                selectedCardUI.GetComponent<CardUIEffect>().ResetRotation();
                break;
            case InputState.SelectingCard:
                ToggleCardSelection(card);
                break;
        }


    }
    private void UpdateHoverTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, targetLayer);

        Monster foundMonster = (hit.collider != null) ? hit.collider.GetComponent<Monster>() : null;

        // 타겟이 바뀌었을 때만 이벤트 처리 (이전 타겟 해제, 새 타겟 하이라이트 등)
        if (hoverTarget != foundMonster)
        {
            if (hoverTarget != null) OnTargetExit(hoverTarget);
            hoverTarget = foundMonster;
            if (hoverTarget != null) OnTargetEnter(hoverTarget);
        }
    }

    private void OnTargetEnter(Monster monster)
    {
        // 여기서 몬스터의 하이라이트 효과를 켜는 함수 호출
        Debug.Log($"[InputManager] 타겟 조준 중: {monster.name}");
        selectedCardUI.UpdateUI(monster);
        monster.SetHighlight(true);   // 이거 미완성임 
    }

    private void OnTargetExit(Monster monster)
    {
        // 하이라이트 효과 끄기
        selectedCardUI.UpdateUI();
        monster.SetHighlight(false);   // 이것도;
    }
    /// <summary>
    /// Target이 필요한 카드 사용시에만 실행됨 - 타겟(몬스터)을 조준하고 클릭하거나 취소하는 로직
    /// </summary>
    private void HandleTargetSelection()
    {
        if (selectedCardUI == null)
        {
            hoverTarget = null;
            UpdateCurrentState(InputState.None);
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            CancelSelection();
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, targetLayer);

            if (hoverTarget != null) 
            {
                selectedCardUI.OnCardUsed(hoverTarget.gameObject);
                CancelSelection();
                selectedCardUI = null;
                hoverTarget = null;
                UpdateCurrentState(InputState.None);
            }
        }
    }

    /// <summary>
    /// 타겟이 필요없는 카드를 사용시 실행됨 - 마우스를 카드가 따라가게 하는 함수
    /// </summary>
    private void HandleMousePosition()
    {
        if(selectedCardUI == null)
        {
            hoverTarget = null;
            UpdateCurrentState(InputState.None);
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        selectedCardUI.GetComponent<CardUIEffect>().MoveCardPosition(mousePosition);

        if (Input.GetMouseButtonDown(1))
        {
            CancelSelection();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // if (EventSystem.current.IsPointerOverGameObject()) return;

            selectedCardUI.OnCardUsed();
            CancelSelection();
        }
    }



    /// <summary>
    /// 선택했던 카드를 취소하고 다시 손패로 안전하게 되돌리는 함수 (우클릭 등)
    /// </summary>
    public void CancelSelection()
    {
        if (currentState == InputState.None) return;

        Debug.Log("[InputManager] 카드 선택이 취소되었습니다.");

        if (hoverTarget != null)
        {
            OnTargetExit(hoverTarget);
            hoverTarget = null;
        }

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
        UpdateCurrentState(InputState.None);
        HandManager.Instance.AlignCards();
    }
}
