using System.Collections;
using UnityEngine;

// 1. 효과 인터페이스
public interface ICardEffect
{
    IEnumerator Execute();
}

// 2. 메서드를 효과로 감싸는 래퍼 클래스 (이게 핵심입니다!)
public class TriggerEffectWrapper : ICardEffect
{
    private RuntimeCard _card;
    private CardTriggerType _triggerType;
    private GameObject _targetMonster;

    public TriggerEffectWrapper(RuntimeCard card, CardTriggerType triggerType, GameObject targetMonster = null)
    {
        _card = card;
        _triggerType = triggerType;
        _targetMonster = targetMonster;
    }

    public IEnumerator Execute()
    {
        yield return BattleManager.Instance.ExecuteCardTriggerEffects(_card, _triggerType, _targetMonster);
        CardManager.Instance.AddCardToDiscard(_card);
    }
}