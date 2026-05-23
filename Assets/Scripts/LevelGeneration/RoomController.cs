using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Размещается на префабе каждой комнаты.
/// Управляет: активацией дверных проёмов, эффектами тьмы, событиями входа/выхода.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomController : MonoBehaviour
{
    // ─── Inspector ──────────────────────────────────────────────────────────
    [Header("Doors (assign in prefab — can be null)")]
    public GameObject doorNorth;
    public GameObject doorSouth;
    public GameObject doorEast;
    public GameObject doorWest;

    [Header("Dark Room FX")]
    public Light roomLight;
    [Tooltip("Renderer для тумана/оверлея темноты")]
    public Renderer darkFogRenderer;
    [Tooltip("Звук предупреждения при нахождении в тёмной комнате")]
    public AudioSource warningAudio;

    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    public UnityEvent onDarkTimerExpired;

    // ─── State ──────────────────────────────────────────────────────────────
    private MazeCell _cell;
    private bool _playerInside;
    private float _darkTimer;
    private Coroutine _darkCoroutine;

    // ─── Public ─────────────────────────────────────────────────────────────

    /// <summary>Вызывается MazeGenerator после размещения комнаты.</summary>
    public void Initialize(MazeCell cell)
    {
        _cell = cell;
        ConfigureDoors();
        ConfigureVisuals();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Door Configuration
    // ════════════════════════════════════════════════════════════════════════

    private void ConfigureDoors()
    {
        SetDoor(doorNorth, _cell.ConnectedNorth && (_cell.RoomData?.hasNorthDoor ?? true));
        SetDoor(doorSouth, _cell.ConnectedSouth && (_cell.RoomData?.hasSouthDoor ?? true));
        SetDoor(doorEast,  _cell.ConnectedEast  && (_cell.RoomData?.hasEastDoor  ?? true));
        SetDoor(doorWest,  _cell.ConnectedWest  && (_cell.RoomData?.hasWestDoor  ?? true));
    }

    /// <param name="open">true = открыт (стена убрана), false = закрыт (стена стоит)</param>
    private static void SetDoor(GameObject door, bool open)
    {
        if (door == null) return;
        door.SetActive(!open); // Объект-стена активен = дверь закрыта
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Visual Setup
    // ════════════════════════════════════════════════════════════════════════

    private void ConfigureVisuals()
    {
        bool isDark = _cell.RoomData?.roomType == RoomType.Dark;

        if (roomLight != null)
        {
            roomLight.intensity = isDark ? 0.05f : 1f;
            roomLight.color     = isDark ? new Color(0.1f, 0.05f, 0.15f) : Color.white;
        }

        if (darkFogRenderer != null)
            darkFogRenderer.enabled = isDark;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Player Detection
    // ════════════════════════════════════════════════════════════════════════

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        onPlayerEnter?.Invoke();

        if (_cell?.RoomData?.roomType == RoomType.Dark)
            StartDarkTimer(other.GetComponent<PlayerHealthController>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        StopDarkTimer();
        onPlayerExit?.Invoke();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Dark Room Mechanics
    // ════════════════════════════════════════════════════════════════════════

    private void StartDarkTimer(PlayerHealthController health)
    {
        if (_darkCoroutine != null) StopCoroutine(_darkCoroutine);
        _darkCoroutine = StartCoroutine(DarkRoomRoutine(health));
    }

    private void StopDarkTimer()
    {
        if (_darkCoroutine != null)
        {
            StopCoroutine(_darkCoroutine);
            _darkCoroutine = null;
        }
        _darkTimer = 0f;
        SetWarningAudio(false);
    }

    private IEnumerator DarkRoomRoutine(PlayerHealthController health)
    {
        var data  = _cell.RoomData;
        _darkTimer = 0f;

        // Фаза 1 — игрок находится в норме, таймер тикает
        while (_darkTimer < data.maxDarkTime && _playerInside)
        {
            _darkTimer += Time.deltaTime;

            // Предупреждение в последней трети времени
            float warningThreshold = data.maxDarkTime * 0.6f;
            if (_darkTimer >= warningThreshold)
                SetWarningAudio(true);

            yield return null;
        }

        if (!_playerInside) yield break;

        // Фаза 2 — время истекло, наносим урон пока игрок внутри
        onDarkTimerExpired?.Invoke();
        SetWarningAudio(false);

        while (_playerInside)
        {
            health?.TakeDamage(data.darkDamagePerSecond * Time.deltaTime);
            yield return null;
        }
    }

    private void SetWarningAudio(bool play)
    {
        if (warningAudio == null) return;
        if (play && !warningAudio.isPlaying) warningAudio.Play();
        if (!play && warningAudio.isPlaying) warningAudio.Stop();
    }

    // ─── Debug ──────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (_cell == null) return;

        Gizmos.color = _cell.RoomData?.roomType switch
        {
            RoomType.Dark  => Color.blue,
            RoomType.Start => Color.green,
            RoomType.End   => Color.red,
            _              => Color.white
        };

        Gizmos.DrawWireCube(transform.position, new Vector3(18f, 3f, 18f));
    }
}
