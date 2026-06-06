using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private Image BackgroundDark_Default;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject RewardButtonPrefab;

    [Header("BattleUI")]
    [SerializeField] private CanvasGroup BattleCanvasGroup; // 배틀화면의 부모 canvasgroup
    [SerializeField] public Button confirmButton;
    [SerializeField] public Button endTurnButton;
    [SerializeField] private GameObject cardSelectUI;
    [SerializeField] private TextMeshProUGUI PlayerEnergyText;

    [Header("ResultUI")]
    [SerializeField] private CanvasGroup resultCanvasGroup; // 결과화면의 부모 canvasgroup
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private CanvasGroup CardChoiceUI;
    [SerializeField] private CanvasGroup RewardUI;
    [SerializeField] private RectTransform RewardButtonContainer;
    [SerializeField] private CardUI CardUI_1;
    [SerializeField] private CardUI CardUI_2;
    [SerializeField] private CardUI CardUI_3;

    [Header("DefaultUI")]
    [SerializeField] public GameObject HighBarUI;

    [Header("PopupUI")]
    [SerializeField] private Image BackgroundDark_Popup;
    [SerializeField] public RectTransform CardListContentparent;
    [SerializeField] public GameObject CardListPopup;
    [SerializeField] public GameObject MapPopup;

    [Header("MainMenuUI")]
    [SerializeField] public GameObject MainMenuUI;






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
    public void OnClickGameStartButton() => InputManager.Instance.OnGameStartButtonClicked();

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
        ClearAllChildren(RewardButtonContainer);
        InputManager.Instance.UpdateCurrentState(InputState.Result);
        SetBackgroundDark(true);
        gameOverPanel.SetActive(type == GameOverType.PlayerDead);
        victoryPanel.SetActive(type == GameOverType.AllMonsterDead);
        SpawnRewardButtons();
        AddDataOnResultChoiceCard();
        StartCoroutine(AppearUIByFadeAction(resultCanvasGroup));
        
    }

    private void SpawnRewardButtons()
    {
        GameObject button = Instantiate(RewardButtonPrefab, RewardButtonContainer);
        button.GetComponent<Button>().onClick.AddListener(CardResultChoice);
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
        group.DOFade(1f, 0f);
    }

    public void SetBackgroundDark(bool isDark)
    {
        if (isDark)
        {
            BackgroundDark_Default.gameObject.SetActive(true);
            BackgroundDark_Default.DOFade(0.9f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();
        }
        else
        {
            BackgroundDark_Default.DOFade(0f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();
            BackgroundDark_Default.gameObject.SetActive(false);
        }
    }
    public void SetPopupBackGroundDark(bool isDark)
    {
        if (isDark)
        {
            BackgroundDark_Popup.gameObject.SetActive(true);
            BackgroundDark_Popup.DOFade(0.9f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();
        }
        else
        {
            if (InputManager.Instance.currentState == InputState.Result)
            {
                BackgroundDark_Popup.gameObject.SetActive(false);
                SetBackgroundDark(true);
            }
            else
            {
                BackgroundDark_Popup.DOFade(0f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();
                BackgroundDark_Popup.gameObject.SetActive(false);
            }

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
        CardUI_1.SetupUI(new RuntimeCard(CardDatabase.Instance.GetRandomCard()));
        CardUI_2.SetupUI(new RuntimeCard(CardDatabase.Instance.GetRandomCard()));
        CardUI_3.SetupUI(new RuntimeCard(CardDatabase.Instance.GetRandomCard()));
    }

    public void ResetAllUI()
    {
        StartCoroutine(AppearUIByFadeAction(BattleCanvasGroup));
        resultCanvasGroup.gameObject.SetActive(false);
        ShowCardSelectUI(false);
    }

    public void ClearAllChildren(RectTransform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            Destroy(child.gameObject);
        }
    }

    public void ResetRewardUI()
    {
        CardChoiceUI.gameObject.SetActive(false);
        RewardUI.gameObject.SetActive(true);
        RewardButtonContainer.gameObject.SetActive(true);  
        confirmButton.gameObject.SetActive(true);
    }



}
