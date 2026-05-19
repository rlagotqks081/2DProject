using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public RuntimeCard TargetRuntimeCard { get; private set; }

    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Image artworkImage;      // 카드 일러스트
    [SerializeField] private TextMeshProUGUI nameText;    // 카드 이름
    [SerializeField] private TextMeshProUGUI costText;    // 마나 비용
    [SerializeField] private TextMeshProUGUI descText;    // 카드 설명

    /// <summary>
    /// 카드 생성 시점에서 데이터와 UI를 연결하는 함수
    /// </summary>
    public void SetupUI(RuntimeCard runtimeCard)
    {
        TargetRuntimeCard = runtimeCard;

        // 원본 데이터(OriginData)에서 정보를 가져와 UI에 주입
        var origin = runtimeCard.OriginData;

        //if (artworkImage != null) artworkImage.sprite = origin.cardIllustration;   ----- 나중에 경로넣으면 하기
        if (nameText != null) nameText.text = origin.cardName;
        if (descText != null) descText.text = origin.description;

        //런타임에 계산된 비용(강화나 디버프 효과 적용값)을 표시
        UpdateUI();
    }

    /// <summary>
    /// 비용이나 수치가 변했을 때 UI만 갱신할 때 사용
    /// </summary>
    public void UpdateUI()
    {
        if (costText != null && TargetRuntimeCard != null)
        {
            costText.text = TargetRuntimeCard.GetCalculatedCost().ToString();
        }
    }

    /// <summary>
    /// 유니티 이벤트 시스템: 카드 클릭 감지
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭 시에만 인풋 매니저에게 나를 선택해달라고 요청
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.TrySelectCard(this);
            }
        }
    }

    /// <summary>
    /// 카드가 최종적으로 사용되었을 때 호출 (InputManager -> 여기로 호출)
    /// </summary>
    public void OnCardUsed(GameObject targetMonster)
    {
        // 1. 배틀 매니저에게 내 데이터와 타겟 정보를 던짐
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.PlayerUseCard(TargetRuntimeCard, targetMonster);
        }


    }
}