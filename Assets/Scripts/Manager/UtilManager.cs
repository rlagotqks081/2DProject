using UnityEngine;

public static class UtilManager
{
    public static int CalculateFinalDamage(int baseDamage, GameObject attacker, GameObject target)
    {
        int finalDamage = baseDamage;

        BuffSystem attackerBuff = attacker.GetComponent<BuffSystem>();
        if(attackerBuff != null )
        {
            finalDamage += attackerBuff.GetBuffValue(BuffType.Strength);

            if(attackerBuff.HasBuff(BuffType.Weak))
            {
                finalDamage = Mathf.FloorToInt(finalDamage * 0.75f);
            }
        }

        BuffSystem targetBuff = target.GetComponent<BuffSystem>();
        if(targetBuff != null )
        {
            if(targetBuff.HasBuff(BuffType.Vulnerable))
            {
                finalDamage = Mathf.FloorToInt(finalDamage * 1.5f);
            }          
        }
        return Mathf.Max(0, finalDamage);
    }

    public static int CalculateFinalBlock(int baseBlock, GameObject targetObj)
    {
        int finalBlock = baseBlock;

        BuffSystem targetBuff = targetObj.GetComponent<BuffSystem>();  
        if(targetBuff != null )
        {
            finalBlock += targetBuff.GetBuffValue(BuffType.Dexterity);
            
            if(targetBuff.HasBuff(BuffType.Frail))
            {
                finalBlock = Mathf.FloorToInt(finalBlock * 0.75f);
            }
        }
        return Mathf.Max(0, finalBlock);
    }
}
