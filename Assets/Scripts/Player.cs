using UnityEditor.Build.Content;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    public static Player Instance { get; private set; }
    public BuffSystem _buffSystem { get; private set; } 
    [Header("Stats")]
    public int maxHp = 80;
    public int currentHp;
    public int block;
    public int maxEnergy = 3;
    public int currentEnergy;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _buffSystem = GetComponent<BuffSystem>();
        }
        else Destroy(gameObject);

    }

    public void ResetStats()
    {
        currentHp = maxHp;
        block = 0;
        currentEnergy = maxEnergy;
    }

    public void OnStartTurn()
    {
        currentEnergy = maxEnergy;
        block = 0;
    }

    public void TakeDamage(int damage)
    {
        if(block > 0)
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

        if (damage >0)
        {
            currentHp -= damage;
            if(currentHp <= 0)
            {
                currentHp = 0;
                //게임오버 구현하기
            }
        }
    }

    public void TakeDirectDamage(int damage)
    {
        currentHp -= damage;

        if(currentHp <0)
        {
            currentHp = 0;
            // 게임오버 구현
        }
    }

    public void AddBlock(int amount)
    {
        block += amount;
    }

    public void AddMaxEnergy(int amount)
    {
        maxEnergy += amount;
    }

    public void AddCurEnergy(int amount)
    {
        currentEnergy += amount;
    }

    public bool SpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        return false;
    }
}
