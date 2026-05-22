using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // 필드에 존재하는 활성화된 몬스터들을 관리하는 리스트
    [SerializeField] public List<Monster> activeMonsters = new List<Monster>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 플레이어가 손패(UI)에서 카드를 선택해 필드에 내려고 할 때 호출되는 함수
    /// </summary>
    public bool PlayerUseCard(CardUI targetCard, GameObject targetMonster = null)
    {
        RuntimeCard runtimeCard = targetCard.TargetRuntimeCard;
        if (runtimeCard == null) return false;
        int requiredCost;

        if (!runtimeCard.CanUse(out string failReason))
        {
            Debug.LogWarning($"[배틀] 카드 사용 실패: {failReason}");
            return false;
        }
        if (CardCalculator.IsSpendingAllCosts(runtimeCard)) requiredCost = Player.Instance.currentEnergy;
        else requiredCost = runtimeCard.GetCalculatedCost();

        Player.Instance.currentEnergy -= requiredCost;

        Debug.Log($"[배틀] {runtimeCard.OriginData.cardName} 사용 성공! 코스트 {requiredCost} 소모.");

        ExecuteCardTriggerEffects(runtimeCard, CardTriggerType.OnPlay, targetMonster);

        if (runtimeCard.OriginData.isExhaust)
        {
            CardManager.Instance.UseCardToExhaust(runtimeCard);
        }
        else
        {
            CardManager.Instance.UseCardToDiscard(runtimeCard);
        }
        return true;
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

    // 만들다가 말았음(최신화 해야함)
    private void StartMonsterTurn()
    {
        GameObject playerObj = Player.Instance.gameObject;

        // 필드에 살아있는 모든 몬스터를 순회하며 예약된 행동 실행
        foreach (Monster monster in activeMonsters)
        {
            if (monster == null) continue;

            MonsterPatternData pattern = monster.GetCurrentIntent();
            if (pattern == null) continue;


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
        // Player.Instance.RestoreEnergyToMax();
        CardManager.Instance.DrawCards(5);
    }



    /// <summary>
    /// 특정 트리거 시점(OnPlay, OnDiscard 등)에 맞춰 카드가 가진 효과를 실행 + 전체적 대상 효과 추가해야함
    /// </summary>
    public void ExecuteCardTriggerEffects(RuntimeCard runtimeCard, CardTriggerType targetTrigger, GameObject targetMonster = null)
    {
        if (runtimeCard == null) return;
        if (targetMonster == null && runtimeCard.GetCardEffectTarget() == EffectTarget.Target) return;
        
        GameObject playerObj = Player.Instance.gameObject;
        foreach (CardEffect effect in runtimeCard.OriginData.cardEffects)
        {
            if (effect.GetTriggerType() != targetTrigger) continue;

            GameObject actualTarget = (effect.GetTarget() == EffectTarget.Self) ? playerObj : targetMonster;
            int finalValue = 0;
            int finalExecuteCount = CardCalculator.GetAttackCount(runtimeCard, effect);

            switch(effect.GetEffectType())
            {
                case CardEffectType.Damage:
                    finalValue = CardCalculator.DamageCalculate(runtimeCard, effect);
                    break;
                case CardEffectType.Block:
                    finalValue = CardCalculator.BlockCalculate(runtimeCard, effect);
                    break;
                case CardEffectType.GetBuff:
                case CardEffectType.ApplyBuff:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
                case CardEffectType.Damage_By_Block:
                    finalValue = CardCalculator.GetBlockValueEffectDamage(runtimeCard, effect, true);
                    break;
                case CardEffectType.DrawCard:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
                case CardEffectType.DiscardCard:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
                case CardEffectType.Damage_Use_AllCost:
                    finalValue = CardCalculator.DamageCalculate(runtimeCard, effect);
                    break;
                case CardEffectType.Block_Use_AllCost:
                    finalValue = CardCalculator.BlockCalculate(runtimeCard, effect);
                    break;
                case CardEffectType.GetCost:
                    finalValue = CardCalculator.GetBaseDamage(runtimeCard, effect);
                    break;
            }


            //최종 계산된 가공 수치와 횟수만큼 인게임 효과 실행
            ApplyEffect(effect.GetEffectType(), actualTarget, finalValue, finalExecuteCount, effect);

        }
    }

    /// <summary>
    /// 최종처리가 끝난 데이터를 기반으로 카드의 효과를 실행
    /// </summary>
    private void ApplyEffect(CardEffectType type, GameObject target, int value, int executeCount, CardEffect effect)
    {
        if (target == null && effect.GetTarget() == EffectTarget.Target) return;


        switch (type)
        {
            case CardEffectType.Damage_Use_AllCost:
            case CardEffectType.Damage_By_Block:
            case CardEffectType.Damage:
                if(target == null)
                {
                    foreach(Monster targets in activeMonsters)
                    {
                        targets.TakeDamage(CardCalculator.VulnerableCalculate(value, BuffManager.Instance.IsObjHasBuff(targets.gameObject, BuffType.Vulnerable)), executeCount);
                    }
                    break;
                }
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(CardCalculator.VulnerableCalculate(value,BuffManager.Instance.IsObjHasBuff(target,BuffType.Vulnerable)), executeCount);
                }
                break;
            case CardEffectType.Block_Use_AllCost:
            case CardEffectType.Block:
                Player.Instance.AddBlock(value, executeCount);
                break;
            case CardEffectType.GetBuff:
            case CardEffectType.ApplyBuff:
                if(target == null)
                {
                    foreach(Monster targets in activeMonsters)
                    {
                        BuffManager.Instance.ApplyBuff(targets.gameObject, effect.GetBuffType(), value, executeCount);
                    }
                    break;
                }
                BuffManager.Instance.ApplyBuff(target, effect.GetBuffType(), value, executeCount);
                break;
            case CardEffectType.DrawCard:
                CardManager.Instance.DrawCards(value, executeCount);
                break;
            case CardEffectType.DiscardCard:
                // 카드 랜덤으로 버리는 로직 짜기 @@
                if(effect.GetTarget() == EffectTarget.Random)
                {
                    // 랜덤카드 버리기 효과 실행
                }
                else
                {
                    InputManager.Instance.StartSelectingMultipleCards(value, (list) =>
                    {
                        if(list != null)
                        {
                            foreach (RuntimeCard card in list)
                            {
                                CardManager.Instance.GetCardUI(card);
                                if (card == null) continue;
                                CardManager.Instance.DiscardFromHand(card);
                            }
                        }
                    });
                }
                    
                break;
            case CardEffectType.GetCost:
                Player.Instance.AddCurEnergy(value, executeCount);
                break;
        }
    }
}