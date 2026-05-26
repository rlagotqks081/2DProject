using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCardClicked += HandleCardClicked;
        }
    }
    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCardClicked -= HandleCardClicked;
        }
    }

    private void HandleCardClicked(RuntimeCard card)
    {
        switch(InputManager.Instance.currentState)
        {

        }
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
            CardManager.Instance.RemoveCardFromHand(card);
            yield return StartCoroutine(card.EffectRoutine());
            CardManager.Instance.AddCardToDiscard(card);
        }
        HandManager.Instance.AlignCards();
    }

    private IEnumerator FollowMouseRoutine(RuntimeCard card)
    {
        while (true)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            CardManager.Instance.activeCardUIs[card].transform.position = mousePos;
            yield return null;
        }
    }

    private IEnumerator TargetMonsterAndPlay(RuntimeCard card)
    {
        targetTCS = new TaskCompletionSource<Monster>();
        InputManager.Instance.currentState = InputState.SelectingTarget;

        // 타겟 선택 이벤트 연결
        System.Action onRightClick = () => { targetTCS.TrySetCanceled(); };
        InputManager.Instance.OnRightClick += onRightClick;
        targetingArrow.Show(true);

        while (!targetTCS.Task.IsCompleted && !targetTCS.Task.IsCanceled)
        {
            targetingArrow.UpdateArrow(CardManager.Instance.activeCardUIs[card].transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            yield return null;
        }

        targetingArrow.Show(false);

        yield return new WaitUntil(() => targetTCS.Task.IsCompleted || targetTCS.Task.IsCanceled);

        InputManager.Instance.OnRightClick -= onRightClick;
        InputManager.Instance.currentState = InputState.Idle;

        if (!targetTCS.Task.IsCanceled)
        {
            Monster selected = targetTCS.Task.Result;
            Debug.Log(selected.name + "에게 공격 시전!");
            CardManager.Instance.RemoveCardFromHand(card);

            yield return StartCoroutine(card.EffectRoutine());

            CardManager.Instance.AddCardToDiscard(card);
        }
    }

    // InputManager에서 호출될 타겟 선택 함수
    public void SelectTarget(Monster monster)
    {
        if (targetTCS != null && !targetTCS.Task.IsCompleted)
        {
            targetTCS.TrySetResult(monster);
        }
    }

    public void ConfirmSelection()
    {
        if (selectedCards.Count == requiredCount)
        {
            // 완성된 리스트를 반환
            selectionTCS.SetResult(new List<RuntimeCard>(selectedCards));
            selectedCards.Clear();
        }
    }

    public IEnumerator CardRoutine(CardUI targetCard, GameObject targetMonster = null)
    {
        InputManager.Instance.UpdateCurrentState(InputState.Processing);

        // 공격 연출 애니메이션 실행

        



        InputManager.Instance.UpdateCurrentState(InputState.None);
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
                        monster.currentBlock += monsterEffect.value;
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
                        StartCoroutine(HandleDiscardEffect(value));
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
    private IEnumerator HandleDiscardEffect(int value) 
    {

        if (CardManager.Instance.HandPile.Count <= value)
        {
            foreach (var card in CardManager.Instance.HandPile)
                CardManager.Instance.DiscardFromHand(card);
        }
        else
        {
            var tcs = new TaskCompletionSource<List<RuntimeCard>>();

            // InputManager에 필요한 수치만 전달
            InputManager.Instance.StartSelectingMultipleCards(value, (selected) => tcs.SetResult(selected));

            yield return new WaitUntil(() => tcs.Task.IsCompleted);

            foreach (var card in tcs.Task.Result)
            {
                CardManager.Instance.DiscardFromHand(card);
            }
        }
    }
    public IEnumerator RequestCardSelection(int count)
    {
        requiredCount = count;
        selectedCards.Clear();
        selectionTCS = new TaskCompletionSource<List<RuntimeCard>>();
        InputManager.Instance.currentState = InputState.SelectingCard;

        // 선택이 완료될 때까지 대기
        yield return new WaitUntil(() => selectionTCS.Task.IsCompleted);

        InputManager.Instance.currentState = InputState.Processing;
    }
}