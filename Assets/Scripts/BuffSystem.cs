using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BuffSystem : MonoBehaviour
{

    private Dictionary<BuffType, int> currentBuffs = new Dictionary<BuffType, int>();
    private Dictionary<BuffType, GameObject> buffIcons = new Dictionary<BuffType, GameObject>();

    public void AddBuff(BuffType type, int value)
    {
        if (currentBuffs.ContainsKey(type))
        {
            currentBuffs[type] += value;
        }
        else
        {
            currentBuffs.Add(type, value);
        }

        if (currentBuffs[type] <= 0) currentBuffs.Remove(type);
        UpdateBuffIcon(type);
    }

    public bool HasBuff(BuffType type)
    {
        return currentBuffs.ContainsKey(type) && currentBuffs[type] > 0;
    }

    public int GetBuffValue(BuffType type)
    {
        if(currentBuffs.TryGetValue(type, out int value))
        {
            return value;
        }
        return 0;
    }

    public void ClearBuffs()
    {
        currentBuffs.Clear();
        foreach(GameObject obj in buffIcons.Values)
        {
            obj.SetActive(false);
        }
    }

    public void AddBuffIcon(BuffType type)
    {
        if (buffIcons.ContainsKey(type)) return;
        GameObject newIcon = SpawnManager.Instance.SpawnBuffIcons(this);
        if (newIcon != null)
        {
            newIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprite/Buff_Icon/" + type.ToString() + "_Icon");
            buffIcons.Add(type, newIcon);
        }

    }
    public void RemoveBuff(BuffType type)
    {
        if (currentBuffs.ContainsKey(type)) currentBuffs.Remove(type);
    }
    public void UpdateBuffIcon(BuffType type)
    {
        if (!buffIcons.ContainsKey(type))
        {
            AddBuffIcon(type);
        }


        if (!currentBuffs.ContainsKey(type))
        {
            buffIcons[type].SetActive(false);
        }
        else
        {
            buffIcons[type].GetComponentInChildren<TextMeshProUGUI>().text = currentBuffs[type].ToString();
            buffIcons[type].SetActive(true);
        }
    }
    public void UpdateAllBuffIcon()
    {
        foreach(BuffType type in buffIcons.Keys)
        {
            UpdateBuffIcon(type);
        }
    }
    public void TickTurnBuffs()
    {
        Debug.Log($"buffststem - {this.gameObject}");
        foreach(BuffType type in currentBuffs.Keys.ToList())
        {
            switch(type)
            {
                case BuffType.Vulnerable:
                case BuffType.Weak:
                case BuffType.Poison:
                case BuffType.Frail:
                    AddBuff(type, -1);
                    break;
            }
        }
    }

    
}
