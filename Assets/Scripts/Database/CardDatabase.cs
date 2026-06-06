using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }


    public Dictionary<int, CardData> cardDictionary= new Dictionary<int, CardData>();
    public List<int> cardIDList = new List<int>();

    private void Awake()
    {
        Instance = this;
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("JsonFiles/CardData");

        if (jsonFile == null)
        {
            Debug.LogError("파일 없음!");
            return;
        }

        // JsonUtility 대신 JsonConvert 사용
        CardListWrapper wrapper = JsonConvert.DeserializeObject<CardListWrapper>(jsonFile.text);

        if (wrapper == null || wrapper.cards == null)
        {
            Debug.LogError("Wrapper 파싱 실패! JSON 구조를 확인하세요.");
            return;
        }

        foreach (var data in wrapper.cards)
        {
            cardDictionary[data.cardID] = data;
            cardIDList.Add(data.cardID);
        }

        Debug.Log($"[CardDatabase] 총 {cardDictionary.Count}개의 카드가 로드되었습니다.");
    }

    public CardData GetCard(int id)
    {
        return cardDictionary.TryGetValue(id, out CardData data) ? data : null;
    }
    public CardData GetRandomCard()
    {
        int index = Random.Range(0, cardIDList.Count);
        return cardDictionary[cardIDList[index]];
    }
}