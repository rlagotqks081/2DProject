using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public enum InputState
{
    Idle,
    SelectingTarget,
    SelectingCard,
    SelectedSkillCard,
    Processing,
    CardListPopupOpened,
    MapOpened,
    Result
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private Stack<InputState> StateHistory = new Stack<InputState>();
    [Header("현재 입력 상태")]
    public InputState currentState = InputState.Idle;

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
    public event Action<RuntimeCard> OnCardClicked;



    // 여기서부터 갈아엎으면서 만든거
    public event Action OnLeftClick;
    public event Action OnRightClick;
    private bool isMandatory;

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
            case InputState.SelectingTarget:
                UpdateHoverTarget();
                HandleTargetSelection();
                break;
            case InputState.SelectedSkillCard:
                if (Input.GetMouseButtonDown(0)) OnLeftClick?.Invoke();
                if (Input.GetMouseButtonDown(1)) OnRightClick?.Invoke();
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
    private void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentState == InputState.SelectingTarget)
            {
                if (hoverTarget != null)
                {
                    Monster m = hoverTarget;
                    if (m != null) BattleManager.Instance.SelectTarget(m);
                }
            }
            else
            {
                OnLeftClick?.Invoke();
            }
        }

        if (Input.GetMouseButtonDown(1)) OnRightClick?.Invoke();
    }

    public void HandleCardClick(RuntimeCard card)
    {
        switch (currentState)
        {
            case InputState.Idle:
                selectedCardUI = CardManager.Instance.GetCardUI(card);
                StartCoroutine(BattleManager.Instance.PlayCardRoutine(card));
                break;
            case InputState.SelectingCard:
                ToggleCardSelection(CardManager.Instance.GetCardUI(card));
                break;
            case InputState.SelectingTarget:
            case InputState.Processing:
                Debug.Log("현재 카드를 클릭할 수 없는 상태입니다.");
                break;
            case InputState.Result:
                CardManager.Instance.AddCardOnDeckByData(card);
                UIManager.Instance.ResultCardChoiced();
                break;
            case InputState.CardListPopupOpened:
                break;

        }

    }

    public void OnConfirmButtonClicked()
    {
        var result = new List<RuntimeCard>(selectedCards);



        onSelectionConfirmed?.Invoke(result);

        selectedCards.Clear();
        UIManager.Instance.confirmButton.gameObject.SetActive(false);
        UIManager.Instance.SetConfirmButtonInteractable(false);
        UIManager.Instance.ShowCardSelectUI(false);
        currentState = InputState.Idle;            // 상태를 기본으로 복귀
    }

    public void OnCardDeckListButtonClicked()
    {
        if (currentState == InputState.CardListPopupOpened)
        {
            RestorePreState();
            UIManager.Instance.CardListPopup.SetActive(false);
            UIManager.Instance.SetBackgroundDark(false);
        }
        else if (currentState == InputState.MapOpened)
        {
            UpdateCurrentState(InputState.CardListPopupOpened, true);
            UIManager.Instance.CardListPopup.SetActive(true);
            UIManager.Instance.SetCardDeckList();
            UIManager.Instance.MapPopup.SetActive(false);
        }
        else
        {
            CancelSelection();
            UpdateCurrentState(InputState.CardListPopupOpened, true);
            UIManager.Instance.CardListPopup.SetActive(true);
            UIManager.Instance.SetCardDeckList();
            UIManager.Instance.SetBackgroundDark(true);
        }
    }

    public void OnOpenMapButtonClicked()
    {
        if (currentState == InputState.MapOpened)
        {
            RestorePreState();
            UIManager.Instance.MapPopup.SetActive(false);
            UIManager.Instance.SetBackgroundDark(false);
        }
        else if (currentState == InputState.CardListPopupOpened)
        {
            UpdateCurrentState(InputState.MapOpened, true);
            UIManager.Instance.MapPopup.SetActive(true);
            UIManager.Instance.CardListPopup.SetActive(false);
        }
        else
        {
            CancelSelection();
            UpdateCurrentState(InputState.MapOpened, true);
            UIManager.Instance.MapPopup.SetActive(true);
            UIManager.Instance.SetBackgroundDark(true);
        }
    }

    public void StartSelectingMultipleCards(int maxSelectCount, Action<List<RuntimeCard>> onConfirmed, bool isMandatory = true)
    {
        currentState = InputState.SelectingCard;
        this.isMandatory = isMandatory;
        this.maxSelectCount = maxSelectCount;
        selectedCards.Clear();
        selectedCardsUI.Clear();
        UIManager.Instance.ShowCardSelectUI(true);

        // 이 Action이 나중에 BattleManager의 tcs.SetResult를 호출
        this.onSelectionConfirmed = onConfirmed;

        UIManager.Instance.confirmButton.gameObject.SetActive(true);
        UIManager.Instance.SetConfirmButtonInteractable(false);

    }

    public void RestorePreState()
    {
        if(StateHistory.Count > 0)
        {
            InputState preState = StateHistory.Pop();
            if(preState == InputState.SelectingTarget || preState == InputState.SelectedSkillCard)
            {
                UpdateCurrentState(InputState.Idle);
            }
            else UpdateCurrentState(preState, true);
        }
        else
        {
            Debug.LogError("[InputManager] InputState 이전 상태가 없음");
        }
    }

    /// <summary>
    /// InputState를 바꾸는 함수, Result 상태에선 isForceExecute에 true를 줘야 바꿀수있음
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isForceExecute">true면 강제로 변환, false면 특정상태일때 currentState가 바뀌지않음</param>
    public void UpdateCurrentState(InputState state, bool isForceExecute = false)
    {
        if (currentState == InputState.Result && !isForceExecute) return;
        if(currentState != InputState.CardListPopupOpened && currentState != InputState.MapOpened)
        {
            StateHistory.Push(currentState);
        }
 

        currentState = state;
        if (currentState == InputState.Processing)
        {
            UIManager.Instance.SetEndTurnButtonInteractable(false);
            UIManager.Instance.SetConfirmButtonInteractable(false);
        }
        else if (currentState == InputState.Idle) 
        {
            UIManager.Instance.SetEndTurnButtonInteractable(true);
            UIManager.Instance.SetConfirmButtonInteractable(false);
        }
        else if (currentState == InputState.SelectingCard)
        {
            UIManager.Instance.SetEndTurnButtonInteractable(false);
            UIManager.Instance.SetConfirmButtonInteractable(true);
        }
        else if (currentState == InputState.Result)
        {
            UIManager.Instance.SetEndTurnButtonInteractable(false);
            UIManager.Instance.SetConfirmButtonInteractable(false);
        }
        else
        {
            UIManager.Instance.SetEndTurnButtonInteractable(false);
            UIManager.Instance.SetConfirmButtonInteractable(false);
        }
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
        }
        else
        {
            if (selectedCards.Count >= maxSelectCount) return; // 횟수 제한

            selectedCards.Add(cardData);
            cardUIEffect.SetSelectedState(true);
            cardUIEffect.ResetRotation();
            selectedCardsUI.Add(cardUI);
        }

        if(isMandatory)
        {
            if(selectedCards.Count == maxSelectCount)
            {
                UIManager.Instance.SetConfirmButtonInteractable(true);
            }
            else if (selectedCards.Count < maxSelectCount)
            {
                UIManager.Instance.SetConfirmButtonInteractable(false);
            }
        }
        else 
        {
            UIManager.Instance.SetConfirmButtonInteractable(true);
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
    /// 선택했던 카드를 취소하고 다시 손패로 안전하게 되돌리는 함수 (우클릭 등)
    /// </summary>
    public void CancelSelection()
    {
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
    }

    public void OnTestButtonClicked()
    {
        UpdateCurrentState(InputState.Idle, true);
        UIManager.Instance.SetBackgroundDark(false);
        UIManager.Instance.ResetAllUI();
        GameManager.Instance.ChangeState(GameState.WinBattle);
    }
}
