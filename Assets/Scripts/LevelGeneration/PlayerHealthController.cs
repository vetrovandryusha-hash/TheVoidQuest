using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Простой контроллер здоровья игрока.
/// Подключается к системе тёмных комнат через RoomController.
/// </summary>
public class PlayerHealthController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;  // (current / max) нормализованное
    public UnityEvent        onDeath;

    public float CurrentHealth => _currentHealth;
    public bool  IsAlive       => _currentHealth > 0f;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    /// <summary>Наносит урон (вызывается из RoomController).</summary>
    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        onHealthChanged?.Invoke(_currentHealth / maxHealth);

        if (_currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        onHealthChanged?.Invoke(_currentHealth / maxHealth);
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died.");
        onDeath?.Invoke();
        // Здесь можно вызвать GameManager.Instance.OnPlayerDied() и т.п.
    }
}
