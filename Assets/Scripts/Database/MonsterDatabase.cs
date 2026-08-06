using System.Collections.Generic;
using UnityEngine;

public class MonsterDatabase : MonoBehaviour
{
    public static MonsterDatabase Instance;
    private Dictionary<string, MonsterData> MonsterDict = new Dictionary<string, MonsterData>();
    private List<string> MonsterID = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllMonsters();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllMonsters()
    {
        MonsterData[] loadedMonsters = Resources.LoadAll<MonsterData>("MonsterData");

        foreach(MonsterData monster in loadedMonsters)
        {
            if(!MonsterDict.ContainsKey(monster.monsterKey))
            {
                MonsterDict.Add(monster.monsterKey, monster);
                MonsterID.Add(monster.monsterKey);
            }// 치장아이템 데이터, 사용할지는 모르겠음
            [CreateAssetMenu(fileName = "NewCosmeticItem", menuName = "ScriptableObject/Cosmetic")]
            public class CosmeticItem : ItemBase
    {

        // 치장 관련 변수 적기
    }
}
        Debug.Log($"[MonsterDatabase] DB 로드 완료, 총 {MonsterDict.Count} 마리 ");
    }

    public MonsterData GetMonsterByID(string monsterKey)
    {
        if(MonsterDict.ContainsKey(monsterKey))
        {
            return MonsterDict[monsterKey];
        }
        return null;
    }
    public MonsterData GetRandomMonster()
    {
        int randomIndex = Random.Range(0, MonsterID.Count);
        return MonsterDict[MonsterID[randomIndex]];
    }
}
