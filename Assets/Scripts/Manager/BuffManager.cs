using UnityEngine;
using System.Collections.Generic;


public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    private Dictionary<GameObject, BuffSystem> activeBuffs = new Dictionary<GameObject, BuffSystem>();

    private void Awake()
    {
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
    }

    public void TurnEndChangeBuffs()
    {
        foreach(BuffSystem buffSystem in activeBuffs.Values)
        {
            if (buffSystem.HasBuff(BuffType.Poison)) buffSystem.GetComponent<IDamageable>().TakeDirectDamage(buffSystem.GetBuffValue(BuffType.Poison));
            buffSystem.TickTurnBuffs();
        }
    }
    public void ApplyBuff(GameObject target, BuffType type, int value, int count = 1)
    {
        if (target == null) return;
        if (!activeBuffs.ContainsKey(target))
        {
            activeBuffs.Add(target, target.GetComponent<BuffSystem>());
        }
        for (int i = 0; i < count; i++)
        {
            activeBuffs[target].AddBuff(type, value);
        }
    }

    public void RemoveBuff(GameObject target, BuffType type)
    {
        if (target == null) return; 
        if (!activeBuffs.ContainsKey(target))
        {
            activeBuffs.Add(target, target.GetComponent<BuffSystem>());
        }
        activeBuffs[target].RemoveBuff(type);

    }
    public bool IsObjHasBuff(GameObject target, BuffType type)
    {
        if(activeBuffs.ContainsKey(target))
        {
            return activeBuffs[target].HasBuff(type);
        }
        return false;
    }

    public void AddBuffObj(GameObject target)
    {
        if (target == null) return;
        if (activeBuffs.ContainsKey(target)) return;
        BuffSystem buffSystem = target.GetComponent<BuffSystem>();
        if (buffSystem == null) return;

        activeBuffs.Add(target, buffSystem);
    }
    public void RemoveBuffObj(GameObject target)
    {
        if (target == null) return;
        if(activeBuffs.ContainsKey(target))
        {
            activeBuffs.Remove(target);
        }
    }
    public void ClearTargetBuffs(GameObject target)
    {
        if (target == null) return;
        if(activeBuffs.ContainsKey(target))
        {
            activeBuffs[target].ClearBuffs();
        }
    }
}
