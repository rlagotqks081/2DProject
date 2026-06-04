using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("dtd")]
    // 드로우할 카드가 있는 파일
    public List<RuntimeCard> DrawPile {  get; private set; } = new List<RuntimeCard>();
    // 내 손에 있는 카드가 있는 파일
    public List<RuntimeCard> HandPile { get; private set; } = new List<RuntimeCard>();
    // 사용하거나 버려진 카드가 있는 파일
    public List<RuntimeCard> DiscardPile { get; private set; } = new List<RuntimeCard>();
    // 소멸한 카드가 있는 파일
    public List<RuntimeCard> ExhaustPile { get; private set; } = new List<RuntimeCard>();
    // 플레이어가 소유한 카드 덱이 있는 파일
    public List<RuntimeCard> CardDeck { get; private set; } = new List<RuntimeCard>();
    // 존재하는 RuntimeCard와 해당 카드의 UI를 매칭한 딕셔너리
    public Dictionary<RuntimeCard, CardUI> activeCardUIs { get; private set; } = new Dictionary<RuntimeCard, CardUI>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
        if (BattleFlowManager.Instance != null)
        {
            BattleFlowManager.Instance.OnGameOver += ResetCardData;
        }
    }

    public void ResetCardData(GameOverType result)
    {
        if(result == GameOverType.PlayerDead)
        {
            CardDeck.Clear();
            activeCardUIs.Clear();
        }
        HandPile.Clear();
        DiscardPile.Clear();
        DrawPile.Clear();
        ExhaustPile.Clear();
    }

    public void BaseCardSetup()
    {
        AddCardOnDeck(1004);
        AddCardOnDeck(1012);
        AddCardOnDeck(1005);
        AddCardOnDeck(1007);
        AddCardOnDeck(1011);
        AddCardOnDeck(1013);
    }

    public void SetupCards()
    {
        HandPile.Clear();
        DiscardPile.Clear();
        DrawPile.Clear();
        ExhaustPile.Clear();
        HandManager.Instance.SetupHand();

        DrawPile.AddRange(CardDeck);
        Debug.Log($"{DrawPile}");
        DrawCards(6);
    }

    /// <summary>
    /// 드로우하는 카드의 데이터(RuntimeCard)를 HandPile에 저장하는 함수
    /// </summary>
    /// <param name="amount"> 드로우하려는 카드의 수량</param>
    /// <param name="count"> 해당 효과의 실행 횟수</param>
    public void DrawCards(int amount, int count = 1)
    {
        for(int j = 0; j < count; j++)  // 실행횟수에 따라 반복
        {
            for (int i = 0; i < amount; i++)  // 드로우하는 카드의 수 만큼 반복
            {
                if (DrawPile.Count == 0)
                {
                    ShuffleDiscardIntoDrawPile();
                }
                if (DrawPile.Count > 0)
                {
                    RuntimeCard card = DrawPile[0];
                    DrawPile.RemoveAt(0);
                    HandPile.Add(card);
                    HandManager.Instance.AddCardToHand(card);
                }
            }
        }
    }

    /// <summary>
    /// 손에있는 모든카드를 버릴때 호출 - 턴종료시
    /// </summary>
    public IEnumerator DiscardAllCards()
    {
        List<RuntimeCard> tempCards = new List<RuntimeCard>(HandPile);
        foreach(RuntimeCard card in tempCards)
        {
            yield return new WaitForSeconds(0.1f);
            if(HandPile.Contains(card))
            {
                HandPile.Remove(card);
                DiscardPile.Add(card);
                StartCoroutine(HandManager.Instance.RemoveCardFromHand(activeCardUIs[card]));
            }
        }
        yield break;
    }
    public IEnumerator RemoveCardFromHand(RuntimeCard card)
    {
        if (HandPile.Contains(card))
        {
            HandPile.Remove(card);
            yield return HandManager.Instance.RemoveCardFromHand(activeCardUIs[card]);
        }
    }
    public void AddCardToDiscard(RuntimeCard card)
    {
        if(!DiscardPile.Contains(card))
        {
            DiscardPile.Add(card);
        }
    }




    /// <summary>
    /// 버려진 카드 폴더에 있는것 모두 드로우 파일에 옮기기 (파일 셔플도 같이하기)
    /// </summary>
    private void ShuffleDiscardIntoDrawPile()
    {
        if (DiscardPile.Count == 0) return;
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        // 여기서 DrawPile 셔플하는 로직 추가하기
    }

    /// <summary>
    /// 기존에 없던 신규카드를 덱에 추가할때 호출
    /// </summary>
    public void AddCardOnDeck(int CardID)
    {
        RuntimeCard newCard = new RuntimeCard(CardDatabase.Instance.GetCard(CardID));
        if(newCard != null)
        {
            CardDeck.Add(newCard);
        }
    }

    public void AddCardOnDeckByData(RuntimeCard card)
    {
        if(card != null && !CardDeck.Contains(card))
        {
            CardDeck.Add(card);
            Debug.Log($"[CardManager] 카드 추가됨 {card}");
        }
    }

    public void AddCardUIDic(RuntimeCard newCard, CardUI newCardUI)
    {
        if (newCard != null && newCardUI != null)
            activeCardUIs.Add(newCard, newCardUI);
        else Debug.Log("activeCardUIs 데이터 추가 실패");
    }

    public CardUI GetCardUI(RuntimeCard card)
    {
        if(card != null && activeCardUIs.ContainsKey(card))
        {
            return activeCardUIs[card];
        }
        return null;
    }
}
