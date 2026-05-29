using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable
{
    public static Player Instance { get; private set; }
    public BuffSystem _buffSystem { get; private set; }
    [Header("")]
    public Image fillImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;
    [Header("Stats")]
    public int maxHp = 80;
    private int currentHp;
    public int block;
    public int maxEnergy = 3;
    public int currentEnergy;
    public int CurrentHp
    {
        get => currentHp;
        set
        {
            // 0보다 작으면 0으로, 아니면 입력된 값 그대로 설정
            currentHp = Mathf.Max(0, value);

            if (currentHp <= 0)
            {
                BattleFlowManager.Instance.ChackBattleState();
            }
        }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _buffSystem = GetComponent<BuffSystem>();
        }
        else Destroy(gameObject);

    }

    private void UpdateHealthBar()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = (float)CurrentHp / maxHp;
            healthText.text = currentHp.ToString() + "/" + maxHp.ToString();
        }
    }
    public void ResetStats()
    {
        CurrentHp = maxHp;
        block = 0;
        currentEnergy = maxEnergy;
        UpdateHealthBar();
        UIManager.Instance.UpdatePlayerEnergyText();
    }

    public void OnStartTurn()
    {
        currentEnergy = maxEnergy;
        block = 0;
    }

    public void TakeDamage(int damage, int count = 1)
    {
        for(int i = 0; i < count; i++)
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
                CurrentHp -= damage;
                UpdateHealthBar();
            }
        }
    }

    public void TakeDirectDamage(int damage, int count = 1)
    {
        CurrentHp -= damage;
    }

    public void AddBlock(int amount, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            block += amount;
        }
    }

    public void AddMaxEnergy(int amount)
    {
        maxEnergy += amount;
    }

    public void AddCurEnergy(int amount, int count = 1)
    {
        if (amount < 0) return;
        for (int i = 0; i < count; i++)
        {
            currentEnergy += amount;
        }
        UIManager.Instance.UpdatePlayerEnergyText();

    }

    public bool SpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            UIManager.Instance.UpdatePlayerEnergyText();
            return true;
        }
        return false;
    }
}
