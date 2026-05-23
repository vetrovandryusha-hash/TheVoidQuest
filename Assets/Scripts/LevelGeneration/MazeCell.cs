using UnityEngine;

/// <summary>
/// Хранит состояние одной ячейки в сетке лабиринта.
/// Не MonoBehaviour — чистые данные.
/// </summary>
public class MazeCell
{
    public Vector2Int GridPosition { get; }
    public RoomData    RoomData    { get; set; }
    public GameObject  Instance    { get; set; }

    // Флаги соединений с соседями (после генерации)
    public bool ConnectedNorth { get; set; }
    public bool ConnectedSouth { get; set; }
    public bool ConnectedEast  { get; set; }
    public bool ConnectedWest  { get; set; }

    public bool IsVisited { get; set; }

    public MazeCell(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }

    /// <summary>Возвращает противоположное направление.</summary>
    public static Direction Opposite(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East  => Direction.West,
            Direction.West  => Direction.East,
            _ => Direction.North
        };
    }

    public void SetConnected(Direction dir, bool value)
    {
        switch (dir)
        {
            case Direction.North: ConnectedNorth = value; break;
            case Direction.South: ConnectedSouth = value; break;
            case Direction.East:  ConnectedEast  = value; break;
            case Direction.West:  ConnectedWest  = value; break;
        }
    }

    public bool IsConnected(Direction dir)
    {
        return dir switch
        {
            Direction.North => ConnectedNorth,
            Direction.South => ConnectedSouth,
            Direction.East  => ConnectedEast,
            Direction.West  => ConnectedWest,
            _ => false
        };
    }
}

public enum Direction { North, South, East, West }
