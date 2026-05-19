using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class CardUIEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float hoverMoveY = 50f;
    [SerializeField] private float duration = 0.2f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private int originalTransformIndex;

    void Start()
    {
        UpdateOriginalPosition();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InputManager.Instance != null && InputManager.Instance.currentState == InputState.SelectingTarget)
            return;

        transform.DOKill();

        originalTransformIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling(); // UI를 맨 앞으로 보내서 가려지지 않게 처리

        transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutCubic);
        transform.DOLocalMoveY(originalPosition.y + hoverMoveY, duration).SetEase(Ease.OutCubic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
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
    }

    public void UpdateOriginalPosition()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
}