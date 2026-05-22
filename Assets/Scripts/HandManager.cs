using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [SerializeField] private RectTransform handLayoutGroup;
    [SerializeField] private GameObject cardPrefab;

    [Header("Hand settings")]
    [SerializeField] private float cardSpacing = 150f;
    [SerializeField] private float arcIntensity = -5f;
    [SerializeField] private float rotationIntensity = 5f;

    // 현재 핸드에 있는 카드 오브젝트
   [SerializeField] private List<GameObject> handCardUIs = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetupHand()
    {
        for(int i = handLayoutGroup.childCount - 1; i >= 0; i--)
        {
            Destroy(handLayoutGroup.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 카드 데이터(RuntimeCard)를 받아 화면에 생성하고 손패에 추가하는 함수
    /// </summary>
    public void AddCardToHand(RuntimeCard cardData)
    {
        GameObject newCardObj;

        if(CardManager.Instance.activeCardUIs.ContainsKey(cardData))
        {
            newCardObj = CardManager.Instance.activeCardUIs[cardData].gameObject;
            newCardObj.SetActive(true);
            newCardObj.GetComponent<CardUI>().UpdateUI();
        }
        else
        {
            newCardObj = Instantiate(cardPrefab, handLayoutGroup);
            CardUI newCardUI = newCardObj.GetComponent<CardUI>();
            CardManager.Instance.AddCardUIDic(cardData, newCardUI);

            newCardUI.SetupUI(cardData);
        }

        if (newCardObj != null)
        {
            handCardUIs.Add(newCardObj);
            newCardObj.transform.localScale = Vector3.zero;
            newCardObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        newCardObj.transform.SetAsLastSibling();
        AlignCards();
    }

    /// <summary>
    /// 손에 있는 CardUI오브젝트들을 부채꼴로 정렬하는 함수
    /// </summary>
    [ContextMenu("AlignCards")]
    public void AlignCards()
    {
        int cardCount = handCardUIs.Count;
        if (cardCount == 0) return;

        float midindex = (cardCount - 1) / 2f;

        for(int i = 0; i < cardCount; i++)
        {
            float offset = i - midindex;
            float posX = offset * cardSpacing;
            float posY = offset * offset * arcIntensity + 150f;
            float rotZ = -offset * rotationIntensity;
            
            handCardUIs[i].GetComponent<RectTransform>().localPosition = new Vector3(posX, posY, 0f);
            handCardUIs[i].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, rotZ);
            handCardUIs[i].GetComponent<CardUIEffect>().UpdateOriginalPosition();
            handCardUIs[i].GetComponent<CardUI>().UpdateUI();
        }
    }

    /// <summary>
    /// 손에서 버려지는 카드의 CardUI를 받아서 해당 오브젝트를 disactive하고 재정렬 하는 함수
    /// </summary>
    public void RemoveCardFromHand(CardUI cardUI) // 버려지는 애니메이션 추가해야함
    {
        GameObject cardObj = cardUI.gameObject;
        if (handCardUIs.Contains(cardObj))
        {
            handCardUIs.Remove(cardObj);
            cardObj.SetActive(false);
            AlignCards();   
        }
    }
}
