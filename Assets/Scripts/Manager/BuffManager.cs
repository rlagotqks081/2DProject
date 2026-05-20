using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering;


public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    

    private void Awake()
    {
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
    }

    public void ApplyBuff(GameObject target, BuffType type, int value)
    {
        if (target == null) return;

        BuffSystem buffSystem = target.GetComponent<BuffSystem>();
        if(buffSystem != null)
        {
            buffSystem.AddBuff(type, value);
        }
    }

    public void RemoveBuff(GameObject target, BuffType type)
    {
        BuffSystem buffSystem = target.GetComponent<BuffSystem>();
        if (buffSystem != null)
        {
            buffSystem.RemoveBuff(type);
        }
    }
}
