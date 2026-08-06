using System.Collections;
using UnityEngine;

// 카드애니매이션도 이렇게 wrapper만들어서 큐로 카드효과와 함께 관리하도록 수정하기
// 이게 되야 히트애니메이션도 편하게 넣고 할거같음.

public interface ICardEffect
{
    IEnumerator Execute();
}

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