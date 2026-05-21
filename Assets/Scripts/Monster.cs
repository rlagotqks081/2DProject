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

    private void Start()
    {
        // 씬에 직접 배치된 경우를 위한 예외 처리
        if (OriginData != null && currentHp == 0)
        {
            SetupMonster(OriginData);
        }
    }

    public void SetupMonster(MonsterData data)
    {
        OriginData = data;

        monsterName = data.monsterName;
        maxHp = data.maxHp;
        currentHp = maxHp;
        currentBlock = 0;

        runtimePatterns = new List<MonsterPatternData>(data.patterns);
        currentPatternIndex = 0;
        buffSystem = GetComponent<BuffSystem>();
        Debug.Log($"[Monster] '{monsterName}' 로드 완료. (HP: {maxHp})");
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = damage;
        if (finalDamage <= 0) return;

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
            currentHp = Mathf.Max(0, currentHp - finalDamage);
            UpdateHealthBar();
            Debug.Log($"[{monsterName}] 피격! 남은 체력: {currentHp}/{maxHp}");
        }

        // 3. 사망 체크
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if(fillImage != null)
        {
            fillImage.fillAmount = (float)currentHp / maxHp;
            healthText.text = currentHp.ToString() + "/" + maxHp.ToString();
        }
    }
    private void Die()
    {
        Debug.Log($"[{monsterName}] 사망!");

        // 배틀 매니저의 활성 리스트에서 자신을 제거
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.activeMonsters.Remove(this);
        }

        Destroy(gameObject);
    }

    public void AddBlock(int value)
    {
        if (value <= 0) return;
        currentBlock += value;
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