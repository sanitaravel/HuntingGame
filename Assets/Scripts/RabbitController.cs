using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RabbitController : MonoBehaviour
{
    public Tilemap tilemap;
    public Pathfinder pathfinder;
    public float moveSpeed = 3f;
    public int roamRadius = 3; // Radius for roaming in calm state
    public TurnManager turnManager;
    public Transform lynxTransform; // Reference to Lynx transform
    public Tilemap highlightTilemap; // Tilemap for highlighting roam zone
    public TileBase highlightTile; // Tile to use for highlighting
    public bool canHighlight = false;

    private List<Vector3Int> path;
    private int currentTileIndex = 0;
    private bool isMoving = false;
    public enum State { Calm, Escape }
    public State CurrentState { get; private set; } = State.Calm;
    private Vector3Int homeCell;
    private Dictionary<Vector3Int, TileBase> previousTiles = new Dictionary<Vector3Int, TileBase>();

    void Awake()
    {
        homeCell = tilemap.WorldToCell(transform.position);
        if (turnManager != null)
            turnManager.RegisterRabbit(this);
    }

    void Update()
    {
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
            turnManager.EndRabbitTurn();
        }
    }

    public void StartTurn()
    {
        // Check distance to Lynx
        if (lynxTransform != null)
        {
            Vector3Int lynxCell = tilemap.WorldToCell(lynxTransform.position);
            Vector3Int rabbitCell = tilemap.WorldToCell(transform.position);
            int distance = Mathf.Abs(lynxCell.x - rabbitCell.x) + Mathf.Abs(lynxCell.y - rabbitCell.y);

            if (distance <= roamRadius)
            {
                CurrentState = State.Escape;
            }
            else
            {
                if (CurrentState == State.Escape)
                {
                    // Transitioning to Calm, update home
                    homeCell = rabbitCell;
                }
                CurrentState = State.Calm;
            }
        }

        if (CurrentState == State.Calm)
        {
            Roam();
        }
        else if (CurrentState == State.Escape)
        {
            Escape();
        }
    }

    void Roam()
    {
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        List<Vector3Int> reachableCells = GetReachableCells(currentCell, roamRadius);

        // Filter to cells within roamRadius of homeCell
        List<Vector3Int> validCells = new List<Vector3Int>();
        foreach (Vector3Int cell in reachableCells)
        {
            int distToHome = Mathf.Abs(cell.x - homeCell.x) + Mathf.Abs(cell.y - homeCell.y);
            if (distToHome <= roamRadius)
            {
                validCells.Add(cell);
            }
        }

        if (validCells.Count > 0)
        {
            Vector3Int targetCell = validCells[Random.Range(0, validCells.Count)];
            path = pathfinder.FindPath(currentCell, targetCell);
            if (path != null)
            {
                currentTileIndex = 0;
                isMoving = true;
                Debug.Log("Rabbit roaming to: " + targetCell + " (home: " + homeCell + ")");
            }
            else
            {
                // Can't find path, end turn
                turnManager.EndRabbitTurn();
            }
        }
        else
        {
            // No valid cells, end turn
            turnManager.EndRabbitTurn();
        }
    }

    void Escape()
    {
        Vector3Int startCell = tilemap.WorldToCell(transform.position);
        Vector3Int lynxCell = tilemap.WorldToCell(lynxTransform.position);
        List<Vector3Int> reachableCells = GetReachableCells(startCell, roamRadius);

        if (reachableCells.Count > 0)
        {
            List<Vector3Int> candidates = new List<Vector3Int>();
            int maxDistance = 0;

            foreach (Vector3Int cell in reachableCells)
            {
                int distance = Mathf.Abs(cell.x - lynxCell.x) + Mathf.Abs(cell.y - lynxCell.y);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    candidates.Clear();
                    candidates.Add(cell);
                }
                else if (distance == maxDistance)
                {
                    candidates.Add(cell);
                }
            }

            Vector3Int bestCell = candidates[Random.Range(0, candidates.Count)];

            path = pathfinder.FindPath(startCell, bestCell);
            if (path != null)
            {
                currentTileIndex = 0;
                isMoving = true;
                Debug.Log("Rabbit escaping to: " + bestCell);
            }
            else
            {
                // Can't find path, end turn
                turnManager.EndRabbitTurn();
            }
        }
        else
        {
            // No cells to move, end turn
            turnManager.EndRabbitTurn();
        }
    }

    List<Vector3Int> GetReachableCells(Vector3Int startCell, int maxDistance)
    {
        List<Vector3Int> reachable = new List<Vector3Int>();
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

            if (dist > 0 && dist <= maxDistance) // dist > 0 to exclude start cell
            {
                reachable.Add(cell);
            }

            if (dist < maxDistance)
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
                    }
                }
            }
        }

        return reachable;
    }

    void OnMouseEnter()
    {
        if (!canHighlight) return;

        // Save current highlights
        previousTiles.Clear();
        BoundsInt bounds = highlightTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (highlightTilemap.HasTile(cell))
                {
                    previousTiles[cell] = highlightTilemap.GetTile(cell);
                }
            }
        }
        HighlightRoamZone();
    }

    void OnMouseExit()
    {
        if (highlightTilemap != null)
        {
            highlightTilemap.ClearAllTiles();
            foreach (var kvp in previousTiles)
            {
                highlightTilemap.SetTile(kvp.Key, kvp.Value);
            }
            previousTiles.Clear();
        }
    }

    void HighlightRoamZone()
    {
        if (highlightTilemap == null || highlightTile == null) return;

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

            if (dist < roamRadius)
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