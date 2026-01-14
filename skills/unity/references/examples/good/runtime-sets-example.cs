using UnityEngine;
using System.Collections.Generic;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// GOOD EXAMPLE: Custom RuntimeSet for enemy tracking
    ///
    /// Benefits:
    /// - O(1) access to all enemies (no FindObjectsOfType)
    /// - Self-registering collection (objects manage themselves)
    /// - Custom query methods for game logic
    /// - EventChannel notifications for UI
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/Runtime Sets/Enemy")]
    public class EnemyRuntimeSetSO : RuntimeSetSO<EnemyController>
    {
        /// <summary>
        /// Get the closest enemy to a position
        /// </summary>
        public EnemyController GetClosestTo(Vector3 position)
        {
            EnemyController closest = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (var enemy in Items)
            {
                if (enemy == null) continue;

                float distanceSqr = (enemy.transform.position - position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closest = enemy;
                }
            }

            return closest;
        }

        /// <summary>
        /// Get all enemies within radius
        /// </summary>
        public List<EnemyController> GetEnemiesInRadius(Vector3 center, float radius)
        {
            var result = new List<EnemyController>();
            float radiusSqr = radius * radius;

            foreach (var enemy in Items)
            {
                if (enemy == null) continue;

                if ((enemy.transform.position - center).sqrMagnitude <= radiusSqr)
                {
                    result.Add(enemy);
                }
            }

            return result;
        }

        /// <summary>
        /// Get enemies by team
        /// </summary>
        public List<EnemyController> GetEnemiesByTeam(int teamId)
        {
            var result = new List<EnemyController>();

            foreach (var enemy in Items)
            {
                if (enemy == null) continue;
                if (enemy.TeamId == teamId)
                {
                    result.Add(enemy);
                }
            }

            return result;
        }
    }
}

namespace ProjectName.Enemy
{
    /// <summary>
    /// GOOD EXAMPLE: Enemy self-registers to RuntimeSet
    ///
    /// Benefits:
    /// - Automatic registration/unregistration
    /// - No central manager required
    /// - Works with object pooling
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("RuntimeSet")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        [Header("Enemy Settings")]
        [SerializeField] private int teamId;

        public int TeamId => teamId;

        private void OnEnable()
        {
            // Self-register when enabled
            enemySet?.Add(this);
        }

        private void OnDisable()
        {
            // Self-unregister when disabled (works with pooling)
            enemySet?.Remove(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null)
                Debug.LogWarning($"[EnemyController] enemySet is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}

namespace ProjectName.AI
{
    /// <summary>
    /// GOOD EXAMPLE: AI system uses RuntimeSet for targeting
    ///
    /// Benefits:
    /// - No FindObjectsOfType (O(1) access)
    /// - Always up-to-date list
    /// - Custom query methods available
    /// </summary>
    public class AITargetingSystem : MonoBehaviour
    {
        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        [Header("Settings")]
        [SerializeField] private float detectionRadius = 10f;

        /// <summary>
        /// Find best target for attacking
        /// </summary>
        public EnemyController FindBestTarget(Vector3 position)
        {
            // Use RuntimeSet query methods instead of FindObjectsOfType
            var nearbyEnemies = enemySet.GetEnemiesInRadius(position, detectionRadius);

            if (nearbyEnemies.Count == 0)
                return null;

            // Find lowest health enemy
            EnemyController bestTarget = null;
            float lowestHealth = float.MaxValue;

            foreach (var enemy in nearbyEnemies)
            {
                // Assume enemy has Health property
                float health = GetEnemyHealth(enemy);
                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }

        private float GetEnemyHealth(EnemyController enemy)
        {
            // Implementation would get health from enemy
            return 100f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null)
                Debug.LogWarning($"[AITargetingSystem] enemySet is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.UI
{
    /// <summary>
    /// GOOD EXAMPLE: UI subscribes to RuntimeSet changes via EventChannel
    ///
    /// Benefits:
    /// - Event-driven updates (not polling)
    /// - Decoupled from enemy spawning
    /// - Shows current count at any time
    /// </summary>
    public class EnemyCountUI : MonoBehaviour
    {
        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;

        [Header("UI")]
        [SerializeField] private TMPro.TextMeshProUGUI countText;

        private void OnEnable()
        {
            // Subscribe to count change EventChannel
            if (onEnemyCountChanged != null)
                onEnemyCountChanged.OnEventRaised += UpdateDisplay;

            // Initial update
            UpdateDisplay(enemySet.Count);
        }

        private void OnDisable()
        {
            if (onEnemyCountChanged != null)
                onEnemyCountChanged.OnEventRaised -= UpdateDisplay;
        }

        private void UpdateDisplay(int count)
        {
            countText.text = $"Enemies: {count}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null)
                Debug.LogWarning($"[EnemyCountUI] enemySet is not assigned.", this);

            if (onEnemyCountChanged == null)
                Debug.LogWarning($"[EnemyCountUI] onEnemyCountChanged is not assigned.", this);

            if (countText == null)
                Debug.LogWarning($"[EnemyCountUI] countText is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.Core
{
    /// <summary>
    /// GOOD EXAMPLE: Wave manager monitors RuntimeSet for victory condition
    ///
    /// Benefits:
    /// - Clear separation of concerns
    /// - Event-driven victory detection
    /// - Easy to test
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        [Header("Event Channels - Input")]
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;

        [Header("Event Channels - Output")]
        [SerializeField] private VoidEventChannelSO onWaveCompleted;

        private bool waveActive = false;

        private void OnEnable()
        {
            if (onEnemyCountChanged != null)
                onEnemyCountChanged.OnEventRaised += CheckWaveCompletion;
        }

        private void OnDisable()
        {
            if (onEnemyCountChanged != null)
                onEnemyCountChanged.OnEventRaised -= CheckWaveCompletion;
        }

        public void StartWave()
        {
            waveActive = true;
        }

        private void CheckWaveCompletion(int enemyCount)
        {
            if (waveActive && enemyCount == 0)
            {
                waveActive = false;
                onWaveCompleted?.RaiseEvent();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enemySet == null)
                Debug.LogWarning($"[WaveManager] enemySet is not assigned.", this);

            if (onEnemyCountChanged == null)
                Debug.LogWarning($"[WaveManager] onEnemyCountChanged is not assigned.", this);

            if (onWaveCompleted == null)
                Debug.LogWarning($"[WaveManager] onWaveCompleted is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.Core
{
    /// <summary>
    /// GOOD EXAMPLE: Scene cleanup clears RuntimeSets
    ///
    /// Benefits:
    /// - Prevents stale references
    /// - Clean state between scenes
    /// </summary>
    public class SceneCleanup : MonoBehaviour
    {
        [Header("RuntimeSets to Clear")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        private void OnDestroy()
        {
            // Clear RuntimeSets when scene unloads
            enemySet?.Clear();
        }
    }
}
