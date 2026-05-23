using UnityEngine;

/// <summary>
/// Рисует карту лабиринта прямо в сцене (только в редакторе).
/// Прикрепить на тот же объект, что и MazeGenerator.
/// </summary>
[RequireComponent(typeof(MazeGenerator))]
public class MazeDebugVisualizer : MonoBehaviour
{
    [Header("Gizmo Colours")]
    public Color normalRoomColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    public Color darkRoomColor   = new Color(0.1f, 0.1f, 0.5f, 0.7f);
    public Color startColor      = Color.green;
    public Color endColor        = Color.red;
    public Color pathColor       = new Color(1f, 0.8f, 0f, 0.9f);
    public Color connectionColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Options")]
    public bool showConnections = true;
    public bool showMainPath    = true;
    public bool showRoomTypes   = true;
    public float cellDisplaySize = 2f;

    private MazeGenerator _gen;

    private void Awake()  => _gen = GetComponent<MazeGenerator>();
    private void OnValidate() => _gen = GetComponent<MazeGenerator>();

    private void OnDrawGizmos()
    {
        if (_gen == null) _gen = GetComponent<MazeGenerator>();
        if (_gen?.Grid == null) return;

        var grid = _gen.Grid;
        var path = _gen.MainPath;

        for (int x = 0; x < _gen.gridWidth; x++)
        {
            for (int z = 0; z < _gen.gridHeight; z++)
            {
                var cell = grid[x, z];
                if (cell == null || !cell.IsVisited) continue;

                Vector3 worldPos = transform.position
                    + new Vector3(x * _gen.cellSize.x, 0f, z * _gen.cellSize.z);

                // Цвет комнаты
                if (showRoomTypes)
                {
                    Gizmos.color = cell.RoomData?.roomType switch
                    {
                        RoomType.Start  => startColor,
                        RoomType.End    => endColor,
                        RoomType.Dark   => darkRoomColor,
                        _               => normalRoomColor
                    };
                    Gizmos.DrawCube(worldPos, Vector3.one * cellDisplaySize * 0.8f);
                }

                // Соединения
                if (showConnections)
                {
                    Gizmos.color = connectionColor;
                    if (cell.ConnectedNorth) DrawConnection(worldPos, worldPos + new Vector3(0, 0,  _gen.cellSize.z));
                    if (cell.ConnectedEast)  DrawConnection(worldPos, worldPos + new Vector3( _gen.cellSize.x, 0, 0));
                }
            }
        }

        // Основной путь
        if (showMainPath && path != null && path.Count > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 a = transform.position + new Vector3(path[i].GridPosition.x     * _gen.cellSize.x, 0.5f, path[i].GridPosition.y     * _gen.cellSize.z);
                Vector3 b = transform.position + new Vector3(path[i+1].GridPosition.x   * _gen.cellSize.x, 0.5f, path[i+1].GridPosition.y   * _gen.cellSize.z);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawSphere(a, cellDisplaySize * 0.25f);
            }
            Gizmos.DrawSphere(transform.position + new Vector3(path[^1].GridPosition.x * _gen.cellSize.x, 0.5f, path[^1].GridPosition.y * _gen.cellSize.z), cellDisplaySize * 0.3f);
        }
    }

    private static void DrawConnection(Vector3 from, Vector3 to)
    {
        Gizmos.DrawLine(from + Vector3.up * 0.1f, to + Vector3.up * 0.1f);
    }
}
