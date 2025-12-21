using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Data
{
    /// <summary>
    /// GOOD EXAMPLE: ScriptableObject for immutable data
    ///
    /// Benefits:
    /// - Data separated from logic
    /// - Designer-friendly (edit in Inspector)
    /// - Memory efficient (shared across instances)
    /// - Easy to create variants (Fast Enemy, Slow Enemy, Boss)
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy", menuName = "ProjectName/Data/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique enemy identifier")]
        public string enemyID;

        [Tooltip("Enemy display name")]
        public string enemyName;

        [Header("Stats")]
        [Tooltip("Maximum health")]
        public int maxHealth = 100;

        [Tooltip("Movement speed")]
        public float moveSpeed = 3f;

        [Tooltip("Attack damage")]
        public int attackDamage = 10;

        [Tooltip("Attack range")]
        public float attackRange = 2f;

        [Header("Visual")]
        [Tooltip("Enemy prefab")]
        public GameObject prefab;

        [Tooltip("Enemy icon")]
        public Sprite icon;
    }

    /// <summary>
    /// GOOD EXAMPLE: ScriptableObject for runtime state
    ///
    /// Benefits:
    /// - Persistent state across scene loads
    /// - Easy to reset/clear
    /// - Inspector-visible during Play mode
    /// </summary>
    [CreateAssetMenu(fileName = "QuestProgress", menuName = "ProjectName/Data/Quest/QuestProgress")]
    public class QuestProgressSO : ScriptableObject
    {
        [Header("Current Quest")]
        public QuestSO currentQuest;

        [Header("Progress")]
        public int currentProgress;
        public float timeRemaining;
        public QuestState state;

        public void Initialize(QuestSO quest)
        {
            currentQuest = quest;
            currentProgress = 0;
            timeRemaining = quest.timeLimit;
            state = QuestState.InProgress;
        }

        public bool AddProgress(int amount = 1)
        {
            currentProgress += amount;

            if (currentProgress >= currentQuest.requiredCount)
            {
                state = QuestState.Success;
                return true;
            }

            return false;
        }

        public void Clear()
        {
            currentQuest = null;
            currentProgress = 0;
            timeRemaining = 0f;
            state = QuestState.Inactive;
        }
    }

    /// <summary>
    /// GOOD EXAMPLE: Logic class using ScriptableObject data
    ///
    /// Benefits:
    /// - Logic separated from data
    /// - Can swap different EnemyDataSO at runtime
    /// - Easy to test with mock data
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemyDataSO enemyData;

        private int currentHealth;

        private void Start()
        {
            // Initialize from ScriptableObject
            currentHealth = enemyData.maxHealth;
        }

        private void Update()
        {
            // Use data from ScriptableObject
            Move(enemyData.moveSpeed * Time.deltaTime);
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Move(float distance)
        {
            // Movement logic
        }

        private void Die()
        {
            // Death logic
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemyData == null)
                Debug.LogWarning($"[EnemyController] enemyData is not assigned on {gameObject.name}.", this);
        }
#endif
    }

    /// <summary>
    /// GOOD EXAMPLE: RuntimeSet ScriptableObject
    ///
    /// Benefits:
    /// - Centralized collection management
    /// - No FindObjectsOfType calls
    /// - Easy to iterate over active enemies
    /// - Inspector-visible during Play mode
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
    public class EnemyRuntimeSetSO : ScriptableObject
    {
        private List<EnemyController> items = new List<EnemyController>();

        /// <summary>
        /// Read-only access to the collection
        /// </summary>
        public IReadOnlyList<EnemyController> Items => items;

        /// <summary>
        /// Add an enemy to the set
        /// </summary>
        public void Add(EnemyController item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EnemyRuntimeSet] Attempted to add null enemy");
                return;
            }

            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Remove an enemy from the set
        /// </summary>
        public void Remove(EnemyController item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// Clear all enemies from the set
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Get the count of active enemies
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Get the closest enemy to a position
        /// </summary>
        public EnemyController GetClosestTo(Vector3 position)
        {
            EnemyController closest = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in items)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }
    }

    /// <summary>
    /// GOOD EXAMPLE: Enemy registers/unregisters with RuntimeSet
    /// </summary>
    public class EnemyRuntimeSetRegistration : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyController;
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        private void OnEnable()
        {
            // Register when enabled
            enemySet?.Add(enemyController);
        }

        private void OnDisable()
        {
            // Unregister when disabled
            enemySet?.Remove(enemyController);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemyController == null)
                enemyController = GetComponent<EnemyController>();

            if (enemySet == null)
                Debug.LogWarning($"[EnemyRuntimeSetRegistration] enemySet is not assigned on {gameObject.name}.", this);
        }
#endif
    }

    public enum QuestState
    {
        Inactive,
        InProgress,
        Success,
        Failed
    }
}
