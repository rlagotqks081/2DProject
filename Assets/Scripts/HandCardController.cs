using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HandCardController : MonoBehaviour
{
    [Header("Hand settings")]
    [SerializeField] private float cardSpacing = 120f;
    [SerializeField] private float arcIntensity = 15f;
    [SerializeField] private float rotationIntensity = 5f;

    public List<RectTransform> cards = new List<RectTransform>();

    [ContextMenu("AlignCards")]
    public void AlignCards()
    {
        cards.Clear();
        foreach (Transform child in transform)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            if(rect != null && child.gameObject.activeSelf)
            {
                cards.Add(rect);
            }
        }
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        float midindex = (cardCount - 1) / 2f;

        for(int i = 0; i < cardCount; i++)
        {
            float offset = i - midindex;
            float posX = offset * cardSpacing;
            float posY = offset * offset * arcIntensity + 100f;
            float rotZ = -offset * rotationIntensity;
            cards[i].localPosition = new Vector3(posX, posY, 0f);
            cards[i].localRotation = Quaternion.Euler(0f, 0f, rotZ);

        }
    }
}
