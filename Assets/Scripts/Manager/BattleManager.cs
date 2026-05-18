using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int CalculateFinalDamage(int baseDamage, GameObject attacker, GameObject target)
    {
        // 이후에 힘버프 효율이 다른 공격카드 (대검, 연타)등은 나중에 구현해야됨.
        int finalDamage = baseDamage;
        int strength = BuffManager.Instance.GetBuffValue(attacker, BuffType.Strength);
        finalDamage += strength;

        if(BuffManager.Instance.GetBuffValue(attacker, BuffType.Weak) > 0)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * 0.75f);
        }
        if(BuffManager.Instance.GetBuffValue(target, BuffType.Vulnerable) > 0)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * 1.5f);
        }
        return finalDamage;
    }

    public void ExecuteCardEffects(CardData cardData, GameObject targetMonster) // CardData는 내일만들거임!!!
    {
        GameObject playerObj = Player.Instance.gameObject;
        // CardData 클래스 만들고, CardEffect enum만든후에 구현
    }

    public void ExecuteMonsterAction(GameObject monsterObj, MonsterAction action)
    {
        //여기도 비슷하게 MonsterAction의 SubEffect를 더 보완하고 구현
    }

    public void ProcessPoisonDamage(GameObject target, int poisonDamage)
    {

        Player player = target.GetComponent<Player>();
        if(player != null)
        {
            player.TakeDirectDamage(poisonDamage);
            return;
        }

        Monster monster = target.GetComponent<Monster>();
        if(monster != null)
        {
            monster.TakeDirectDamage(poisonDamage);
        }
    }
}
