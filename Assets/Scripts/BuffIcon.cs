using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public BuffType type;


    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.SetBuffPopup(true, type);
        UIManager.Instance.UpdateBuffPopupPosition();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.SetBuffPopup(false, type);
    }

    public void OnPointerMove(PointerEventData eventData) 
    {
        UIManager.Instance.UpdateBuffPopupPosition();
    }


}
