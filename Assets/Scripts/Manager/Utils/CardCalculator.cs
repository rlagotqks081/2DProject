using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class CardCalculator
{

    /// <summary>
    /// 카드의 Damage 효과 하나의 기본데미지 + 강화 보너스 계산
    /// </summary>
    public static int GetBaseDamage(RuntimeCard card, CardEffect effect)
    {
        return effect.value + (card.UpgradeCount * effect.upgradeBonus);
    }

    public static int DamageCalculate(RuntimeCard card, CardEffect effect, Monster target)
    {
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);


        // 플레이어의 힘 적용
        value += Player.Instance._buffSystem.GetBuffValue(BuffType.Strength);

        // 플레이어의 약화 적용
        if (Player.Instance._buffSystem.HasBuff(BuffType.Weak))
        {
            value = Mathf.FloorToInt(value * 0.75f);
        }

        // 타겟이 있고 취약 상태면 증폭
        if (target != null && target._buffSystem.GetBuffValue(BuffType.Vulnerable) > 0)
        {
            value = Mathf.FloorToInt(value * 1.5f);
        }

        return Mathf.Max(0, value);
    }

    public static int BlockCalculate(RuntimeCard card, CardEffect effect)
    {
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);


        value += Player.Instance._buffSystem.GetBuffValue(BuffType.Dexterity);
        if (Player.Instance._buffSystem.HasBuff(BuffType.Frail))
            value = Mathf.FloorToInt(value * 0.75f);

        return Mathf.Max(0, value);
    }

    public static int BuffCalculate(RuntimeCard card, CardEffect effect)
    {
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);
        return value;
    }





    /// <summary>
    /// 카드의 Damage 효과 하나의 강화/플레이어 버프 계산
    /// </summary>
    public static int ApplyPlayerBuffs(RuntimeCard card, CardEffect effect)
    {
        int baseDmg = effect.value + (card.UpgradeCount * effect.upgradeBonus);

        baseDmg += Player.Instance._buffSystem.GetBuffValue(BuffType.Strength);

        if (Player.Instance._buffSystem.HasBuff(BuffType.Weak))
            baseDmg = Mathf.FloorToInt(baseDmg * 0.75f);

        return Mathf.Max(0, baseDmg); 
    }

    /// <summary>
    /// 카드의 Damage 효과 하나의 강화/버프/적버프 계산
    /// </summary>
    public static int ApplyMonsterDebuffs(RuntimeCard card, CardEffect effect, Monster target)
    {
        int baseDmg = ApplyPlayerBuffs(card, effect);
        if (target == null) return baseDmg;

        if (target._buffSystem.HasBuff(BuffType.Vulnerable))
            baseDmg = Mathf.FloorToInt(baseDmg * 1.5f);
        return Mathf.Max(0, baseDmg);

    }

    /// <summary>
    /// 카드의 effect 하나의 실행횟수의 강화 계산
    /// </summary>
    public static int GetAttackCount(RuntimeCard card, CardEffect effect)
    {
        if(effect.GetEffectType() == CardEffectType.Block_Use_AllCost || effect.GetEffectType() == CardEffectType.Damage_Use_AllCost)
        {
            return Player.Instance.currentEnergy + (card.UpgradeCount * effect.upgradeCountBonus);
        }
        int baseCount = effect.executeCount + (card.UpgradeCount * effect.upgradeCountBonus);
        return baseCount;
    }

    /// <summary>
    /// 방어력 비례 피해를 입히는 효과의 데미지 계산(인게임/타겟팅시엔 공격값, 이외에는 -1 리턴)
    /// </summary>
    public static int GetBlockValueEffectDamage(RuntimeCard card, CardEffect effect, Monster target, bool IsInGame = false)
    {
        if (!IsInGame) return -1;
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);

        value += Player.Instance.block;

        // 플레이어의 힘 적용
        value += Player.Instance._buffSystem.GetBuffValue(BuffType.Strength);

        // 플레이어의 약화 적용
        if (Player.Instance._buffSystem.HasBuff(BuffType.Weak))
        {
            value = Mathf.FloorToInt(value * 0.75f);
        }

        // 타겟이 있고 취약 상태면 증폭
        if (target != null && target._buffSystem.GetBuffValue(BuffType.Vulnerable) > 0)
        {
            value = Mathf.FloorToInt(value * 1.5f);
        }

        return Mathf.Max(0, value);

    }
}