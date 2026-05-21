using UnityEngine;
using System.Collections.Generic;
public static class DescriptionGenerator
{
    public static string Generate(RuntimeCard card, DescriptionType desType, Monster target = null)
    {
        if (card == null) return null;
        string desc = card.GetOriginalDesc();
        var replacements = new Dictionary<string, string>();
        bool isTargeting = (desType == DescriptionType.Targeting);

        foreach (CardEffect effect in card.OriginData.cardEffects)
        {
            switch (effect.GetEffectType())
            {
                case CardEffectType.Damage:
                    replacements["{Damage}"] = CardCalculator.FinalDamageCalculate(card, effect, target).ToString();
                    if (effect.executeCount > 1) replacements["{Count}"] = CardCalculator.GetAttackCount(card, effect).ToString();
                    break;
                case CardEffectType.Block:
                    replacements["{Block}"] = CardCalculator.BlockCalculate(card, effect).ToString();
                    break;
                case CardEffectType.GetBuff:
                case CardEffectType.ApplyBuff:
                    replacements["{BuffValue}"] = (effect.value + (card.UpgradeCount * effect.upgradeBonus)).ToString();
                    break;
                case CardEffectType.Damage_By_Block:
                    int blockDmg = CardCalculator.GetFinalBlockValueDamage(card, effect, target);
                    if (blockDmg == -1)
                    {
                        replacements["(피해를 {BlockValue} 줍니다.)"] = "";
                        break;
                    }
                    replacements["{BlockValue}"] = blockDmg.ToString();
                    break;
                case CardEffectType.DrawCard:
                    replacements["{DrawValue}"] = (effect.value + (card.UpgradeCount * effect.upgradeCountBonus)).ToString();
                    break;
                case CardEffectType.DiscardCard:
                    replacements["{DiscardValue}"] = (effect.value + (card.UpgradeCount * effect.upgradeCountBonus)).ToString();
                    break;
                case CardEffectType.Damage_Use_AllCost:
                    replacements["{Damage}"] = CardCalculator.FinalDamageCalculate(card, effect, target).ToString();
                    replacements["X"] = Player.Instance.currentEnergy.ToString();
                    break;
                case CardEffectType.Block_Use_AllCost:
                    replacements["{All_Value}"] = CardCalculator.BlockCalculate(card, effect).ToString();
                    replacements["X"] = Player.Instance.currentEnergy.ToString();
                    break;
            }
        }


        foreach (var pair in replacements)
        {
            desc = desc.Replace(pair.Key, pair.Value);
        }
        return desc;
    }
}
