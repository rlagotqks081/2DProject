using System.Collections.Generic;
using UnityEngine;

public static class StringDatabase
{
    private static Dictionary<BuffType, string> BuffDesc = new Dictionary<BuffType, string>
    {
        { BuffType.Strength, "공격 피해가 힘 수치 만큼 증가합니다." },
        { BuffType.Vulnerable, "공격 으로부터 50% 더 많은 피해를 받습니다." },
        { BuffType.Dexterity, "얻는 방어도가 민첩 수치 만큼 증가합니다." },
        { BuffType.Frail, "얻는 방어도가 25% 감소합니다." },
        { BuffType.Artifact, "디버프를 얻을 때 인공물 수치 만큼 무시합니다." },
        { BuffType.Weak, "공격 시 피해량이 25% 감소합니다." },
        { BuffType.Poison, "상대 턴이 시작할 때 중독된 적은 중독 수치만큼 피해를 받습니다. \n중독은 매 턴 1씩 감소합니다." }

    };

    private static Dictionary<BuffType, string> BuffTitle = new Dictionary<BuffType, string>
    {
        { BuffType.Strength, "힘" },
        { BuffType.Vulnerable, "취약" },
        { BuffType.Dexterity, "민첩" },
        { BuffType.Frail, "취약" },
        { BuffType.Artifact, "인공물" },
        { BuffType.Weak, "약화" },
        { BuffType.Poison, "중독" }


    };

    public static string GetBuffDesc(BuffType type)
    {
        if(BuffDesc.ContainsKey(type)) return BuffDesc[type];
        return null;
    }

    public static string GetBuffTitle(BuffType type)
    {
        if(BuffTitle.ContainsKey(type)) return BuffTitle[type];
        return null;
    }
}
