using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Generation Size")]
    public int mapHeight = 15;       // 층수
    public int maxCellsWidth = 5;    // 한층에 존재하는 최대 노드 수

    [Header("Reference")]
    [SerializeField] private MapDisplayer mapDisplayer;

    private List<MapLayer> mapLayers = new List<MapLayer>();

    private void Start()
    {
        GenerateMap();
    }

    [ContextMenu("새로운 맵 생성")]
    public void GenerateMap()
    {
        mapLayers.Clear();



        for (int y = 0; y < mapHeight; y++)
        {
            MapLayer layer = new MapLayer { layerIndex = y };

            // 각 층마다 랜덤하게 3~5개의 노드를 생성함
            int nodeCount = Random.Range(3, maxCellsWidth + 1);
            for (int x = 0; x < nodeCount; x++)
            {
                MapNode node = new MapNode { x = x, y = y };
                layer.nodes.Add(node);
            }
            mapLayers.Add(layer);
        }

        // 아래층 윗층 노드 연결하기
        ConnectLayers();

        // 노드 종류 부여하기(몬스터, 상점, 휴식, 이벤트)
        AssignNodeTypes();

        // 초반 노드들 셋업
        SetInitialNodeStates();

        if (mapDisplayer != null)
        {
            mapDisplayer.DrawMap(mapLayers, OnNodeSelectedInGame);
        }
    }

    private void ConnectLayers()
    {
        for (int y = 0; y < mapHeight - 1; y++)
        {
            MapLayer currentLayer = mapLayers[y];
            MapLayer nextLayer = mapLayers[y + 1];

            int currentCount = currentLayer.nodes.Count;
            int nextCount = nextLayer.nodes.Count;

            // 아래층 노드에서 위층 노드로 인접한 범위 내에서 연결하기
            foreach (MapNode currentNode in currentLayer.nodes)
            {
                // 현재 내 위치(x)를 기준으로 다음 층에서 정렬 상 가장 가까운 인덱스 비율을 계산
                float ratio = (float)currentNode.x / currentCount;
                int targetNextX = Mathf.Clamp(Mathf.RoundToInt(ratio * nextCount), 0, nextCount - 1);

                // 가장 가까운 위층 노드를 메인 길로 지정하여 추가
                currentNode.nextNodes.Add(nextLayer.nodes[targetNextX]);

                // 15%확률로 해당 노드에서 길 추가생성(갈림길)
                if (Random.value < 0.15f)
                {
                    // 왼쪽(-1) 또는 오른쪽(+1) 인접한 칸 중 하나를 랜덤하게 골라 연결
                    int neighborX = targetNextX + (Random.value < 0.5f ? -1 : 1);

                    // 유효한 인덱스 범위 안이고, 중복 연결이 아니라면 선 추가
                    if (neighborX >= 0 && neighborX < nextCount)
                    {
                        MapNode neighborNode = nextLayer.nodes[neighborX];
                        if (!currentNode.nextNodes.Contains(neighborNode))
                        {
                            currentNode.nextNodes.Add(neighborNode);
                        }
                    }
                }
            }

            // 연결안된 노드 강제로 연결
            foreach (MapNode nextNode in nextLayer.nodes)
            {
                bool hasParent = false;
                foreach (MapNode currentNode in currentLayer.nodes)
                {
                    if (currentNode.nextNodes.Contains(nextNode))
                    {
                        hasParent = true;
                        break;
                    }
                }

                // 부모가 없는 외톨이 노드가 있다면
                if (!hasParent)
                {
                    // 이 외톨이 노드와 일직선 상에 가장 가까운 아래층 부모를 찾아서 강제 연결
                    float ratio = (float)nextNode.x / nextCount;
                    int closestParentX = Mathf.Clamp(Mathf.RoundToInt(ratio * currentCount), 0, currentCount - 1);

                    currentLayer.nodes[closestParentX].nextNodes.Add(nextNode);
                }
            }
        }
    }

    private void AssignNodeTypes()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            MapLayer layer = mapLayers[y];

            foreach (MapNode node in layer.nodes)
            {
                if (y == 0) { node.nodeType = NodeType.NormalEnemy; continue; }
                if (y == mapHeight - 1) { node.nodeType = NodeType.Rest; continue; }
                if (y == mapHeight / 2) { node.nodeType = NodeType.Event; continue; }

                if (y < 5) node.nodeType = GetRandomTypeStandard();
                else node.nodeType = GetRandomTypeWithElite();

                FixConsecutiveTypes(node, y);
            }
        }
    }

    private NodeType GetRandomTypeStandard()
    {
        float rand = Random.value;
        if (rand < 0.55f) return NodeType.NormalEnemy;
        if (rand < 0.80f) return NodeType.Event;
        if (rand < 0.92f) return NodeType.Shop;
        return NodeType.Rest;
    }

    private NodeType GetRandomTypeWithElite()
    {
        float rand = Random.value;
        if (rand < 0.45f) return NodeType.NormalEnemy;
        if (rand < 0.65f) return NodeType.EliteEnemy;
        if (rand < 0.85f) return NodeType.Event;
        if (rand < 0.93f) return NodeType.Shop;
        return NodeType.Rest;
    }

    private void FixConsecutiveTypes(MapNode node, int currentY)
    {
        if (currentY == 0) return;
        MapLayer parentLayer = mapLayers[currentY - 1];
        bool isParentShop = false;
        bool isParentRest = false;

        foreach (MapNode parentNode in parentLayer.nodes)
        {
            if (parentNode.nextNodes.Contains(node))
            {
                if (parentNode.nodeType == NodeType.Shop) isParentShop = true;
                if (parentNode.nodeType == NodeType.Rest) isParentRest = true;
            }
        }

        if (node.nodeType == NodeType.Shop && isParentShop) node.nodeType = NodeType.NormalEnemy;
        else if (node.nodeType == NodeType.Rest && isParentRest) node.nodeType = NodeType.Event;
    }

    private void SetInitialNodeStates()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            foreach (MapNode node in mapLayers[y].nodes)
            {
                node.nodeState = (y == 0) ? NodeState.Available : NodeState.Locked;
            }
        }
    }

    // [인게임 실제 진행 흐름 함수] 플레이어가 노드를 최종 선택했을 때 실행됨
    private void OnNodeSelectedInGame(MapNode clickedNode)
    {
        if (GameManager.Instance.currentState != GameState.Map) return;

        foreach (MapLayer layer in mapLayers)
        {
            foreach (MapNode node in layer.nodes)
            {
                if (node.nodeState == NodeState.Available) node.nodeState = NodeState.Locked;
            }
        }

        clickedNode.nodeState = NodeState.Visited;

        foreach (MapNode nextNode in clickedNode.nextNodes)
        {
            nextNode.nodeState = NodeState.Available;
        }

        mapDisplayer.RefreshMapVisuals();
        UIManager.Instance.OnClickMapOpenButton();
        if(GameManager.Instance.isFirstMapSelect)
        {
            UIManager.Instance.MainMenuUI.SetActive(false);
            UIManager.Instance.HighBarUI.SetActive(true);
        }

        switch(clickedNode.nodeType)
        {
            case NodeType.NormalEnemy:
                GameManager.Instance.ChangeState(GameState.Monster);
                break;
            case NodeType.EliteEnemy:
                GameManager.Instance.ChangeState(GameState.Monster);
                break;
            case NodeType.Event:
                GameManager.Instance.ChangeState(GameState.Map);
                break;
            case NodeType.Rest:
                if (clickedNode.y == 14) GameManager.Instance.QuitGame();
                else GameManager.Instance.ChangeState(GameState.Map);
                break;
            case NodeType.Boss:
                GameManager.Instance.ChangeState(GameState.Map);
                break;
            case NodeType.Shop:
                GameManager.Instance.ChangeState(GameState.Map);
                break;

        }
        
    }
}