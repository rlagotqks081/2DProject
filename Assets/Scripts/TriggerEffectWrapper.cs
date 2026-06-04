using System.Collections;
using UnityEngine;

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