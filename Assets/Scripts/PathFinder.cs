using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    public Tilemap tilemap;
    public List<Vector3Int> FindPath(Vector3Int startCell, Vector3Int targetCell)
    {
        Debug.Log("Starting A* pathfinding from " + startCell + " to " + targetCell);
        Node startNode = new Node(startCell, IsWalkable(startCell));
        Node targetNode = new Node(targetCell, IsWalkable(targetCell));

        startNode.gCost = 0;
        startNode.hCost = Mathf.Abs(startCell.x - targetCell.x) + Mathf.Abs(startCell.y - targetCell.y);

        Debug.Log("Start walkable: " + startNode.isWalkable + ", Target walkable: " + targetNode.isWalkable);

        List<Node> open = new List<Node>();
        HashSet<Node> closed = new HashSet<Node>();
        Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();
        List<Vector3Int> path = null;

        allNodes[startCell] = startNode;
        allNodes[targetCell] = targetNode;

        open.Add(startNode);
        int maxIterations = 50000;
        int iterations = 0;

        while (open.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Node current = open[0];
            for (int i = 1; i < open.Count; i++)
                if (open[i].fCost < current.fCost || open[i].fCost == current.fCost && open[i].hCost < current.hCost)
                    current = open[i];

            open.Remove(current);
            closed.Add(current);

            if (current.cellPosition == targetNode.cellPosition)
            {
                Debug.Log("Path found");
                path = RetracePath(startNode, current);
                break;
            }

            foreach (Node neighbor in GetNeighbors(current, allNodes))
            {
                if (!neighbor.isWalkable || closed.Contains(neighbor))
                    continue;

                int newCost = current.gCost + 10; // No diagonal yet
                if (newCost < neighbor.gCost || !open.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = Mathf.Abs(neighbor.cellPosition.x - targetNode.cellPosition.x)
                                   + Mathf.Abs(neighbor.cellPosition.y - targetNode.cellPosition.y);

                    neighbor.parent = current;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.Log("Pathfinding exceeded max iterations, no path found");
        }
        else if (path == null)
        {
            Debug.Log("No path found");
        }
        return path;
    }

    List<Vector3Int> RetracePath(Node start, Node end)
    {
        Debug.Log("Retracing path");
        List<Vector3Int> path = new List<Vector3Int>();
        Node current = end;

        while (current != start)
        {
            path.Add(current.cellPosition);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    List<Node> GetNeighbors(Node node, Dictionary<Vector3Int, Node> allNodes)
    {
        List<Node> neighbors = new List<Node>();
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)
        };

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = node.cellPosition + dir;
            if (!allNodes.ContainsKey(neighborPos))
            {
                bool walkable = IsWalkable(neighborPos);
                allNodes[neighborPos] = new Node(neighborPos, walkable);
            }
            neighbors.Add(allNodes[neighborPos]);
        }
        return neighbors;
    }   

    public bool IsWalkable(Vector3Int cell)
    {
        TileBase tile = tilemap.GetTile(cell);
        return tile != null && tile.name.Contains("Earth");
    }
}
