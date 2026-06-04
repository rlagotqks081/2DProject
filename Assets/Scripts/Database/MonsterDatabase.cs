using System.Collections.Generic;
using UnityEngine;

public class MonsterDatabase : MonoBehaviour
{
    public static MonsterDatabase Instance;
    private Dictionary<string, MonsterData> MonsterDict = new Dictionary<string, MonsterData>();

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
}
