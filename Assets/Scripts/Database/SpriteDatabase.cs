using System.Collections.Generic;
using UnityEngine;

public static class SpriteDatabase
{
    private static Dictionary<BuffType, Sprite> BuffSprite = new Dictionary<BuffType, Sprite>();

    public static void LoadAllBuffs()
    {
        foreach(BuffType type in System.Enum.GetValues(typeof(BuffType))) 
        {
            Sprite icon = Resources.Load<Sprite>("Sprite/Buff_Icon/" + type.ToString() + "_Icon");
            if(icon != null)
            {
                BuffSprite.Add(type, icon);
            }
        }
    }

    public static Sprite GetBuffSprite(BuffType type)
    {
        if(BuffSprite.ContainsKey(type)) return BuffSprite[type];
        return null;
    }
}
