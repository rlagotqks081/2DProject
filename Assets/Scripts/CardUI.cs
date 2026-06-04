using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public RuntimeCard TargetRuntimeCard { get; private set; }

    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Image CardImage;      // 카드 일러스트
    [SerializeField] private Image BaseImage;      // 카드 테두리 일러스트
    [SerializeField] private TextMeshProUGUI nameText;    // 카드 이름
    [SerializeField] private TextMeshProUGUI costText;    // 마나 비용
    [SerializeField] private TextMeshProUGUI descText;    // 카드 설명
    [SerializeField] private TextMeshProUGUI typeText;    // 카드 종류
    [SerializeField] public RectTransform rectTransform;    



    [ContextMenu("CardUpgradeTest")]
    public void CardUpgradeTest()
    {
        TargetRuntimeCard.UpgradeCard();
    }


    /// <summary>
    /// 카드 생성 시점에서 데이터와 UI를 연결하는 함수
    /// </summary>
    public void SetupUI(RuntimeCard runtimeCard)
    {
        TargetRuntimeCard = runtimeCard;
        var origin = runtimeCard.OriginData;

        //if (artworkImage != null) artworkImage.sprite = origin.cardIllustration;   ----- 나중에 경로넣으면 하기

        //런타임에 계산된 비용(강화나 디버프 효과 적용값)을 표시
        UpdateUI();
    }

    /// <summary>
    /// 비용이나 수치가 변했을 때 UI만 갱신할 때 사용
    /// </summary>
    public void UpdateUI(Monster target = null) // 카드 설명에서 강화된 수치는 어떻게 적용할지 아직 추가하지않음
    {
        if (costText != null && TargetRuntimeCard != null)
        {
            costText.text = TargetRuntimeCard.GetCalculatedCost().ToString();
        }
        if (nameText != null) nameText.text = TargetRuntimeCard.OriginData.cardName;
        if (descText != null) descText.text = TargetRuntimeCard.GetDescription(DescriptionType.Default,target);
        if (typeText != null) typeText.text = TargetRuntimeCard.OriginData.GetCardType().ToString();
    }

    /// <summary>
    /// 유니티 이벤트 시스템: 카드 클릭 감지
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (InputManager.Instance.currentState == InputState.Processing) return;
        // 좌클릭 시에만 인풋 매니저에게 나를 선택해달라고 요청
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.HandleCardClick(TargetRuntimeCard);
            }
        }
        if (InputManager.Instance.currentState == InputState.Result) UIManager.Instance.DiscardAnimation(this.gameObject);

    }


}