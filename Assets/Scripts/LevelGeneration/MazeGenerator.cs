using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Процедурный генератор лабиринта на основе DFS (Recursive Backtracker).
/// Поддерживает два режима: полная сетка (лабиринт) и линейный коридор.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    // ─── Inspector ──────────────────────────────────────────────────────────
    [Header("Grid Size")]
    [Min(3)] public int gridWidth  = 7;
    [Min(3)] public int gridHeight = 7;

    [Header("Room Prefabs")]
    public RoomData startRoomData;
    public RoomData endRoomData;
    public List<RoomData> normalRooms;
    public List<RoomData> darkRooms;

    [Header("Room Placement")]
    [Tooltip("Размер одной ячейки в мировых единицах")]
    public Vector3 cellSize = new Vector3(20f, 0f, 20f);

    [Header("Generation Mode")]
    [Tooltip("Linear = длинный коридор с развилками; Maze = полная сетка")]
    public GenerationMode mode = GenerationMode.Maze;

    [Header("Corridor Mode")]
    [Tooltip("Минимальная длина основного пути")]
    [Min(5)] public int minPathLength = 10;
    [Tooltip("Вероятность добавить ответвление от основного пути")]
    [Range(0f, 1f)] public float branchProbability = 0.3f;

    [Header("Dark Room Settings")]
    [Range(0f, 1f)]
    [Tooltip("Доля тёмных комнат (0 = нет, 1 = все)")]
    public float darkRoomRatio = 0.25f;

    [Header("Seed")]
    [Tooltip("0 = случайный сид каждый раз")]
    public int seed = 0;

    [Header("Async Generation")]
    public bool generateAsync = true;
    [Tooltip("Кадров между размещением комнат (для плавной загрузки)")]
    public int framesPerRoom = 0;

    // ─── Events ─────────────────────────────────────────────────────────────
    public event System.Action<MazeCell[,]> OnMazeGenerated;
    public event System.Action              OnMazeCleared;

    // ─── State ──────────────────────────────────────────────────────────────
    private MazeCell[,] _grid;
    private List<MazeCell> _mainPath = new();
    private System.Random _rng;

    public MazeCell[,] Grid       => _grid;
    public List<MazeCell> MainPath => _mainPath;

    // ════════════════════════════════════════════════════════════════════════
    //  Public API
    // ════════════════════════════════════════════════════════════════════════

    [ContextMenu("Generate Maze")]
    public void Generate()
    {
        ClearMaze();
        InitRandom();

        _grid = new MazeCell[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
            for (int z = 0; z < gridHeight; z++)
                _grid[x, z] = new MazeCell(new Vector2Int(x, z));

        if (mode == GenerationMode.Maze)
            GenerateFullMaze();
        else
            GenerateCorridorMaze();

        if (generateAsync)
            StartCoroutine(PlaceRoomsAsync());
        else
            PlaceRoomsSync();
    }

    [ContextMenu("Clear Maze")]
    public void ClearMaze()
    {
        // Уничтожить все порождённые объекты
        if (_grid != null)
        {
            foreach (var cell in _grid)
                if (cell?.Instance != null)
                    Destroy(cell.Instance);
            _grid = null;
        }

        _mainPath.Clear();
        OnMazeCleared?.Invoke();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Generation Algorithms
    // ════════════════════════════════════════════════════════════════════════

    /// Полный лабиринт через DFS с возвратом.
    private void GenerateFullMaze()
    {
        var stack = new Stack<MazeCell>();
        var start = _grid[0, 0];
        start.IsVisited = true;
        stack.Push(start);
        _mainPath.Add(start);

        MazeCell furthest = start;
        int maxDist = 0;
        var dist = new Dictionary<MazeCell, int> { [start] = 0 };

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var neighbours = GetUnvisitedNeighbours(current);

            if (neighbours.Count > 0)
            {
                var (next, dir) = neighbours[_rng.Next(neighbours.Count)];
                ConnectCells(current, next, dir);
                next.IsVisited = true;
                stack.Push(next);

                int d = dist[current] + 1;
                dist[next] = d;
                if (d > maxDist) { maxDist = d; furthest = next; }
            }
            else
            {
                stack.Pop();
            }
        }

        // Определяем старт/финиш: самые удалённые ячейки
        AssignStartEnd(start, furthest);
        BuildMainPath(start, furthest, dist);
        AssignRoomTypes();
    }

    /// Длинный коридор: сначала прокладывается гарантированный путь,
    /// потом добавляются случайные ответвления.
    private void GenerateCorridorMaze()
    {
        // Шаг 1: случайное блуждание до достижения нужной длины или тупика
        var path = new List<MazeCell>();
        var current = _grid[0, 0];
        current.IsVisited = true;
        path.Add(current);

        int maxIterations = gridWidth * gridHeight * 4;
        int iter = 0;

        while (path.Count < minPathLength && iter++ < maxIterations)
        {
            var next = GetRandomUnvisitedNeighbour(current);
            if (next.HasValue)
            {
                ConnectCells(current, next.Value.cell, next.Value.dir);
                next.Value.cell.IsVisited = true;
                current = next.Value.cell;
                path.Add(current);
            }
            else
            {
                // Тупик — откат
                if (path.Count > 1)
                {
                    path.RemoveAt(path.Count - 1);
                    current = path[^1];
                }
                else break;
            }
        }

        _mainPath = new List<MazeCell>(path);

        // Шаг 2: ответвления
        if (branchProbability > 0f)
            AddBranches(path);

        AssignStartEnd(path[0], path[^1]);
        AssignRoomTypes();
    }

    private void AddBranches(List<MazeCell> mainPath)
    {
        foreach (var cell in mainPath)
        {
            if ((float)_rng.NextDouble() > branchProbability) continue;

            var current = cell;
            int branchLen = _rng.Next(2, Mathf.Max(3, gridWidth / 2));

            for (int i = 0; i < branchLen; i++)
            {
                var next = GetRandomUnvisitedNeighbour(current);
                if (!next.HasValue) break;
                ConnectCells(current, next.Value.cell, next.Value.dir);
                next.Value.cell.IsVisited = true;
                current = next.Value.cell;
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Room Type Assignment
    // ════════════════════════════════════════════════════════════════════════

    private void AssignStartEnd(MazeCell start, MazeCell end)
    {
        start.RoomData = startRoomData;
        end.RoomData   = endRoomData;
    }

    private void AssignRoomTypes()
    {
        // Собираем все ячейки без RoomData
        var unassigned = new List<MazeCell>();
        foreach (var cell in _grid)
            if (cell != null && cell.IsVisited && cell.RoomData == null)
                unassigned.Add(cell);

        int totalUnassigned = unassigned.Count;
        int darkCount = Mathf.RoundToInt(totalUnassigned * darkRoomRatio);

        // Перемешиваем и раздаём тёмные комнаты
        Shuffle(unassigned);
        for (int i = 0; i < unassigned.Count; i++)
        {
            bool isDark = i < darkCount && darkRooms.Count > 0;
            unassigned[i].RoomData = isDark
                ? WeightedRandom(darkRooms)
                : WeightedRandom(normalRooms);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Room Placement
    // ════════════════════════════════════════════════════════════════════════

    private void PlaceRoomsSync()
    {
        foreach (var cell in _grid)
        {
            if (cell == null || !cell.IsVisited || cell.RoomData?.roomPrefab == null) continue;
            PlaceRoom(cell);
        }
        OnMazeGenerated?.Invoke(_grid);
    }

    private IEnumerator PlaceRoomsAsync()
    {
        int count = 0;
        foreach (var cell in _grid)
        {
            if (cell == null || !cell.IsVisited || cell.RoomData?.roomPrefab == null) continue;
            PlaceRoom(cell);

            count++;
            if (framesPerRoom > 0 && count % Mathf.Max(1, framesPerRoom) == 0)
                yield return null;
        }
        OnMazeGenerated?.Invoke(_grid);
    }

    private void PlaceRoom(MazeCell cell)
    {
        Vector3 worldPos = GridToWorld(cell.GridPosition);
        var instance = Instantiate(cell.RoomData.roomPrefab, worldPos, Quaternion.identity, transform);
        instance.name = $"Room_{cell.GridPosition.x}_{cell.GridPosition.y}_{cell.RoomData.roomType}";
        cell.Instance = instance;

        // Передаём данные в компонент комнаты, если он есть
        if (instance.TryGetComponent<RoomController>(out var ctrl))
            ctrl.Initialize(cell);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════════════

    private void ConnectCells(MazeCell a, MazeCell b, Direction dir)
    {
        a.SetConnected(dir, true);
        b.SetConnected(MazeCell.Opposite(dir), true);
    }

    private List<(MazeCell cell, Direction dir)> GetUnvisitedNeighbours(MazeCell cell)
    {
        var result = new List<(MazeCell, Direction)>();
        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            var pos = cell.GridPosition + DirToOffset(dir);
            if (InBounds(pos) && !_grid[pos.x, pos.y].IsVisited)
                result.Add((_grid[pos.x, pos.y], dir));
        }
        return result;
    }

    private (MazeCell cell, Direction dir)? GetRandomUnvisitedNeighbour(MazeCell cell)
    {
        var list = GetUnvisitedNeighbours(cell);
        if (list.Count == 0) return null;
        return list[_rng.Next(list.Count)];
    }

    private void BuildMainPath(MazeCell start, MazeCell end, Dictionary<MazeCell, int> dist)
    {
        // BFS-трассировка от end к start по убыванию dist
        _mainPath.Clear();
        var current = end;
        var visited = new HashSet<MazeCell>();

        while (current != start)
        {
            _mainPath.Add(current);
            visited.Add(current);

            MazeCell best = null;
            int bestDist = int.MaxValue;

            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                if (!current.IsConnected(dir)) continue;
                var pos = current.GridPosition + DirToOffset(dir);
                if (!InBounds(pos)) continue;
                var neighbour = _grid[pos.x, pos.y];
                if (visited.Contains(neighbour)) continue;
                if (dist.TryGetValue(neighbour, out int d) && d < bestDist)
                { bestDist = d; best = neighbour; }
            }

            if (best == null) break;
            current = best;
        }

        _mainPath.Add(start);
        _mainPath.Reverse();
    }

    private Vector3 GridToWorld(Vector2Int pos)
        => new Vector3(pos.x * cellSize.x, 0f, pos.y * cellSize.z);

    private static Vector2Int DirToOffset(Direction dir) => dir switch
    {
        Direction.North => Vector2Int.up,
        Direction.South => Vector2Int.down,
        Direction.East  => Vector2Int.right,
        Direction.West  => Vector2Int.left,
        _ => Vector2Int.zero
    };

    private bool InBounds(Vector2Int pos)
        => pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;

    private void InitRandom()
        => _rng = seed == 0 ? new System.Random() : new System.Random(seed);

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private RoomData WeightedRandom(List<RoomData> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        float total = pool.Sum(r => r.spawnWeight);
        if (total <= 0f) return pool[_rng.Next(pool.Count)];

        float pick = (float)_rng.NextDouble() * total;
        float acc = 0f;
        foreach (var r in pool)
        {
            acc += r.spawnWeight;
            if (pick <= acc) return r;
        }
        return pool[^1];
    }
}

public enum GenerationMode { Maze, Corridor }
