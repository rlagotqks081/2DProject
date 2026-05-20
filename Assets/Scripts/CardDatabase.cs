using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    // 외부에서 접근 가능한 딕셔너리

    public Dictionary<int, CardData> cardDictionary= new Dictionary<int, CardData>();

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
        }

        Debug.Log($"[CardDatabase] 총 {cardDictionary.Count}개의 카드가 로드되었습니다.");
    }

    public CardData GetCard(int id)
    {
        return cardDictionary.TryGetValue(id, out CardData data) ? data : null;
    }
}