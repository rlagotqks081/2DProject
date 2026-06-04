using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject cardSelectUI;
    [SerializeField] private CanvasGroup BattleCanvasGroup; // 배틀화면의 부모 canvasgroup
    [SerializeField] private CanvasGroup resultCanvasGroup; // 결과화면의 부모 canvasgroup
    [SerializeField] public GameObject CardListPopup;
    [SerializeField] public GameObject MapPopup;
    [SerializeField] public RectTransform CardListContentparent;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI PlayerEnergyText;
    [SerializeField] private CanvasGroup CardChoiceUI;
    [SerializeField] private CanvasGroup RewardUI;
    [SerializeField] private Image BackGround_Dark;
    [SerializeField] private CardUI CardUI_1;
    [SerializeField] private CardUI CardUI_2;
    [SerializeField] private CardUI CardUI_3;

    public Button confirmButton; 
    public Button endTurnButton;
    public Button CardDeckListButton;
    public Button TestButton;
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

    private void Start()
    {
        if (BattleFlowManager.Instance != null)
        {
            BattleFlowManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    public void OnClickConfirmButton() => InputManager.Instance.OnConfirmButtonClicked();
    public void OnClickCardDeckListButton() => InputManager.Instance.OnCardDeckListButtonClicked();
    public void OnClickTestButton() => InputManager.Instance.OnTestButtonClicked();
    public void OnClickMapOpenButton() => InputManager.Instance.OnOpenMapButtonClicked();

    public void UpdatePlayerEnergyText()
    {
        if(Player.Instance != null)
        {
            PlayerEnergyText.text = Player.Instance.currentEnergy.ToString() + "/" + Player.Instance.maxEnergy.ToString();
        }
    }

    private void HandleGameOver(GameOverType type)
    {
        StartCoroutine(ClearBattleUI());
        InputManager.Instance.UpdateCurrentState(InputState.Result);
        SetBackgroundDark(true);
        gameOverPanel.SetActive(type == GameOverType.PlayerDead);
        victoryPanel.SetActive(type == GameOverType.AllMonsterDead);
        AddDataOnResultChoiceCard();
        StartCoroutine(AppearUIByFadeAction(resultCanvasGroup));
        
    }

    private IEnumerator ClearBattleUI()
    {
        yield return BattleCanvasGroup.DOFade(0f, 0.5f).WaitForCompletion();
        if (BattleFlowManager.Instance.IsGameOver == false) yield break;
        BattleCanvasGroup.gameObject.SetActive(false);
        BattleCanvasGroup.alpha = 1f;
    }

    public void SetConfirmButtonInteractable(bool isInteractable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = isInteractable;
        }
    }
    public void SetEndTurnButtonInteractable(bool isInteractable)
    {
        if(endTurnButton != null)
        {
            endTurnButton.interactable = isInteractable;
        }
    }
    public void ShowCardSelectUI(bool isShow)
    {
        cardSelectUI.SetActive(isShow);
        foreach (Transform child in cardSelectUI.transform)
        {
            child.gameObject.SetActive(isShow);
        }
    }

    public void ResultCardChoiced()
    {
        StartCoroutine(DisableUIByFadeAction(CardChoiceUI));
        StartCoroutine(AppearUIByFadeAction(RewardUI));
    }

    public void CardResultChoice()
    {
        StartCoroutine(DisableUIByFadeAction(RewardUI));
        StartCoroutine(AppearUIByFadeAction(CardChoiceUI));
    }

    public void DiscardAnimation(GameObject cardObj)
    {
        cardObj.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack);
    }

    public IEnumerator AppearUIByFadeAction(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        yield return group.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();
    }
    public IEnumerator DisableUIByFadeAction(CanvasGroup group)
    {
        yield return group.DOFade(0f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        group.gameObject.SetActive(false);
    }

    public void SetBackgroundDark(bool isDark)
    {
        if (isDark)
        {
            BackGround_Dark.gameObject.SetActive(true);
            BackGround_Dark.DOFade(0.9f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();
        }
        else
        {
            if (InputManager.Instance.currentState == InputState.Result) return;
            BackGround_Dark.DOFade(0f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();
            BackGround_Dark.gameObject.SetActive(false);
        }

    }

    public void SetCardDeckList()
    {
        foreach(RectTransform child in CardListContentparent)
        {
            Destroy(child.gameObject);
        }
        foreach(RuntimeCard card in CardManager.Instance.CardDeck)
        {
            GameObject newCard = Instantiate(cardPrefab, CardListContentparent);
            newCard.GetComponent<CardUI>().SetupUI(card);
        }

    }
    // 아래는 임시 함수 다른매니저로 역할을 옮겨야함

    public void AddDataOnResultChoiceCard()
    {
        CardUI_1.SetupUI(new RuntimeCard(CardDatabase.Instance.GetCard(1001)));
        CardUI_2.SetupUI(new RuntimeCard(CardDatabase.Instance.GetCard(1002)));
        CardUI_3.SetupUI(new RuntimeCard(CardDatabase.Instance.GetCard(1003)));
    }

    public void ResetAllUI()
    {
        StartCoroutine(AppearUIByFadeAction(BattleCanvasGroup));
        resultCanvasGroup.gameObject.SetActive(false);
        ShowCardSelectUI(false);
    }

}
