using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class CardUIEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float hoverMoveY = 100f;
    [SerializeField] private float duration = 0.2f;

    private Vector3 originalPosition;
    private Vector3 originalScale = Vector3.one;
    private Quaternion originalRotation;
    private int originalTransformIndex;
    public bool isSelected = false;

    void Start()
    {
        UpdateOriginalPosition();
    }

    public void SetSelectedState(bool selected)
    {
        isSelected = selected;
        if (isSelected)
        {
            // 선택된 상태라면 즉시 확대 효과 취소 (원래 크기로 복구)
            ResetToOriginalState();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InputManager.Instance.currentState == InputState.SelectedSkillCard) return;
        if (isSelected || InputManager.Instance.currentState == InputState.Processing) return;
        if (InputManager.Instance != null && InputManager.Instance.currentState == InputState.SelectingTarget)
            return;

        transform.DOKill();

        UpdateOriginalTransformIndex();

        transform.SetAsLastSibling(); // UI를 맨 앞으로 보내서 가려지지 않게 처리

        transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutCubic);
        transform.DOLocalMoveY(originalPosition.y + hoverMoveY, duration).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InputManager.Instance.currentState == InputState.SelectedSkillCard) return;
        if (isSelected || InputManager.Instance.currentState == InputState.Processing) return;
        if (InputManager.Instance != null && InputManager.Instance.currentState == InputState.SelectingTarget)
            return;

        ResetToOriginalState();
    }

    /// <summary>
    /// 카드를 정상적으로 내려놓거나 취소했을 때, 원래 자리로 부드럽게 되돌리는 안전장치 함수
    /// </summary>
    public void ResetToOriginalState()
    {
        transform.DOKill();

        transform.SetSiblingIndex(originalTransformIndex);

        transform.DOScale(originalScale, duration).SetEase(Ease.OutCubic);
        transform.DOLocalMove(originalPosition, duration).SetEase(Ease.OutCubic);
        transform.localRotation = originalRotation;
    }

    /// <summary>
    /// 카드를 마우스의 위치로 부드럽게 이동시키는 함수
    /// </summary>
    public void MoveCardPosition(Vector3 newPosition)
    {
        transform.DOMove(newPosition, 0.1f).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// 카드의 현재회전값을 0,0,0으로 초기화
    /// </summary>
    public void ResetRotation()
    {
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 현재 카드의 위치, 기울임정도 를 저장함
    /// </summary>
    public void UpdateOriginalPosition()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void UpdateOriginalTransformIndex()
    {
        originalTransformIndex = transform.GetSiblingIndex();
    }
}