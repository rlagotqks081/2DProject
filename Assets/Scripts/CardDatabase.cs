using UnityEngine;
using System.Collections.Generic;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    // 외부에서 접근 가능한 딕셔너리
    private Dictionary<int, CardData> cardDictionary = new Dictionary<int, CardData>();

    private void Awake()
    {
        Instance = this;
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("CardData");

        if (jsonFile == null)
        {
            Debug.LogError("CardData.json 파일을 찾을 수 없습니다!");
            return;
        }

        // JSON 파싱 (Wrapper를 통해 리스트 추출)
        CardListWrapper wrapper = JsonUtility.FromJson<CardListWrapper>(jsonFile.text);

        // 리스트 데이터를 딕셔너리에 매핑
        foreach (var data in wrapper.cards)
        {
            if (!cardDictionary.ContainsKey(data.cardID))
            {
                cardDictionary.Add(data.cardID, data);
            }
        }

        Debug.Log($"[CardDatabase] 총 {cardDictionary.Count}개의 카드가 로드되었습니다.");
    }

    public CardData GetCard(int id)
    {
        return cardDictionary.TryGetValue(id, out CardData data) ? data : null;
    }
}