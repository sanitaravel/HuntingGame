using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LynxController : MonoBehaviour
{
    public Tilemap tilemap;
    public Pathfinder pathfinder;
    public float moveSpeed = 3f;
    public int maxCells = 7; // Maximum number of cells the Lynx can travel
    public Tilemap highlightTilemap; // Tilemap for highlighting reachable tiles
    public TileBase highlightTile; // Tile to use for highlighting
    public TurnManager turnManager;

    private List<Vector3Int> path;
    private int currentTileIndex = 0;
    private Vector3Int previousCell;
    private bool isMoving = false;
    private bool canMove = false;

    void Update()
    {
        if (canMove && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            Vector3Int startCell = tilemap.WorldToCell(transform.position);
            Vector3Int targetCell = tilemap.WorldToCell(mouseWorldPos);

            path = pathfinder.FindPath(startCell, targetCell);
            if (path != null && path.Count <= maxCells + 1)
            {
                currentTileIndex = 0;
                isMoving = true;
                canMove = false;
                Debug.Log("Path calculated: " + path.Count + " steps");
            }
            else
            {
                path = null;
                Debug.Log("Cell is too far");
            }
        }

        if (path != null && currentTileIndex < path.Count)
        {
            Vector3 target = tilemap.GetCellCenterWorld(path[currentTileIndex]);
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                currentTileIndex++;
            }
        }

        // Check if movement has stopped
        if (isMoving && path != null && currentTileIndex >= path.Count)
        {
            isMoving = false;
            HighlightReachableTiles();
            turnManager.EndLynxTurn();
        }
    }

    void Awake()
    {
        previousCell = tilemap.WorldToCell(transform.position);
        if (turnManager != null)
            turnManager.RegisterLynx(this);
    }

    public void StartTurn()
    {
        canMove = true;
        HighlightReachableTiles();
    }

    void HighlightReachableTiles()
    {
        if (highlightTilemap == null || highlightTile == null) return;

        highlightTilemap.ClearAllTiles();

        Vector3Int startCell = tilemap.WorldToCell(transform.position);

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, int> distance = new Dictionary<Vector3Int, int>();

        queue.Enqueue(startCell);
        visited.Add(startCell);
        distance[startCell] = 0;

        while (queue.Count > 0)
        {
            Vector3Int cell = queue.Dequeue();
            int dist = distance[cell];

            if (dist < maxCells)
            {
                Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
                foreach (Vector3Int dir in directions)
                {
                    Vector3Int neighbor = cell + dir;
                    if (!visited.Contains(neighbor) && pathfinder.IsWalkable(neighbor))
                    {
                        visited.Add(neighbor);
                        distance[neighbor] = dist + 1;
                        queue.Enqueue(neighbor);
                        highlightTilemap.SetTile(neighbor, highlightTile);
                    }
                }
            }
        }
    }
}
