using UnityEngine;
using System.Collections.Generic;
public static class DescriptionGenerator
{
    public static string Generate(RuntimeCard card, DescriptionType desType, bool IsApplyBuff, Monster target = null)
    {
        string desc = "";
        var replacements = new Dictionary<string, string>();
        bool isTargeting = (desType == DescriptionType.Targeting);

        foreach (CardEffect effect in card.OriginData.cardEffects)
        {
            desc += DescriptionDatabase.GetTemplates(effect.effectType);
            switch (effect.GetEffectType())
            {
                case CardEffectType.Damage:
                    replacements["{Damage}"] = CardCalculator.DamageCalculate(card, effect, target).ToString();
                    break;
                case CardEffectType.Block:
                    replacements["{Block}"] = CardCalculator.BlockCalculate(card, effect).ToString();
                    break;
                case CardEffectType.GetBuff:
                case CardEffectType.ApplyBuff:
                    replacements["{BuffType}"] = DescriptionDatabase.GetBuffDesc(effect.effectType);
                    replacements["{BuffValue}"] = (effect.value + (card.UpgradeCount * effect.upgradeBonus)).ToString();
                    break;
                case CardEffectType.Damage_By_Block:
                    int blockDmg = CardCalculator.GetBlockValueEffectDamage(card, effect, target,IsApplyBuff);
                    if (blockDmg == -1)
                    {
                        replacements["{(피해를 {BlockValue} 줍니다.)}"] = "";
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
                    replacements["{All_Value}"] = CardCalculator.DamageCalculate(card, effect, target).ToString();
                    break;
                case CardEffectType.Block_Use_AllCost:
                    replacements["{All_Value}"] = CardCalculator.BlockCalculate(card, effect).ToString();
                    break;
            }
        }

        string result = desc;
        foreach (var pair in replacements)
        {
            result = result.Replace(pair.Key, pair.Value);
        }
        return result;
    }
}
