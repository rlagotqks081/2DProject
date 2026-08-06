using FantasyBattlegroundsPixelArtOriginal;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    [SerializeField] public int turnCount = 0;
    // 필드에 존재하는 활성화된 몬스터들을 관리하는 리스트
    [SerializeField] public List<Monster> activeMonsters = new List<Monster>();

    private List<RuntimeCard> selectedCards = new List<RuntimeCard>();
    private int requiredCount;
    private TaskCompletionSource<List<RuntimeCard>> selectionTCS;
    private TaskCompletionSource<bool> isSelectedTCS;
    private TaskCompletionSource<Monster> targetTCS;
    public LineRenderer targetingLine;
    [SerializeField] TargetingArrow targetingArrow;


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

    private void Start()
    {
        if (BattleFlowManager.Instance != null)
        {
            BattleFlowManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    public void HandleGameOver(GameOverType result)
    {
        BuffManager.Instance.ClearTargetBuffs(Player.Instance.gameObject);
        activeMonsters.Clear();
        turnCount = 0;
    }



    public IEnumerator PlayCardRoutine(RuntimeCard card)
    {
        // 1. 타입에 따른 분기
        if (card.GetCardEffectTarget() == EffectTarget.Target)
        {
            yield return StartCoroutine(TargetMonsterAndPlay(card));
        }
        else
        {
            yield return StartCoroutine(FollowMouseAndPlay(card));
        }
    }

    private IEnumerator FollowMouseAndPlay(RuntimeCard card)
    {
        isSelectedTCS = new TaskCompletionSource<bool>();
        InputManager.Instance.UpdateCurrentState(InputState.SelectedSkillCard);

        // 마우스 추적 코루틴 별도 실행
        Coroutine follow = StartCoroutine(FollowMouseRoutine(card));

        // 클릭 이벤트 연결
        System.Action onLeftClick = () => { isSelectedTCS.TrySetResult(true); };
        System.Action onRightClick = () => { isSelectedTCS.TrySetResult(false); };

        InputManager.Instance.OnLeftClick += onLeftClick;
        InputManager.Instance.OnRightClick += onRightClick;

        yield return new WaitUntil(() => isSelectedTCS.Task.IsCompleted);

        // 정리
        InputManager.Instance.OnLeftClick -= onLeftClick;
        InputManager.Instance.OnRightClick -= onRightClick;
        StopCoroutine(follow);


        if (isSelectedTCS.Task.Result)
        {
            yield return StartCoroutine(PlayerUseCard(card));
        }
        else
        {
            InputManager.Instance.CancelSelection();
            InputManager.Instance.UpdateCurrentState(InputState.Idle);
        }
    }

    private IEnumerator FollowMouseRoutine(RuntimeCard card)
    {
        CardUIEffect selectedCard = CardManager.Instance.activeCardUIs[card].GetComponent<CardUIEffect>();
        while (true)
        {
            Vector3 mousePos = Input.mousePosition;
            selectedCard.MoveCardPosition(mousePos);
            yield return null;
        }
    }
    private IEnumerator DrawArrowRoutine(RuntimeCard card)
    {
        CardUI targetCardUI = CardManager.Instance.GetCardUI(card);
        while (true)
        {
            if (targetCardUI == null) break;

            Vector2 startPos = targetCardUI.transform.position;
            Vector2 mousePos = Input.mousePosition;
            targetingArrow.UpdateCurve(startPos, mousePos);

            yield return null;
        }
    }
    private IEnumerator TargetMonsterAndPlay(RuntimeCard card)
    {
        targetTCS = new TaskCompletionSource<Monster>();
        InputManager.Instance.UpdateCurrentState(InputState.SelectingTarget);

        // 타겟 선택 이벤트 연결
        System.Action onRightClick = () => { targetTCS.TrySetCanceled(); };
        InputManager.Instance.OnRightClick += onRightClick;
        targetingArrow.Show(true);
        Coroutine arrow = StartCoroutine(DrawArrowRoutine(card));

        yield return new WaitUntil(() => targetTCS.Task.IsCompleted || targetTCS.Task.IsCanceled);

        targetingArrow.Show(false);
        StopCoroutine(arrow);
        InputManager.Instance.OnRightClick -= onRightClick;

        if (!targetTCS.Task.IsCanceled && targetTCS.Task.Result != null)
        {
            Monster selected = targetTCS.Task.Result;
            Debug.Log(selected.name + "에게 공격 시전!");

            yield return StartCoroutine(PlayerUseCard(card,selected.gameObject));

        }
        else
        {
            InputManager.Instance.UpdateCurrentState(InputState.Idle);
        }
            InputManager.Instance.CancelSelection();
    }


    // InputManager에서 호출될 타겟 선택 함수
    public void SelectTarget(Monster monster)
    {
        if (targetTCS != null && !targetTCS.Task.IsCompleted)
        {
            targetTCS.TrySetResult(monster);
        }
    }



    /// <summary>
    /// 플레이어가 손패(UI)에서 카드를 선택해 필드에 내려고 할 때 호출되는 함수
    /// </summary>
    public IEnumerator PlayerUseCard(RuntimeCard runtimeCard, GameObject targetMonster = null)
    {
        if (runtimeCard == null) yield break;
        int requiredCost;

        if (!runtimeCard.CanUse(out string failReason))
        {
            Debug.LogWarning($"[배틀] 카드 사용 실패: {failReason}");
            InputManager.Instance.UpdateCurrentState(InputState.Idle);
            InputManager.Instance.CancelSelection();
            yield break;
        }
        if (CardCalculator.IsSpendingAllCosts(runtimeCard)) requiredCost = Player.Instance.currentEnergy;
        else requiredCost = runtimeCard.GetCalculatedCost();



        yield return StartCoroutine(CardManager.Instance.RemoveCardFromHand(runtimeCard));
        HandManager.Instance.AlignCards();

        InputManager.Instance.UpdateCurrentState(InputState.Processing);

        Debug.Log($"[배틀] {runtimeCard.OriginData.cardName} 사용 성공! 코스트 {requiredCost} 소모.");

        
        EffectManager.Instance.AddEffect(new TriggerEffectWrapper(runtimeCard, CardTriggerType.OnPlay, targetMonster));

        Player.Instance.SpendEnergy(requiredCost);

    }



    public IEnumerator StartMonsterTurn()
    {
        GameObject playerObj = Player.Instance.gameObject;
        
        // 필드에 살아있는 모든 몬스터를 순회하며 예약된 행동 실행
        foreach (Monster monster in activeMonsters)
        {
            if (monster == null) continue;
            monster.UpdateCurStat();
            MonsterPatternData pattern = monster.GetCurrentIntent();
            if (pattern == null) continue;

            foreach(MonsterEffect monsterEffect in pattern.effects)
            {
                switch (monsterEffect.effectType)
                {
                    case MonsterActionType.Attack:
                        int count = monsterEffect.executeCount <= 0 ? 1 : monsterEffect.executeCount;
                        for (int i = 0; i < count; i++)
                        {
                            int monsterDamage = UtilManager.CalculateFinalDamage(monsterEffect.value, monster.gameObject, playerObj);
                            Player.Instance.TakeDamage(monsterDamage);
                        }
                        break;

                    case MonsterActionType.Defend:
                        int monsterBlock = UtilManager.CalculateFinalBlock(monsterEffect.value,monster.gameObject);
                        monster.AddBlock(monsterBlock);
                        break;

                    case MonsterActionType.Buff:
                    case MonsterActionType.Debuff:
                        GameObject buffTarget = (monsterEffect.effectType == MonsterActionType.Buff) ? monster.gameObject : playerObj;
                        BuffManager.Instance.ApplyBuff(buffTarget, monsterEffect.buffType, monsterEffect.value);
                        break;
                }
            }
            monster.AdvancePattern();
        }
        yield break;
    }

    public void StartPlayerTurn()
    {
        Player.Instance.OnStartTurn();
        CardManager.Instance.DrawCards(5);
        UIManager.Instance.UpdatePlayerEnergyText();
    }



    /// <summary>
    /// 특정 트리거 시점(OnPlay, OnDiscard 등)에 맞춰 카드가 가진 효과를 실행 + 전체적 대상 효과 추가해야함
    /// </summary>
    public IEnumerator ExecuteCardTriggerEffects(RuntimeCard runtimeCard, CardTriggerType targetTrigger, GameObject targetMonster = null)
    {
        if (runtimeCard == null) yield break;
        if (targetMonster == null && runtimeCard.GetCardEffectTarget() == EffectTarget.Target) yield break;
        
        GameObject playerObj = Player.Instance.gameObject;
        foreach (CardEffect effect in runtimeCard.OriginData.cardEffects)
        {
            if (effect.GetTriggerType() != targetTrigger) continue;
            if (BattleFlowManager.Instance.IsGameOver) yield break;
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
            yield return StartCoroutine(ApplyEffect(effect.GetEffectType(), actualTarget, finalValue, finalExecuteCount, effect));

        }
    }

    /// <summary>
    /// 최종처리가 끝난 데이터를 기반으로 카드의 효과를 실행
    /// </summary>
    private IEnumerator ApplyEffect(CardEffectType type, GameObject target, int value, int executeCount, CardEffect effect)
    {
        if (target == null && effect.GetTarget() == EffectTarget.Target) yield break;
        if (BattleFlowManager.Instance.IsGameOver) yield break;

        switch (type)
        {
            case CardEffectType.Damage_Use_AllCost:
            case CardEffectType.Damage_By_Block:
            case CardEffectType.Damage:
                if(target == null)
                {
                    for(int i = activeMonsters.Count - 1; i >= 0; i--)
                    {
                        activeMonsters[i].TakeDamage(CardCalculator.VulnerableCalculate(value, BuffManager.Instance.IsObjHasBuff(activeMonsters[i].gameObject, BuffType.Vulnerable)), executeCount);
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
                int handCount = CardManager.Instance.HandPile.Count;
                if(handCount <= value)  // 버릴카드가 충분하지않으면 전체 카드 버리기 후 종료
                {
                    CardManager.Instance.DiscardAllCards();
                }
                else
                {
                    if (effect.GetTarget() == EffectTarget.Random)
                    {
                        // 랜덤카드 버리기 효과 실행
                    }
                    else
                    {
                        yield return StartCoroutine(HandleDiscardEffect(value));
                    }
                }
                break;
            case CardEffectType.GetCost:
                Player.Instance.AddCurEnergy(value, executeCount);
                break;
        }
    }
    public void AddTurnCount()
    {
        turnCount++;
    }
    public void ResetTurnCount()
    {
        turnCount = 0;
    }
    private IEnumerator HandleDiscardEffect(int value, bool isMandatory = true) 
    {
        var tcs = new TaskCompletionSource<List<RuntimeCard>>();

        // InputManager에 필요한 수치만 전달
        InputManager.Instance.StartSelectingMultipleCards(value, (selected) => tcs.SetResult(selected), isMandatory);

        yield return new WaitUntil(() => tcs.Task.IsCompleted);
        foreach(var card in tcs.Task.Result)
        {
            yield return StartCoroutine(CardManager.Instance.RemoveCardFromHand(card));
        }

        foreach (var card in tcs.Task.Result)
        {
            yield return StartCoroutine(DiscardProcess(card));
        }
        
    }

    public IEnumerator DiscardProcess(RuntimeCard card)
    {
        EffectManager.Instance.AddEffect(new TriggerEffectWrapper(card, CardTriggerType.OnDiscard));


        yield break;
    }

    public void AddActiveMonsterDic(Monster monster)
    {
        if (monster == null) return;
        if (activeMonsters.Contains(monster)) return;
        activeMonsters.Add(monster);
    }
}