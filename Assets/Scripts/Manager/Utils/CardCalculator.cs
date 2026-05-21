using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public static class CardCalculator
{

    /// <summary>
    /// 카드의 Damage 효과 하나의 기본데미지 + 강화 보너스 계산
    /// </summary>
    public static int GetBaseDamage(RuntimeCard card, CardEffect effect)
    {
        return effect.value + (card.UpgradeCount * effect.upgradeBonus);
    }

    /// <summary>
    /// 카드의 데미지 + 강화 + 힘버프 / 약화버프 계산
    /// </summary>
    public static int DamageCalculate(RuntimeCard card, CardEffect effect)
    {
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);


        // 플레이어의 힘 적용
        value += Player.Instance._buffSystem.GetBuffValue(BuffType.Strength);

        // 플레이어의 약화 적용
        if (Player.Instance._buffSystem.HasBuff(BuffType.Weak))
        {
            value = Mathf.FloorToInt(value * 0.75f);
        }

        return Mathf.Max(0, value);
    }

    /// <summary>
    /// 타겟몬스터가 있다면 타겟의 취약까지 모두 계산해서 데미지값을 출력
    /// </summary>
    /// <param name="card"></param>
    /// <param name="effect"></param>
    /// <param name="monster"></param>
    /// <returns></returns>
    public static int FinalDamageCalculate(RuntimeCard card, CardEffect effect, Monster monster = null)
    {
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);


        // 플레이어의 힘 적용
        value += Player.Instance._buffSystem.GetBuffValue(BuffType.Strength);

        // 플레이어의 약화 적용
        if (Player.Instance._buffSystem.HasBuff(BuffType.Weak))
        {
            value = Mathf.FloorToInt(value * 0.75f);
        }

        if(monster != null && BuffManager.Instance.IsObjHasBuff(monster.gameObject, BuffType.Vulnerable))
        {
            value = Mathf.FloorToInt(value * 1.5f);
        }

        return Mathf.Max(0, value);
    }

    public static int VulnerableCalculate(int damage,  bool IstargetVulnerable)
    {
        if(IstargetVulnerable)
        {
            damage = Mathf.FloorToInt(damage * 1.5f);
        }
        return Mathf.Max(0, damage);
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
    public static int GetBlockValueEffectDamage(RuntimeCard card, CardEffect effect, bool IsInGame = false)
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
        return Mathf.Max(0, value);
    }
    public static int GetFinalBlockValueDamage(RuntimeCard card, CardEffect effect, Monster monster = null)
    {
        if (BattleManager.Instance.activeMonsters.Count == 0) return -1;
        int value = effect.value + (card.UpgradeCount * effect.upgradeBonus);

        value += Player.Instance.block;

        // 플레이어의 힘 적용
        value += Player.Instance._buffSystem.GetBuffValue(BuffType.Strength);

        // 플레이어의 약화 적용
        if (Player.Instance._buffSystem.HasBuff(BuffType.Weak))
        {
            value = Mathf.FloorToInt(value * 0.75f);
        }

        if (monster != null && BuffManager.Instance.IsObjHasBuff(monster.gameObject, BuffType.Vulnerable))
        {
            value = Mathf.FloorToInt(value * 1.5f);
        }

        return Mathf.Max(0, value);
    }
}