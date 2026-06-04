using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum NodeType { NormalEnemy, EliteEnemy, Event, Shop, Rest, Boss }
public enum NodeState
{
    Locked,       
    Available,   
    Visited      
}

public class MapNode  
{
    public int x; // 행 안에서의 가로 번호 
    public int y; // 몇 번째 층인지 (세로 번호 0 ~ 14) - 총 15층
    public NodeType nodeType;

    // 이 노드에서 위로 갈 수 있는 다음 노드들의 목록
    public List<MapNode> nextNodes = new List<MapNode>();

    public NodeState nodeState = NodeState.Locked;
}