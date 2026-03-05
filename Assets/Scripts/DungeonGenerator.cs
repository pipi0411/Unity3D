using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public Vector2 size = new Vector2(10, 10);  // Grid size
    public Vector2 startPosition = Vector2.zero; // Start cell
    public GameObject room;                     // Room prefab with RoomBehavior
    public Vector2 offset = new Vector2(6, 6);  // Room spacing

    [Header("Generation Options")]
    public float fillPercent = 0.8f;            // % cells to fill (0.5-1.0 for varied sparsity)
    public bool autoGenerateOnStart = true;     // Auto gen on Play

    private List<Cell> board;

    [System.Serializable]
    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4]; // 0:Up, 1:Down, 2:Right, 3:Left
    }

    void Start()
    {
        if (autoGenerateOnStart)
        {
            GenerateNewDungeon();
        }
    }

    void Update()
    {
        // Regenerate on R key for testing different levels
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateNewDungeon();
        }
    }

    [ContextMenu("Generate New Dungeon")]  // Right-click in Inspector to test
    public void GenerateNewDungeon()
    {
        // Clear old rooms
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        MazeGenerator();
        InstantiateRooms();
    }

    void MazeGenerator()
    {
        board = new List<Cell>();
        int totalCells = (int)(size.x * size.y * fillPercent);

        // Init all cells
        for (int i = 0; i < size.x * size.y; i++)
        {
            board.Add(new Cell());
        }

        int current = GetIndexFromPos(startPosition);
        Stack<int> stack = new Stack<int>();

        board[current].visited = true;
        stack.Push(current);

        int steps = 0;
        while (stack.Count > 0 && steps < totalCells)
        {
            steps++;

            List<int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                int next = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWallBetween(current, next);
                stack.Push(next);
                board[next].visited = true;
                current = next;
            }
            else
            {
                current = stack.Pop();
            }
        }
    }

    void InstantiateRooms()
    {
        for (int j = 0; j < size.y; j++)
        {
            for (int i = 0; i < size.x; i++)
            {
                int index = i + j * (int)size.x;
                if (board[index].visited)
                {
                    Vector3 pos = new Vector3(i * offset.x, 0, j * offset.y);
                    GameObject newRoom = Instantiate(room, pos, Quaternion.identity, transform);
                    newRoom.name = $"Room ({i}, {j})";

                    RoomBehavior rb = newRoom.GetComponent<RoomBehavior>();
                    if (rb != null)
                    {
                        rb.UpdateRoom(board[index].status);
                    }
                }
            }
        }
    }

    List<int> GetUnvisitedNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        int x = cell % (int)size.x;
        int y = cell / (int)size.x;

        // Up
        if (y > 0 && !board[cell - (int)size.x].visited)
            neighbors.Add(cell - (int)size.x);

        // Down
        if (y < size.y - 1 && !board[cell + (int)size.x].visited)
            neighbors.Add(cell + (int)size.x);

        // Right
        if (x < size.x - 1 && !board[cell + 1].visited)
            neighbors.Add(cell + 1);

        // Left
        if (x > 0 && !board[cell - 1].visited)
            neighbors.Add(cell - 1);

        return neighbors;
    }

    void RemoveWallBetween(int current, int next)
    {
        int dx = next % (int)size.x - current % (int)size.x;
        int dy = next / (int)size.x - current / (int)size.x;

        if (dx == 1)  // Right
        {
            board[current].status[2] = true;
            board[next].status[3] = true;
        }
        else if (dx == -1) // Left
        {
            board[current].status[3] = true;
            board[next].status[2] = true;
        }
        else if (dy == 1) // Down
        {
            board[current].status[1] = true;
            board[next].status[0] = true;
        }
        else if (dy == -1) // Up
        {
            board[current].status[0] = true;
            board[next].status[1] = true;
        }
    }

    int GetIndexFromPos(Vector2 pos)
    {
        return (int)pos.x + (int)pos.y * (int)size.x;
    }
}