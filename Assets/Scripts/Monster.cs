using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour, IDamageable
{
    // 나를 태어나게 한 원본 SO (디버깅이나 원본 데이터 확인용)
    public MonsterData OriginData { get; private set; }
    public BuffSystem buffSystem { get; private set; }

    [Header("")]
    public Image fillImage;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;

    [Header("[런타임 실시간 스탯]")]
    public string monsterName;
    public int maxHp;
    public int currentHp;
    public int currentBlock;
    public int actionValue;
    public SpriteRenderer monsterImage;
    public SpriteRenderer actionIcon;
    public TextMeshPro actionText;
    public GameObject healthBar_Block;
    public TextMeshPro blockValueText;

    [Header("[런타임 AI 패턴]")]
    public List<MonsterPatternData> runtimePatterns;
    private int currentPatternIndex = 0;
    private int CurrentHp
    {
        get => currentHp;
        set
        {
            // 0보다 작으면 0으로, 아니면 입력된 값 그대로 설정
            currentHp = Mathf.Max(0, value);

            if (currentHp <= 0)
            {
                BattleFlowManager.Instance.ChackBattleState();
                BuffManager.Instance.RemoveBuffObj(this.gameObject);
            }
        }
    }
    private void Start()
    {
        // 씬에 직접 배치된 경우를 위한 예외 처리
        if (OriginData != null && CurrentHp == 0)
        {
            //SetupMonster(OriginData);
        }
    }

    public void SetupMonster(MonsterData data)
    {
        OriginData = data;

        monsterName = data.monsterName;
        maxHp = data.maxHp;
        CurrentHp = maxHp;
        currentBlock = 0;
        nameText.text = data.monsterName;
        monsterImage.sprite = Resources.Load<Sprite>(data.monsterIcon);
        runtimePatterns = new List<MonsterPatternData>(data.patterns);
        currentPatternIndex = 0;
        buffSystem = GetComponent<BuffSystem>();
        UpdateHealthBar();
        UpdateBlockIcon();
        Debug.Log($"[Monster] '{monsterName}' 로드 완료. (HP: {maxHp})");
    }
    public void TakeDirectDamage(int damage, int count = 1)
    {
        CurrentHp -= damage;
        UpdateHealthBar();
    }
    public void TakeDamage(int damage, int count = 1)
    {
        
        if (damage <= 0) return;
        for (int i = 0; i < count; i++)
        {
            int finalDamage = damage;
            if (CurrentHp <= 0) return;
            if (currentBlock > 0)
            {
                if (currentBlock >= finalDamage)
                {
                    currentBlock -= finalDamage;
                    finalDamage = 0;
                }
                else
                {
                    finalDamage -= currentBlock;
                    currentBlock = 0;
                }
                UpdateBlockIcon();
            }

            if (finalDamage > 0)
            {
                CurrentHp -= finalDamage;
                UpdateHealthBar();
            }
        }
    }

    private void UpdateHealthBar()
    {
        if(fillImage != null)
        {
            fillImage.fillAmount = (float)CurrentHp / maxHp;
            healthText.text = CurrentHp.ToString() + "/" + maxHp.ToString();
        }
    }
    private void UpdateBlockIcon()
    {
        if(currentBlock > 0)
        {
            healthBar_Block.SetActive(true);
            blockValueText.text = currentBlock.ToString();
        }
        else
        {
            healthBar_Block.SetActive(false);
        }
    }
    private void Die()
    {
        Debug.Log($"[{monsterName}] 사망!");

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.activeMonsters.Remove(this);
        }

        Destroy(gameObject);
    }

    public void AddBlock(int value, int count = 1)
    {
        for(int i  = 0; i < count; i++)
        {
            currentBlock += value;
        }
        UpdateBlockIcon();
    }

    public MonsterPatternData GetCurrentIntent()
    {
        if (runtimePatterns == null || runtimePatterns.Count == 0) return null;
        return runtimePatterns[currentPatternIndex % runtimePatterns.Count];
    }

    public void AdvancePattern()
    {
        if (runtimePatterns == null || runtimePatterns.Count <= 1) return;
        currentPatternIndex = (currentPatternIndex + 1) % runtimePatterns.Count;

        UpdateNextActionIcon();
    }

    // 행동아이콘의 생성기준은 일단 첫번째 행동을 기준으로 정함
    public void UpdateNextActionIcon()
    {
        switch (runtimePatterns[currentPatternIndex].effects[0].effectType)  // 몬스터의 다음 행동 아이콘 업데이트
        {
            case MonsterActionType.Attack:
                actionIcon.sprite = Resources.Load<Sprite>("Sprite/Battle_Icon/Attack_Icon");
                actionValue = runtimePatterns[currentPatternIndex].effects[0].value;
                actionText.text = CardCalculator.GetMonsterAttackValue(actionValue, this).ToString();
                break;
            case MonsterActionType.Defend:
                actionIcon.sprite = Resources.Load<Sprite>("Sprite/Battle_Icon/Defend_Icon");
                actionValue = runtimePatterns[currentPatternIndex].effects[0].value;
                actionText.text = UtilManager.CalculateFinalBlock(actionValue, this.gameObject).ToString();
                break;
            case MonsterActionType.Buff:
                // actionIcon.sprite = Resources.Load<Sprite>("Sprite/Battle_Icon/Buff_Icon");
                actionText.text = "";
                break;
            case MonsterActionType.Debuff:
                // actionIcon.sprite = Resources.Load<Sprite>("Sprite/Battle_Icon/Debuff_Icon");
                actionText.text = "";
                break;
        }
    }
    public void UpdateCurStat()
    {
        currentBlock = 0;  // 나중에 바리케이드버프를 만들면 바꿔야할곳
        UpdateBlockIcon();
    }
    public void SetHighlight(bool IsHighlight)
    {

    }
}