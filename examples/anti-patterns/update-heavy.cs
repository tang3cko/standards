using UnityEngine;
using Tang3cko.EventChannels;
using System.Collections.Generic;

namespace ProjectName.AntiPatterns
{
    /// <summary>
    /// ANTI-PATTERN: Heavy Update loop
    ///
    /// Problems:
    /// - UI updated every frame (expensive Canvas rebuild)
    /// - FindObjectsOfType called every frame (very expensive)
    /// - GetComponent called every frame (expensive)
    /// - Multiple Update loops doing similar work
    /// - Poor performance
    /// </summary>
    public class HeavyUpdateBad : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI healthText;
        [SerializeField] private TMPro.TextMeshProUGUI enemyCountText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        // ❌ BAD: Update every frame
        private void Update()
        {
            // ❌ BAD: FindObjectOfType every frame (very expensive)
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Canvas rebuild every frame
                healthText.text = $"HP: {playerHealth.currentHealth}";
            }

            // ❌ BAD: FindObjectsOfType every frame (extremely expensive)
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            enemyCountText.text = $"Enemies: {enemies.Length}";

            // ❌ BAD: Time.time updates every frame
            timeText.text = $"Time: {Time.time:F2}";

            // ❌ BAD: GetComponent every frame (expensive)
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Do something
            }
        }
    }

    /// <summary>
    /// ANTI-PATTERN: Multiple components polling same data
    ///
    /// Problems:
    /// - Each component does redundant work
    /// - Multiple Canvas rebuilds
    /// - No coordination
    /// </summary>
    public class HealthUIBad : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI healthText;

        private void Update()
        {
            // ❌ BAD: Polling every frame
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                healthText.text = $"{playerHealth.currentHealth}";
            }
        }
    }

    public class HealthBarBad : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image fillImage;

        private void Update()
        {
            // ❌ BAD: Same polling, different component
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                fillImage.fillAmount = playerHealth.currentHealth / 100f;
            }
        }
    }

    // ============================================================
    // GOOD ALTERNATIVE: EventChannel-driven updates
    // ============================================================

    /// <summary>
    /// GOOD: Event-driven UI updates
    ///
    /// Benefits:
    /// - Update only when values change
    /// - No FindObjectOfType calls
    /// - No redundant Canvas rebuilds
    /// - Better performance
    /// </summary>
    public class EventDrivenUIGood : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onHealthChanged;
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;
        [SerializeField] private FloatEventChannelSO onTimeUpdated;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI healthText;
        [SerializeField] private TMPro.TextMeshProUGUI enemyCountText;
        [SerializeField] private TMPro.TextMeshProUGUI timeText;

        private void OnEnable()
        {
            // ✅ GOOD: Subscribe to EventChannels
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised += UpdateHealth;

            if (onEnemyCountChanged != null)
                onEnemyCountChanged.OnEventRaised += UpdateEnemyCount;

            if (onTimeUpdated != null)
                onTimeUpdated.OnEventRaised += UpdateTime;
        }

        private void OnDisable()
        {
            // Always unsubscribe
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised -= UpdateHealth;

            if (onEnemyCountChanged != null)
                onEnemyCountChanged.OnEventRaised -= UpdateEnemyCount;

            if (onTimeUpdated != null)
                onTimeUpdated.OnEventRaised -= UpdateTime;
        }

        // ✅ GOOD: Update only when EventChannel fires
        private void UpdateHealth(int health)
        {
            healthText.text = $"HP: {health}";
        }

        private void UpdateEnemyCount(int count)
        {
            enemyCountText.text = $"Enemies: {count}";
        }

        private void UpdateTime(float time)
        {
            timeText.text = $"Time: {time:F2}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[EventDrivenUIGood] onHealthChanged is not assigned.", this);

            if (healthText == null)
                Debug.LogWarning($"[EventDrivenUIGood] healthText is not assigned.", this);
        }
#endif
    }

    /// <summary>
    /// GOOD: PlayerHealth raises EventChannel when health changes
    ///
    /// Benefits:
    /// - EventChannel fires only when health changes
    /// - No polling required
    /// - Multiple subscribers supported
    /// </summary>
    public class PlayerHealthGood : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onHealthChanged;

        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        public int CurrentHealth => currentHealth;

        private void Start()
        {
            currentHealth = maxHealth;
            // Notify initial value
            onHealthChanged?.RaiseEvent(currentHealth);
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            // ✅ GOOD: Raise EventChannel only when health changes
            onHealthChanged?.RaiseEvent(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Death logic
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[PlayerHealthGood] onHealthChanged is not assigned.", this);
        }
#endif
    }

    /// <summary>
    /// GOOD: RuntimeSet instead of FindObjectsOfType
    ///
    /// Benefits:
    /// - O(1) access instead of O(n) search
    /// - No performance overhead
    /// - Easy to iterate
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
    public class EnemyRuntimeSetSO : ScriptableObject
    {
        private List<Enemy> items = new List<Enemy>();

        public IReadOnlyList<Enemy> Items => items;
        public int Count => items.Count;

        public void Add(Enemy enemy)
        {
            if (!items.Contains(enemy))
                items.Add(enemy);
        }

        public void Remove(Enemy enemy)
        {
            items.Remove(enemy);
        }

        public void Clear()
        {
            items.Clear();
        }
    }

    /// <summary>
    /// GOOD: Enemy registers with RuntimeSet
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyRuntimeSetSO enemySet;
        [SerializeField] private VoidEventChannelSO onEnemyCountChanged;

        private void OnEnable()
        {
            // ✅ GOOD: Register with RuntimeSet
            enemySet?.Add(this);

            // Notify count changed
            onEnemyCountChanged?.RaiseEvent();
        }

        private void OnDisable()
        {
            // Unregister
            enemySet?.Remove(this);

            // Notify count changed
            onEnemyCountChanged?.RaiseEvent();
        }
    }

    /// <summary>
    /// GOOD: Cache component references
    ///
    /// Benefits:
    /// - GetComponent called once (in Start/Awake)
    /// - No repeated searches
    /// - Better performance
    /// </summary>
    public class ComponentCachingGood : MonoBehaviour
    {
        // ✅ GOOD: Cache references
        private Rigidbody rb;
        private Animator animator;
        private Collider col;

        private void Awake()
        {
            // Cache once
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            col = GetComponent<Collider>();
        }

        private void Update()
        {
            // ✅ GOOD: Use cached references
            if (rb != null)
            {
                rb.AddForce(Vector3.forward);
            }

            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }

        // ❌ BAD: Don't do this
        private void UpdateBad()
        {
            // GetComponent every frame (expensive!)
            GetComponent<Rigidbody>().AddForce(Vector3.forward);
            GetComponent<Animator>().SetBool("IsMoving", true);
        }
    }
}
