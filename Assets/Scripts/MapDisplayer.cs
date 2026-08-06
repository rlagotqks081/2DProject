using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapDisplayer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject nodePrefab; 
    [SerializeField] private GameObject linePrefab; 

    [Header("Layout Settings")]
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private float xSpacing = 140f;
    [SerializeField] private float ySpacing = 180f;
    [SerializeField] private float bottomOffset = 80f;
    [SerializeField] private float nodeRadius = 30f;

    [Header("Node Icons")]
    [SerializeField] private Sprite iconNormalEnemy;
    [SerializeField] private Sprite iconEliteEnemy;
    [SerializeField] private Sprite iconEvent;
    [SerializeField] private Sprite iconShop;
    [SerializeField] private Sprite iconRest;

    [Header("Containers")]
    [SerializeField] private Transform linesContainer; 
    [SerializeField] private Transform nodesContainer;

    [Header("Node Colors")]
    [SerializeField] private Color nodeLocked = new Color(0.25f, 0.25f, 0.25f, 1.0f);
    [SerializeField] private Color nodeAvailable = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color nodeVisited = new Color(0.4f, 0.4f, 0.4f, 1.0f);

    [Header("Line Colors")]
    [SerializeField] private Color lineLocked = new Color(0.15f, 0.15f, 0.15f, 1.0f);
    [SerializeField] private Color lineAvailable = new Color(1.0f, 0.75f, 0.3f, 1.0f); 
    [SerializeField] private Color lineVisited = new Color(0.4f, 0.4f, 0.4f, 1.0f);

    private Dictionary<MapNode, RectTransform> spawnedNodes = new Dictionary<MapNode, RectTransform>();
    private Dictionary<KeyValuePair<MapNode, MapNode>, Image> spawnedLines = new Dictionary<KeyValuePair<MapNode, MapNode>, Image>();

    private System.Action<MapNode> onNodeClickedCallback;

    public void DrawMap(List<MapLayer> mapLayers, System.Action<MapNode> onNodeClicked)
    {
        onNodeClickedCallback = onNodeClicked;

        foreach (Transform child in linesContainer) Destroy(child.gameObject);
        foreach (Transform child in nodesContainer) Destroy(child.gameObject);
        spawnedNodes.Clear();
        spawnedLines.Clear();

        // content 세로 영역 확보
        float totalHeight = (mapLayers.Count * ySpacing) + bottomOffset + 120f;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        // 노드 생성 / 배치
        foreach (MapLayer layer in mapLayers)
        {
            int totalNodes = layer.nodes.Count;
            foreach (MapNode node in layer.nodes)
            {
                GameObject nodeObj = Instantiate(nodePrefab, nodesContainer);
                RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();

                // 가운데 정렬 공식 적용 배치
                float xPos = (node.x - (totalNodes - 1) / 2f) * xSpacing;
                float yPos = bottomOffset + (node.y * ySpacing);
                nodeRect.anchoredPosition = new Vector2(xPos, yPos);

                // 비주얼 세팅 및 버튼 클릭 이벤트 바인딩
                SetNodeVisualInitial(nodeObj, node);

                spawnedNodes.Add(node, nodeRect);
            }
        }

        // 선 생성 및 배치
        foreach (MapLayer layer in mapLayers)
        {
            foreach (MapNode node in layer.nodes)
            {
                RectTransform startRect = spawnedNodes[node];
                foreach (MapNode nextNode in node.nextNodes)
                {
                    RectTransform endRect = spawnedNodes[nextNode];
                    Image lineImg = CreateLine(startRect.anchoredPosition, endRect.anchoredPosition);

                    var lineKey = new KeyValuePair<MapNode, MapNode>(node, nextNode);
                    spawnedLines.Add(lineKey, lineImg);
                }
            }
        }

        RefreshMapVisuals();
    }

    private void SetNodeVisualInitial(GameObject nodeObj, MapNode node)
    {
        Image img = nodeObj.GetComponent<Image>();
        switch (node.nodeType)
        {
            case NodeType.NormalEnemy: img.sprite = iconNormalEnemy; break;
            case NodeType.EliteEnemy: img.sprite = iconEliteEnemy; break;
            case NodeType.Event: img.sprite = iconEvent; break;
            case NodeType.Shop: img.sprite = iconShop; break;
            case NodeType.Rest: img.sprite = iconRest; break;
        }

        Button btn = nodeObj.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            if (node.nodeState == NodeState.Available) onNodeClickedCallback?.Invoke(node);
        });
    }

    private Image CreateLine(Vector2 startPos, Vector2 endPos)
    {
        float distance = Vector2.Distance(startPos, endPos);
        Vector2 direction = (endPos - startPos).normalized;

        // 그려진 선이 노드 Image 안으로 침투하는것 방지
        Vector2 adjustedStartPos = startPos + (direction * nodeRadius);
        float adjustedDistance = distance - (nodeRadius * 2f);

        if (adjustedDistance < 0) adjustedDistance = 0;

        GameObject lineObj = Instantiate(linePrefab, contentRect);
        lineObj.transform.SetAsFirstSibling(); // 뒤로 깔기

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        lineRect.anchoredPosition = adjustedStartPos;
        lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, adjustedDistance);

        // 회전각 계산 
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle - 90f);

        return lineObj.GetComponent<Image>();
    }

    public void RefreshMapVisuals()
    {
        foreach (var pair in spawnedNodes)
        {
            MapNode node = pair.Key;
            Button btn = pair.Value.GetComponent<Button>();
            Image img = pair.Value.GetComponent<Image>();

            switch (node.nodeState)
            {
                case NodeState.Locked:
                    img.color = nodeLocked;
                    btn.interactable = false;
                    break;
                case NodeState.Available:
                    img.color = nodeAvailable;
                    btn.interactable = true;
                    break;
                case NodeState.Visited:
                    img.color = nodeVisited;
                    btn.interactable = false;
                    break;
            }
        }

        foreach (var pair in spawnedLines)
        {
            MapNode startNode = pair.Key.Key;
            MapNode endNode = pair.Key.Value;
            Image lineImg = pair.Value;

            if (startNode.nodeState == NodeState.Visited && endNode.nodeState == NodeState.Visited)
            {
                lineImg.color = lineVisited; 
            }
            else if (startNode.nodeState == NodeState.Visited && endNode.nodeState == NodeState.Available)
            {
                lineImg.color = lineAvailable; 
            }
            else if (startNode.y == 0 && startNode.nodeState == NodeState.Available)
            {
                lineImg.color = lineAvailable; 
            }
            else
            {
                lineImg.color = lineLocked; 
            }
        }
    }
}