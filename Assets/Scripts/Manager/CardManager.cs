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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 카드 드로우하기
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
                //카드가 드로우되는 애니메이션 넣기 (여기에 넣을지 카드를 다뽑고난 뒤에 넣을지 테스트해야함)
            }
        }
    }

    public void DiscardAllCards()
    {
        foreach(RuntimeCard card in HandPile)
        {
            if(HandPile.Contains(card))
            {
                HandPile.Remove(card);
                DiscardPile.Add(card);
                //버려지는 애니메이션 추가
            }
        }
    }
    
    //카드를 정상적으로 사용해서 버려질때
    public void UseCardToDiscard(RuntimeCard card)
    {
        if (HandPile.Contains(card))
        {
            HandPile.Remove(card);
            DiscardPile.Add(card);
            // 여기에 버려지는 애니메이션 같은것 추가하기
            HandManager.Instance.RemoveCardFromHand()
        }
    }

    // 카드가 소멸할때
    public void UseCardToExhaust(RuntimeCard card)
    {
        if (HandPile.Contains(card)) 
        {
            HandPile.Remove(card);
            ExhaustPile.Add(card);
            //카드가 소멸하는 애니메이션 넣기
        }
    }

    // 버리기 효과로 패에서 버려질때
    public void DiscardFromHand(RuntimeCard card)
    {
        if (HandPile.Contains(card))
        {
            HandPile.Remove(card);
            DiscardPile.Add(card);

            // 여기에 카드가 버려졌을때 사용되는 카드 체크후 실행하는 로직 추가하기
            // 추가로 카드가 버려지는 애니메이션 - CardUIEffect에서 출력하기
        }
    }

    // 버려진 카드 폴더에 있는것 모두 드로우 파일에 옮기기 (파일 셔플도 같이하기)
    private void ShuffleDiscardIntoDrawPile()
    {
        if (DiscardPile.Count == 0) return;
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        // 여기서 DrawPile 셔플하는 로직 추가하기
    }

    public void AddCardOnDeck(RuntimeCard card)
    {

    }
}
