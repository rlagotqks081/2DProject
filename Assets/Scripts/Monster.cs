using System.Collections.Generic;
using TMPro;
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

    [Header("[런타임 AI 패턴]")]
    private List<MonsterPatternData> runtimePatterns = new List<MonsterPatternData>();
    private int currentPatternIndex = 0;
    public int CurrentHp
    {
        get => currentHp;
        set
        {
            // 0보다 작으면 0으로, 아니면 입력된 값 그대로 설정
            currentHp = Mathf.Max(0, value);

            if (currentHp <= 0)
            {
                //Die();   죽는로직 맨들기
            }
        }
    }
    private void Start()
    {
        // 씬에 직접 배치된 경우를 위한 예외 처리
        if (OriginData != null && CurrentHp == 0)
        {
            SetupMonster(OriginData);
        }
    }

    public void SetupMonster(MonsterData data)
    {
        OriginData = data;

        monsterName = data.monsterName;
        maxHp = data.maxHp;
        CurrentHp = maxHp;
        currentBlock = 0;

        runtimePatterns = new List<MonsterPatternData>(data.patterns);
        currentPatternIndex = 0;
        buffSystem = GetComponent<BuffSystem>();
        Debug.Log($"[Monster] '{monsterName}' 로드 완료. (HP: {maxHp})");
    }

    public void TakeDamage(int damage, int count = 1)
    {
        int finalDamage = damage;
        if (finalDamage <= 0) return;
        for (int i = 0; i < count; i++)
        {
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
    }
    public void SetHighlight(bool IsHighlight)
    {

    }
}