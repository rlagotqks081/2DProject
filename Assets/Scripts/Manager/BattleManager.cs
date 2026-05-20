using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BattleManager : MonoBehaviour
{
    // --- 싱글톤 설정 ---
    public static BattleManager Instance { get; private set; }

    // 필드에 존재하는 활성화된 몬스터들을 관리하는 리스트
    public List<Monster> activeMonsters = new List<Monster>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 플레이어가 손패(UI)에서 카드를 선택해 필드에 내려고 할 때 호출되는 함수
    /// </summary>
    public void PlayerUseCard(CardUI targetCard, GameObject targetMonster)
    {
        RuntimeCard runtimeCard = targetCard.TargetRuntimeCard;
        if (runtimeCard == null) return;

        if (!runtimeCard.CanUse(out string failReason))
        {
            Debug.LogWarning($"[배틀] 카드 사용 실패: {failReason}");
            return;
        }

        // --- 검사 통과: 에너지 차감 및 효과 집행 ---
        int requiredCost = runtimeCard.GetCalculatedCost();
        Player.Instance.currentEnergy -= requiredCost;

        Debug.Log($"[배틀] {runtimeCard.OriginData.cardName} 사용 성공! 코스트 {requiredCost} 소모.");

        // 규칙 연산기 가동 (OnPlay 트리거 효과 집행)
        ExecuteCardTriggerEffects(runtimeCard, CardTriggerType.OnPlay, targetMonster);

        // 물리적 카드 배송은 CardManager에게 전권 위임 (쌍둥이 카드 버그 완벽 방지)
        if (runtimeCard.OriginData.isExhaust)
        {
            CardManager.Instance.UseCardToExhaust(runtimeCard);
        }
        else
        {
            CardManager.Instance.UseCardToDiscard(runtimeCard);
        }
    }

    /// <summary>
    /// 플레이어가 '턴 종료' 버튼을 눌렀을 때 호출되는 함수
    /// </summary>
    public void EndPlayerTurn()
    {
        Debug.Log("[배틀] 플레이어 턴 종료.");

        // 손에 남은 카드들 싹 버리기
        // CardManager.Instance.DiscardAllHand();

        StartMonsterTurn();
    }


    private void StartMonsterTurn()
    {
        Debug.Log("[배틀] 몬스터 턴 시작!");
        GameObject playerObj = Player.Instance.gameObject;

        // 필드에 살아있는 모든 몬스터를 순회하며 예약된 행동 실행
        foreach (Monster monster in activeMonsters)
        {
            if (monster == null) continue;

            MonsterPatternData pattern = monster.GetCurrentIntent();
            if (pattern == null) continue;

            Debug.Log($"[몬스터 행동] {monster.monsterName}이(가) '{pattern.patternName}' 시전!");

            switch (pattern.actionType)
            {
                case MonsterActionType.Attack:
                    int count = pattern.attackCount <= 0 ? 1 : pattern.attackCount;
                    for (int i = 0; i < count; i++)
                    {
                        int monsterDamage = UtilManager.CalculateFinalDamage(pattern.value, monster.gameObject, playerObj);
                        Player.Instance.TakeDamage(monsterDamage);
                    }
                    break;

                case MonsterActionType.Defend:
                    monster.currentBlock += pattern.value;
                    break;

                case MonsterActionType.Buff:
                case MonsterActionType.Debuff:
                    GameObject buffTarget = (pattern.actionType == MonsterActionType.Buff) ? monster.gameObject : playerObj;
                    BuffManager.Instance.ApplyBuff(buffTarget, pattern.targetBuffType, pattern.value);
                    break;
            }

            monster.AdvancePattern();
        }

        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        Debug.Log("[배틀] 플레이어 턴 복귀. 드로우 및 에너지 충전.");
        // Player.Instance.RestoreEnergyToMax();
        CardManager.Instance.DrawCards(5);
    }



    /// <summary>
    /// 특정 트리거 시점(OnPlay, OnDiscard 등)에 맞춰 카드가 가진 효과를 실행 + 전체적 대상 효과 추가해야함
    /// </summary>
    public void ExecuteCardTriggerEffects(RuntimeCard runtimeCard, CardTriggerType targetTrigger, GameObject targetMonster)
    {
        if (runtimeCard == null) return;
        GameObject playerObj = Player.Instance.gameObject;
        Monster monster = targetMonster.GetComponent<Monster>();
        foreach (CardEffect effect in runtimeCard.OriginData.cardEffects)
        {
            if (effect.GetTriggerType() != targetTrigger) continue;

            GameObject actualTarget = (effect.GetTarget() == EffectTarget.Self) ? playerObj : targetMonster;
            int finalValue = 0;
            int finalExecuteCount = CardCalculator.GetAttackCount(runtimeCard, effect);

            switch(effect.GetEffectType())
            {
                case CardEffectType.Damage:
                    finalValue = CardCalculator.DamageCalculate(runtimeCard, effect, monster);
                    break;
                case CardEffectType.Block:
                    finalValue = CardCalculator.BlockCalculate(runtimeCard, effect);
                    break;
                case CardEffectType.GetBuff:
                case CardEffectType.ApplyBuff:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
                case CardEffectType.Damage_By_Block:
                    finalValue = CardCalculator.GetBlockValueEffectDamage(runtimeCard, effect, monster, true);
                    break;
                case CardEffectType.DrawCard:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
                case CardEffectType.DiscardCard:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
                case CardEffectType.Damage_Use_AllCost:
                    finalValue = CardCalculator.DamageCalculate(runtimeCard, effect, monster);
                    break;
                case CardEffectType.Block_Use_AllCost:
                    finalValue = CardCalculator.BlockCalculate(runtimeCard, effect);
                    break;
            }


            //최종 계산된 가공 수치와 횟수만큼 인게임 효과 실행
            for (int i = 0; i < finalExecuteCount; i++)
            {
                ApplyEffect(effect.GetEffectType(), actualTarget, finalValue, effect);
            }
        }
    }

    /// <summary>
    /// 최종처리가 끝난 데이터를 기반으로 카드의 효과를 실행
    /// </summary>
    private void ApplyEffect(CardEffectType type, GameObject target, int value, CardEffect effect)
    {
        if (target == null) return;
        GameObject playerObj = Player.Instance.gameObject;

        switch (type)
        {
            case CardEffectType.Damage_Use_AllCost:
            case CardEffectType.Damage_By_Block:
            case CardEffectType.Damage:
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(value);
                }
                break;
            case CardEffectType.Block_Use_AllCost:
            case CardEffectType.Block:
                Player.Instance.AddBlock(value);
                break;
            case CardEffectType.GetBuff:
            case CardEffectType.ApplyBuff:
                BuffType buffType = effect.GetBuffType();
                BuffManager.Instance.ApplyBuff(target, buffType, value);
                break;
            case CardEffectType.DrawCard:
                CardManager.Instance.DrawCards(value);
                break;
            case CardEffectType.DiscardCard:
                // 카드 선택해서/랜덤으로 버리는 로직 짜기 @@
                break;
        }
    }
}