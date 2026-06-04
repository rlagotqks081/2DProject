using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class BuffSystem : MonoBehaviour
{

    [SerializeField] private Dictionary<BuffType, int> currentBuffs = new Dictionary<BuffType, int>();

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

    public void ClearBuffs()
    {
        currentBuffs.Clear();
    }

    public void RemoveBuff(BuffType type)
    {
        if (currentBuffs.ContainsKey(type)) currentBuffs.Remove(type);
    }
    public void TickTurnBuffs()
    {
        Debug.Log($"byffststem - {this.gameObject}");
        foreach(BuffType type in currentBuffs.Keys.ToList())
        {
            switch(type)
            {
                case BuffType.Vulnerable:
                case BuffType.Weak:
                case BuffType.Poison:
                case BuffType.Frail:
                    AddBuff(type, -1);
                    break;
            }
        }
    }
}
