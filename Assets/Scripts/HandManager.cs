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

    private List<GameObject> activeCardUIs = new List<GameObject>();


    /// <summary>
    /// 카드 데이터(RuntimeCard)를 받아 화면에 생성하고 손패에 추가하는 함수
    /// </summary>
    public void AddCardToHand(RuntimeCard cardData)
    {
        // 1. 프리팹 생성
        GameObject newCardObj = Instantiate(cardPrefab, handLayoutGroup);
        CardUI cardUI = newCardObj.GetComponent<CardUI>();

        // 2. 데이터 연동 (SetupUI는 아까 만든 것)
        cardUI.SetupUI(cardData);

        // 3. 리스트 관리
        activeCardUIs.Add(newCardObj);

        // 4. [연출] 드로우 애니메이션
        newCardObj.transform.localScale = Vector3.zero;
        newCardObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // 5. 손패 정렬 로직 호출 (부채꼴 혹은 일렬 정렬)
        AlignCards();
    }

    [ContextMenu("AlignCards")]
    public void AlignCards()
    {
        int cardCount = activeCardUIs.Count;
        if (cardCount == 0) return;

        float midindex = (cardCount - 1) / 2f;

        for(int i = 0; i < cardCount; i++)
        {
            float offset = i - midindex;
            float posX = offset * cardSpacing;
            float posY = offset * offset * arcIntensity + 100f;
            float rotZ = -offset * rotationIntensity;
            activeCardUIs[i].GetComponent<RectTransform>().localPosition = new Vector3(posX, posY, 0f);
            activeCardUIs[i].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, rotZ);

        }
    }

    /// <summary>
    /// 카드가 사용되거나 버려질 때 호출
    /// </summary>
    public void RemoveCardFromHand(GameObject cardObj)
    {
        if (activeCardUIs.Contains(cardObj))
        {
            activeCardUIs.Remove(cardObj);
            Destroy(cardObj); 
            AlignCards();   
        }
    }
}
