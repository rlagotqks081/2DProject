using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

[System.Serializable]
public class MonsterEffect
{
    [Tooltip("효과 종류")]
    public MonsterActionType effectType;
    [Tooltip("수치 (데미지량, 회복량, 버프량 등)")]
    public int value;
    [Tooltip("버프/디버프 종류")]
    public BuffType buffType;
    [Tooltip("실행 횟수")]
    public int executeCount;
}

[System.Serializable]
public class MonsterPatternData // 몬스터패턴 넘어갈때 다뜯어고쳐야함
{
    [Tooltip("한 번에 실행될 효과들의 리스트")]
    public List<MonsterEffect> effects = new List<MonsterEffect>();
}

public enum MonsterActionType
{
    Attack,
    Defend,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("몬스터 기본 정보")]
    public string monsterKey;
    public string monsterName;
    public int maxHp;

    [Header("몬스터 패턴")]
    [Tooltip("몬스터가 순서대로 실행할 행동 리스트")]
    public List<MonsterPatternData> patterns = new List<MonsterPatternData>();
}
