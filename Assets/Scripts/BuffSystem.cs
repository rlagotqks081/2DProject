using UnityEngine;
using System.Collections.Generic;

public class BuffSystem : MonoBehaviour
{

    private Dictionary<BuffType, int> currentBuffs = new Dictionary<BuffType, int>();

    public void AddBuff(BuffType type, int value)
    {
        if (currentBuffs.ContainsKey(type))
        {
            currentBuffs[type] += value;
        }
        else
        {
            currentBuffs.Add(type, value);
        }

        if (currentBuffs[type] <= 0) currentBuffs.Remove(type);
    }

    public bool HasBuff(BuffType type)
    {
        return currentBuffs.ContainsKey(type) && currentBuffs[type] > 0;
    }

    public int GetBuffValue(BuffType type)
    {
        if(currentBuffs.TryGetValue(type, out int value))
        {
            return value;
        }
        return 0;
    }

    public void RemoveBuff(BuffType type)
    {
        if (currentBuffs.ContainsKey(type)) currentBuffs.Remove(type);
    }
    public void TickTurnBuffs()
    {
        if (currentBuffs.ContainsKey(BuffType.Vulnerable)) AddBuff(BuffType.Vulnerable, -1);
        if (currentBuffs.ContainsKey(BuffType.Weak)) AddBuff(BuffType.Weak, -1);
        if (currentBuffs.ContainsKey(BuffType.Poison)) AddBuff(BuffType.Poison, -1);
    }
}
