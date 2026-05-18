using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public enum BuffType
{
    Vulnerable,
    Weak,
    Poison,
    Strength,
    Dexterity,
    Artifact
}
public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    private Dictionary<GameObject, Dictionary<BuffType,int>> allCharacterBuffs = new Dictionary<GameObject, Dictionary<BuffType,int>>();

    private void Awake()
    {
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
    }

    public void RegisterCharacter(GameObject character)
    {
        if(!allCharacterBuffs.ContainsKey(character))
        {
            allCharacterBuffs.Add(character, new Dictionary<BuffType,int>());
        }
    }

    public void UnregisterCharacter(GameObject character)
    {
        if (allCharacterBuffs.ContainsKey(character))
        {
            allCharacterBuffs.Remove(character);
        }
    }

    public void ApplyBuff(GameObject target, BuffType type, int amount)
    {
        if(!allCharacterBuffs.ContainsKey(target))
        {
            RegisterCharacter(target);
        }

        var targetBuffs = allCharacterBuffs[target];

        // 부여하는 효과가 디버프인데, 타겟에게 인공물 버프가 있다면 무효화
        if (IsDebuff(type) && targetBuffs.TryGetValue(BuffType.Artifact, out int artifactVal) && artifactVal > 0)
        {
            targetBuffs[BuffType.Artifact]--;
            return;
        }
        if (targetBuffs.ContainsKey(type))
        {
            targetBuffs[type] += amount;
        }
        else
        {
            targetBuffs.Add(type, amount);
        }
    }

    public int GetBuffValue(GameObject target, BuffType type)
    {
        if(allCharacterBuffs.TryGetValue(target, out var targetBuffs))
        {
            if (targetBuffs.TryGetValue(type, out int value)) 
            {
                return value;
            }
        }
        return 0;
    }
    private bool IsDebuff(BuffType type)
    {
        return type == BuffType.Vulnerable || type == BuffType.Weak || type == BuffType.Poison;
    }

    public void TickBuffsAtTurnEnd(GameObject target)
    {
        if (!allCharacterBuffs.TryGetValue(target, out var targetBuffs)) return;

        List<BuffType> Keys = new List<BuffType>(targetBuffs.Keys);

        foreach (BuffType type in Keys)
        {
            if (targetBuffs[type] <= 0) continue;

            if (type == BuffType.Poison)
            {
                int poisonDamage = targetBuffs[type];
                BattleManager.Instance.ProcessPoisonDamage(target, poisonDamage);
                targetBuffs[type]--;
            }
            else if (type == BuffType.Vulnerable || type == BuffType.Weak)
            {
                targetBuffs[type]--;
            }
        }
    }
}
