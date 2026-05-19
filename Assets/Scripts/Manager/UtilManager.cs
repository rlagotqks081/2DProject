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

    public static int CalculateFinalBlock(int baseBlock, GameObject playerObj)
    {
        int finalBlock = baseBlock;

        BuffSystem playerBuff = playerObj.GetComponent<BuffSystem>();  
        if(playerBuff != null )
        {
            finalBlock += playerBuff.GetBuffValue(BuffType.Dexterity);
            
            if(playerBuff.HasBuff(BuffType.Frail))
            {
                finalBlock = Mathf.FloorToInt(finalBlock * 0.75f);
            }
        }
        return Mathf.Max(0, finalBlock);
    }
}
