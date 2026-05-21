using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

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

    public void TestSetup()
    {
        AddCardOnDeck(1003);
        AddCardOnDeck(1004);
        AddCardOnDeck(1005);
        AddCardOnDeck(1006);
        AddCardOnDeck(1007);
        AddCardOnDeck(1008);
        AddCardOnDeck(1009);
        AddCardOnDeck(1010);
        AddCardOnDeck(1011);
        SetupCards();
    }

    public void SetupCards()
    {
        DrawPile.Clear();
        DrawPile.AddRange(CardDeck);
        DrawCards(6);
    }

    [ContextMenu("DrawCards")]
    /// <summary>
    /// 드로우하는 카드의 데이터(RuntimeCard)를 HandPile에 저장하는 함수
    /// </summary>
    /// <param name="amount"> 드로우하려는 카드의 수량</param>
    public void DrawCards(int amount)
    {
        for(int i = 0; i < amount; i++)
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

    /// <summary>
    /// 손에있는 모든카드를 버릴때 호출
    /// </summary>
    public void DiscardAllCards()
    {
        foreach(RuntimeCard card in HandPile)
        {
            if(HandPile.Contains(card))
            {
                HandPile.Remove(card);
                DiscardPile.Add(card);
                HandManager.Instance.RemoveCardFromHand(activeCardUIs[card]);
            }
        }
    }
    
    /// <summary>
    /// 카드를 정상적으로 사용해서 해당 카드를 버릴때 호출
    /// </summary>
    public void UseCardToDiscard(RuntimeCard card)
    {
        if (HandPile.Contains(card))
        {
            HandPile.Remove(card);
            DiscardPile.Add(card);
            HandManager.Instance.RemoveCardFromHand(activeCardUIs[card]);
        }
    }

    /// <summary>
    /// 카드가 소멸되어서 손에서 사라질때 호출
    /// </summary>
    public void UseCardToExhaust(RuntimeCard card)
    {
        if (HandPile.Contains(card)) 
        {
            HandPile.Remove(card);
            ExhaustPile.Add(card);
            HandManager.Instance.RemoveCardFromHand(activeCardUIs[card]);  //일단 핸드매니저의 버리는함수를 넣었지만 나중엔 바꿀수도 
        }
    }

    /// <summary>
    /// 버리기효과로 손에서 카드를 버릴때 호출
    /// </summary>
    public void DiscardFromHand(RuntimeCard card)
    {
        if (HandPile.Contains(card))
        {
            HandPile.Remove(card);
            DiscardPile.Add(card);

            // 여기에 카드가 버려졌을때 사용되는 카드 체크후 실행하는 로직 추가하기
            HandManager.Instance.RemoveCardFromHand(activeCardUIs[card]);
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

    public void AddCardUIDic(RuntimeCard newCard, CardUI newCardUI)
    {
        if (newCard != null && newCardUI != null)
            activeCardUIs.Add(newCard, newCardUI);
        else Debug.Log("activeCardUIs 데이터 추가 실패");
    }
}
