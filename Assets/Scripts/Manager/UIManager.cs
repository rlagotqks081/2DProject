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
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI PlayerEnergyText;

    public Button confirmButton; 
    public Button endTurnButton;
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

        gameOverPanel.SetActive(type == GameOverType.PlayerDead);
        victoryPanel.SetActive(type == GameOverType.AllMonsterDead);

        resultCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
    }

    private IEnumerator ClearBattleUI()
    {
        yield return BattleCanvasGroup.DOFade(0f, 0.5f).WaitForCompletion();
        foreach(Transform ui in BattleCanvasGroup.transform)
        {
            ui.gameObject.SetActive(false);
        }
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

    public void OnClickConfirmButton() => InputManager.Instance.OnConfirmButtonClicked();
}
