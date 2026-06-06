using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject buffIconPrefab;


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

    public GameObject SpawnRandomMonster()
    {
        MonsterData data = MonsterDatabase.Instance.GetRandomMonster();
        if (data != null)
        {
            GameObject MonsterObj = Instantiate(monsterPrefab);
            Monster monster = MonsterObj.GetComponent<Monster>();
            monster.SetupMonster(data);
            BuffManager.Instance.AddBuffObj(MonsterObj);
            BattleManager.Instance.AddActiveMonsterDic(monster);
            Debug.Log($"[SpawnManager] SpawnMonster {data.monsterName} 로드 되었습니다.");
            return MonsterObj;
        }
        Debug.LogError("[SpawnManager] SpawnRandomMonster 해당 ID의 몬스터데이터가 없습니다");
        return null;
    }

    public GameObject SpawnBuffIcons(BuffSystem buffsystem)
    {
        if (buffsystem == null) return null;
        Transform buffContainer = buffsystem.GetComponentInChildren<BuffContainer>().transform;

        if(buffContainer != null)
        {
            GameObject BuffIcon = Instantiate(buffIconPrefab, buffContainer);
            return BuffIcon;
        }
        Debug.LogError("[SpawnManager] SpawnBuffIcons buffContainer가 존재하지 않습니다");
        return null;
    }
}
