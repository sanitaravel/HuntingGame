using UnityEngine;

public class Node
{
    public Vector3Int cellPosition;
    public bool isWalkable;
    public Node parent;

    public int gCost; // Distance from start
    public int hCost; // Distance to target
    public int fCost => gCost + hCost;

    public Node(Vector3Int pos, bool walkable)
    {
        cellPosition = pos;
        isWalkable = walkable;
        gCost = int.MaxValue;
        hCost = 0;
    }
}
