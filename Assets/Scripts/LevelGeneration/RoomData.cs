using UnityEngine;

public enum RoomType
{
    Normal,
    Dark,
    Start,
    End
}

[CreateAssetMenu(fileName = "RoomData", menuName = "Horror Maze/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Room Identity")]
    public string roomID;
    public RoomType roomType;

    [Header("Prefabs")]
    public GameObject roomPrefab;

    [Header("Dark Room Settings")]
    [Tooltip("Максимальное время пребывания в тёмной комнате (в секундах)")]
    public float maxDarkTime = 10f;
    [Tooltip("Урон в секунду при нахождении в тёмной комнате")]
    public float darkDamagePerSecond = 5f;

    [Header("Connections")]
    public bool hasNorthDoor = true;
    public bool hasSouthDoor = true;
    public bool hasEastDoor  = true;
    public bool hasWestDoor  = true;

    [Header("Spawn Weight")]
    [Range(0f, 1f)]
    [Tooltip("Вероятность появления этой комнаты при генерации (0 = никогда, 1 = всегда)")]
    public float spawnWeight = 0.5f;
}
