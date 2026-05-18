using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public struct SubEffect
{
    public string effectType;
    public int value;
}
public struct MonsterAction
{
    public string description;
    public List<SubEffect> subEffects;
}
public class Monster : MonoBehaviour
{
    [Header("Identity")]
    public string monsterName;
    public int monsterID;

    [Header("Stats")]
    public int maxHp;
    public int currentHp;
    public int block;

    [Header("Pattern Data")]
    public List<MonsterAction> patternList = new List<MonsterAction>();

    private int currentTurnIndex = 0;
    public MonsterAction CurrentIntent => patternList[currentTurnIndex];

    public void SetupMonster(int id, string name, int hp, List<MonsterAction> patterns)
    {
        monsterID = id;
        monsterName = name;
        maxHp = hp;
        currentHp = hp;
        patternList = patterns;
        currentTurnIndex = 0;

        Sprite monsterSprite = Resources.Load<Sprite>($"Sprites/Monsters/Monster_{monsterID}");
        if (monsterSprite != null)
        {
            GetComponent<Image>().sprite = monsterSprite;
        }

        UpdateIntentUI();
    }

    public void UpdateIntentUI()
    {
        if (patternList.Count == 0) return;

        MonsterAction nextAction = CurrentIntent;


    }

    public void ExecuteTurn()
    {
        if (currentHp <= 0) return;
        if (patternList.Count == 0) return;

        MonsterAction action = CurrentIntent;

        currentTurnIndex = (currentTurnIndex + 1) % patternList.Count;

        UpdateIntentUI();
    }

    public void TakeDamage(int damage)
    {
        if (block > 0)
        {
            if (damage <= block)
            {
                block -= damage;
                damage = 0;
            }
            else
            {
                damage -= block;
                block = 0;
            }
        }

        if (damage > 0)
        {
            currentHp -= damage;

            if (currentHp <= 0)
            {
                currentHp = 0;
                //게임오버 구현하기
            }
        }
    }
    public void TakeDirectDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            // 게임오버 구현하기
        }
    }
}
