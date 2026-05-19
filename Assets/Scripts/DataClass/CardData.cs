using System.Collections.Generic;

public enum CardType { Attack, Skill, Power, Status, Curse } // 카드의 타입(공격카드,스킬카드,.파워카드..)
public enum CardEffectType { Damage, Block, Vampire, ApplyBuff, DrawCard, DiscardCard } // 카드의 효과 타입(공격,방어,흡혈,버프(디버프)부여,드로우,버리기)
public enum EffectTarget { Self, Target, AllEnemy } // 효과의 대상 타입
public enum CardTriggerType { OnPlay, OnDiscard } // 카드가 발동하는 조건 (사용시,버려졌을시)
public enum CardConditionType { None, Count_By_Strength, Value_By_Block, Value_By_LostHP } // 카드 효과의 수치 타입 (힘에비례,방어도에비례,잃은체력에 비례)
public enum BuffType { None, Strength, Vulnerable, Weak, Poison, Artifact, Dexterity, Frail } // 부여되는 버프의 타입(힘,취약,약화,독)

[System.Serializable]
public class CardEffect                  // 카드의 한가지 효과를 표현한 데이터
{
    public string effectType;          // 카드의 효과 타입(CardEffectType)
    public string triggerType;         // 카드가 발동하는 조건 (CardTriggerType)
    public int value;                     // 효과의 값
    public int executeCount;          // 효과 실행 횟수
    public int upgradeBonus;         // 카드를 강화했을때 추가되는 수치
    public int upgradeCountBonus; // 카드를 강화했을때 늘어나는 발동횟수
    public int strengthMultiplier;    // 카드의 힘배율(공격카드일때)
    public string targetBuffType;    // 버프의 타입(BuffType)
    public string effectTarget;        //효과의 대상 타입(effectTarget)
    public string conditionType;    //카드효과의 수치 타입(CardConditionType)

    // 인게임 로직 연산 시 안전하게 변환하여 쓸 프로퍼티 래퍼
    public CardEffectType GetEffectType() => System.Enum.TryParse(effectType, out CardEffectType res) ? res : CardEffectType.Damage;
    public CardTriggerType GetTriggerType() => System.Enum.TryParse(triggerType, out CardTriggerType res) ? res : CardTriggerType.OnPlay;
    public BuffType GetBuffType() => System.Enum.TryParse(targetBuffType, out BuffType res) ? res : BuffType.None;
    public EffectTarget GetTarget() => System.Enum.TryParse(effectTarget, out EffectTarget res) ? res : EffectTarget.Target;
    public CardConditionType GetConditionType() => System.Enum.TryParse(conditionType, out CardConditionType res) ? res : CardConditionType.None;
}

[System.Serializable]
public class CardData // 전체 카드 데이터
{
    public int cardID;            
    public string cardName;  
    public string cardType;      // 카드의 타입(CardType)
    public int cost;                // 사용하는데 필요한 에너지
    public string description;   // 카드설명 - 추후에 없앨수도있음
    public string iconPath;
    public int upgradeCostBonus; // 강화시 코스트 변동값 

    public bool isExhaust;      // 소멸카드인가?
    public bool isEthereal;     // 휘발카드인가?

    public List<CardEffect> cardEffects = new List<CardEffect>();

    // 외부 규칙 엔진용 프로퍼티 래퍼
    public CardType GetCardType() => System.Enum.TryParse(cardType, out CardType res) ? res : CardType.Attack;
}

[System.Serializable]
public class CardListWrapper
{
    public List<CardData> cards;
}
