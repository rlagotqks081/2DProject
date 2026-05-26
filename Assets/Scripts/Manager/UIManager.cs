using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject cardSelectUI;

    public Button confirmButton; // 인스펙터에서 버튼 할당
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


    public void SetConfirmButtonInteractable(bool isInteractable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = isInteractable;
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

    public void OnClickConfirmButton() => InputManager.Instance.ConfirmSelection();
}
