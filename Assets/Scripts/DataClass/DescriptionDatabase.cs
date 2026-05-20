using System.Collections.Generic;
using UnityEngine;

public static class DescriptionDatabase
{
    private static readonly Dictionary<string, string> Templates = new Dictionary<string, string>
    {
        {"Damage", "적에게 피해를 {Damage} 줍니다." },
        {"Block", "방어도를 {Block} 얻습니다." },
        {"GetBuff", "{BuffType}을 {BuffValue} 얻습니다." },
        {"ApplyBuff", "{BuffType}을 {BuffValue} 부여합니다." },
        {"DrawCard", "카드를 {DrawValue}장 뽑습니다." },
        {"DiscardCard", "카드를 {DiscardValue}장 버립니다." },
        {"Damage_By_Block", "현재 방어도 만큼의 피해를 줍니다.\n {(피해를 {BlockValue} 줍니다.)}" },
        {"Damage_Use_AllCost", "피해를 {All_Value} 만큼 X 번 줍니다." },
        {"Block_Use_AllCost", "방어도를 {All_Value} 만큼 X 번 얻습니다." }
    };

    private static readonly Dictionary<string, string> BuffDesc = new Dictionary<string, string>
    {
        {"Poison","독" },
        {"Strength","힘" },
        {"Vulnerable","취약" },
        {"Weak","약화" },
        {"Artifact","인공물" },
        {"Dexterity","민첩" },
        {"Frail","손상" }
    };

    public static string GetTemplates(string key) => Templates.ContainsKey(key) ? Templates[key] : "설명 없음";
    public static string GetBuffDesc(string key) => BuffDesc.ContainsKey(key) ? BuffDesc[key] : "설명 없음";
}
