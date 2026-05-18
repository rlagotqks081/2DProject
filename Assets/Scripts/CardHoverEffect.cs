using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Runtime.CompilerServices;
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        transform.DOKill();

        originalTransformIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutCubic);
        transform.DOLocalMoveY(originalPosition.y + hoverMoveY, duration).SetEase(Ease.OutCubic);
    }
    public void OnPointerExit(PointerEventData eventData)
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
