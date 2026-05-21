using System.Collections.Generic;

public enum CardType { Attack, Skill, Power, Status, Curse } // 카드의 타입(공격카드,스킬카드,.파워카드..)
public enum CardEffectType { Damage, Block, GetBuff, ApplyBuff, DrawCard, DiscardCard, Damage_By_Block, Damage_Use_AllCost, Block_Use_AllCost } // 카드의 효과 타입(공격,방어,흡혈,버프(디버프)부여,드로우,버리기)
public enum EffectTarget { Self, Target, AllEnemy } // 효과의 대상 타입
public enum CardTriggerType { OnPlay, OnDiscard } // 카드가 발동하는 조건 (사용시,버려졌을시)
public enum BuffType { None, Strength, Vulnerable, Weak, Poison, Artifact, Dexterity, Frail } // 부여되는 버프의 타입(힘,취약,약화,독)

[System.Serializable]
public class CardEffect                  // 카드의 한가지 효과를 표현한 데이터
{
    public string effectType { get; set; }      // 카드의 효과 타입(CardEffectType)
    public string triggerType { get; set; }         // 카드가 발동하는 조건 (CardTriggerType)
    public int value { get; set; }                     // 효과의 값
    public int executeCount { get; set; }          // 효과 실행 횟수
    public int upgradeBonus { get; set; }         // 카드를 강화했을때 추가되는 수치
    public int upgradeCountBonus { get; set; } // 카드를 강화했을때 늘어나는 발동횟수
    public string targetBuffType { get; set; }    // 버프의 타입(BuffType)
    public string effectTarget { get; set; }        //효과의 대상 타입(effectTarget)

    // 인게임 로직 연산 시 안전하게 변환하여 쓸 프로퍼티 래퍼
    public CardEffectType GetEffectType() => System.Enum.TryParse(effectType, out CardEffectType res) ? res : CardEffectType.Damage;
    public CardTriggerType GetTriggerType() => System.Enum.TryParse(triggerType, out CardTriggerType res) ? res : CardTriggerType.OnPlay;
    public BuffType GetBuffType() => System.Enum.TryParse(targetBuffType, out BuffType res) ? res : BuffType.None;
    public EffectTarget GetTarget() => System.Enum.TryParse(effectTarget, out EffectTarget res) ? res : EffectTarget.Target;
    
}

[System.Serializable]
public class CardData // 전체 카드 데이터
{
    public int cardID { get; set; }
    public string cardName { get; set; }
    public string cardType { get; set; }      // 카드의 타입(CardType)
    public int cost { get; set; }                // 사용하는데 필요한 에너지
    public string description { get; set; } // 카드의 설명
    public string iconPath { get; set; }
    public int upgradeCostBonus { get; set; } // 강화시 코스트 변동값 

    public bool isExhaust { get; set; }      // 소멸카드인가?
    public bool isEthereal { get; set; }    // 휘발카드인가?

    public List<CardEffect> cardEffects { get; set; } = new List<CardEffect>();

    // 외부 규칙 엔진용 프로퍼티 래퍼
    public CardType GetCardType() => System.Enum.TryParse(cardType, out CardType res) ? res : CardType.Attack;
}

[System.Serializable]
public class CardListWrapper
{
    public List<CardData> cards { get; set; } = new List<CardData>();
}
