using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

[System.Serializable]
public class MonsterPatternData // 몬스터패턴 넘어갈때 다뜯어고쳐야함
{
    [Tooltip("패턴의 이름 (강타, 저주 부르기)")]
    public string patternName;
    [Tooltip("행동 종류 ( 공격, 방어 버프, 디버프)")]
    public MonsterActionType actionType;
    [Tooltip("공격/방어/버프/디버프의 기본 수치")]
    public int value;
    [Tooltip("연타 횟수( 공격일때만 )")]
    public int attackCount = 1;
    [Tooltip("버프/디버프 부여할 종류")]
    public BuffType targetBuffType;
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
