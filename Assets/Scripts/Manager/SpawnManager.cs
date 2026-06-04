using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject monsterPrefab;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject SpawnMonster(string monsterID)
    {
        MonsterData data = MonsterDatabase.Instance.GetMonsterByID(monsterID);
        if (data != null) 
        {
            GameObject MonsterObj = Instantiate(monsterPrefab);
            Monster monster = MonsterObj.GetComponent<Monster>();
            monster.SetupMonster(data);
            BuffManager.Instance.AddBuffObj(MonsterObj);
            BattleManager.Instance.AddActiveMonsterDic(monster);
            Debug.Log($"[SpawnManager] SpawnMonster {monsterID} 로드 되었습니다.");
            return MonsterObj;
        }
        Debug.LogError("[SpawnManager] SpawnMonster 해당 ID의 몬스터데이터가 없습니다");
        return null;
    }
}
