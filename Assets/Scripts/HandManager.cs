using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [SerializeField] private RectTransform handLayoutGroup;
    [SerializeField] private CanvasGroup handCanvasGroup;
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

    private void OnEnable()
    {
        if (BattleFlowManager.Instance != null)
        {
            BattleFlowManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void HandleGameOver(GameOverType type)
    {
        StartCoroutine(CleanAllHands());
    }

    private IEnumerator CleanAllHands()
    {
        yield return handCanvasGroup.DOFade(0f, 0.5f).WaitForCompletion();

        foreach (Transform card in handCanvasGroup.transform)
        {
            card.gameObject.SetActive(false);
        }

        handCanvasGroup.alpha = 1f;

        handCardUIs.Clear();
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
            newCardObj.GetComponent<CardUIEffect>().SetSelectedState(false);
            handCardUIs.Add(newCardObj);
            StartCoroutine(CardDrawAnimation(newCardObj));
        }
        AlignCards();
    }
    private IEnumerator CardDrawAnimation(GameObject targetObj)  // 나중엔 드로우할카드 덱 아이콘에서 손패로 오게 짜기
    {
        targetObj.transform.localScale = Vector3.zero;
        targetObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        yield break;
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
        for (int i = 0; i < cardCount; i++)
        {
            handCardUIs[i].transform.SetSiblingIndex(i);
        }
        for (int i = 0; i < cardCount; i++)
        {
            RectTransform transform = handCardUIs[i].GetComponent<RectTransform>();
            float offset = i - midindex;
            float posX = offset * cardSpacing;
            float posY = offset * offset * arcIntensity + 150f;
            float rotZ = -offset * rotationIntensity;

            handCardUIs[i].GetComponent<CardUIEffect>().UpdateOriginalTransformIndex();
            transform.localPosition = new Vector3(posX, posY, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
            handCardUIs[i].GetComponent<CardUIEffect>().UpdateOriginalPosition();
            handCardUIs[i].GetComponent<CardUI>().UpdateUI();
        }
    }

    /// <summary>
    /// 손에서 버려지는 카드의 CardUI를 받아서 해당 오브젝트를 disactive하고 재정렬 하는 함수
    /// </summary>
    public IEnumerator RemoveCardFromHand(CardUI cardUI) // 버려지는 애니메이션 추가해야함
    {
        GameObject cardObj = cardUI.gameObject;
        if (handCardUIs.Contains(cardObj))
        {
            handCardUIs.Remove(cardObj);

            yield return cardObj.transform.DOScale(Vector3.zero, 0.1f)
            .SetEase(Ease.InBack)
            .WaitForCompletion();

            // 4. 애니메이션이 끝난 후 비활성화
            cardObj.SetActive(false);

        }
        yield break;
    }

    public IEnumerator DiscardAnimation(GameObject cardObj)
    {
        cardObj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            cardObj.SetActive(false);
        });
        yield break;
    }

}


