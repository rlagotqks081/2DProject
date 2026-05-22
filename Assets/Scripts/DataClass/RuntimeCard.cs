using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public enum DescriptionType { Default, InHand, Targeting }
public class RuntimeCard   // 게임 플레이중 동적 생성되는 원본(CardDatra)의 복사본 카드
{
    // 원본CardData의 주소값 (수정 X)
    public CardData OriginData { get; private set; }

    public int UpgradeCount { get; private set; } //해당 카드의 강화 횟수

    private int temporaryCostModifier; // 전투도중 일시적인 코스트 변동수치

    public RuntimeCard(CardData originData)
    {
        OriginData = originData;    // 주소값을 넘겨준거라 절대로 수정하면 안댐
        UpgradeCount = 0; // 태어날 때는 기본 0강
        temporaryCostModifier = 0;
    }

    public bool UpgradeCard()
    {
        if (UpgradeCount >= 1) return false;
        UpgradeCount++;
        return true;
    }


    public void ModifyTemporaryCost(int amount)
    {
        temporaryCostModifier += amount;
    }

    public string GetDescription(DescriptionType desType, Monster target = null)
    {
        return DescriptionGenerator.Generate(this, desType, target);
    }
    public string GetOriginalDesc()
    {
        return OriginData.description;
    }

    // 강화시 증가하는 횟수를 반영한 최종 발동횟수 계산
    public int GetCalculatedCount(CardEffect effect)
    {
        int baseCount = effect.executeCount <= 0 ? 1 : effect.executeCount;
        return baseCount + (effect.upgradeCountBonus * UpgradeCount);
    }

    // 강화시 / 전투중 일시적인 코스트 변동을 반영한 최종 코스트 계산 - @@나중에 보완할 필요가 있어보임@@
    public int GetCalculatedCost()
    {
        int finalCost = OriginData.cost - (OriginData.upgradeCostBonus * UpgradeCount);
        finalCost -= temporaryCostModifier;

        if (finalCost <= 0) finalCost = 0;
        return finalCost;
    }

    /// <summary>
    /// 인게임에서 카드 선택시 카드의 타입을 알기위한 함수 
    /// </summary>
    /// <returns>타겟한명을 짚어야하면 Target, 아니면 Self리턴</returns>
    public EffectTarget GetCardEffectTarget()
    {
        foreach(CardEffect effect in OriginData.cardEffects)
        {
            if(effect.GetTarget() == EffectTarget.Target)
            {
                return EffectTarget.Target;
            }
        }
        return EffectTarget.Self;
    }

    

    public bool CanUse(out string errorMessage)
    {
        errorMessage = "";

        bool hasOnPlayEffect = false;
        foreach(var effect in OriginData.cardEffects)
        {
            if(effect.GetTriggerType() == CardTriggerType.OnPlay)
            {
                hasOnPlayEffect = true;
                break;
            }
        }

        if(!hasOnPlayEffect)
        {
            errorMessage = "직접사용이 불가능한 카드입니다(OnPlay아님)";
            return false;
        }

        if(Player.Instance.currentEnergy < GetCalculatedCost())
        {
            errorMessage = "에너지가 부족합니다.";
            return false;
        }
        return true;
    }
}