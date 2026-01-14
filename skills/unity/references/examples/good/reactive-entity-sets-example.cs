using UnityEngine;
using System;
using Tang3cko.ReactiveSO;

namespace ProjectName.Data
{
    /// <summary>
    /// GOOD EXAMPLE: Data struct for ReactiveEntitySet
    ///
    /// Rules:
    /// - Must be unmanaged (no managed references)
    /// - Fields only (no properties, no methods)
    /// - All logic in external pure functions
    /// </summary>
    [Serializable]
    public struct UnitState
    {
        public int health;
        public int maxHealth;
        public int teamId;
        public bool isStunned;
        public float stunEndTime;
    }

    /// <summary>
    /// GOOD EXAMPLE: Logic separated into pure functions
    ///
    /// Benefits:
    /// - Testable without Unity
    /// - No side effects
    /// - Reusable across systems
    /// </summary>
    public static class UnitStateCalculator
    {
        public static float GetHealthPercent(UnitState state)
        {
            return state.maxHealth > 0 ? (float)state.health / state.maxHealth : 0f;
        }

        public static bool IsDead(UnitState state)
        {
            return state.health <= 0;
        }

        public static bool IsStunned(UnitState state)
        {
            return state.isStunned && Time.time < state.stunEndTime;
        }

        public static UnitState ApplyDamage(UnitState state, int damage)
        {
            state.health = Mathf.Max(0, state.health - damage);
            return state;
        }

        public static UnitState ApplyHeal(UnitState state, int amount)
        {
            state.health = Mathf.Min(state.maxHealth, state.health + amount);
            return state;
        }

        public static UnitState ApplyStun(UnitState state, float duration)
        {
            state.isStunned = true;
            state.stunEndTime = Time.time + duration;
            return state;
        }

        public static UnitState ClearStun(UnitState state)
        {
            state.isStunned = false;
            state.stunEndTime = 0f;
            return state;
        }
    }
}

namespace ProjectName.Data
{
    /// <summary>
    /// GOOD EXAMPLE: ReactiveEntitySet ScriptableObject
    ///
    /// Benefits:
    /// - O(1) ID-based lookup
    /// - Per-entity subscriptions
    /// - Scene-persistent state
    /// </summary>
    [CreateAssetMenu(
        fileName = "UnitStateSet",
        menuName = "ProjectName/Reactive Entity Sets/Unit State")]
    public class UnitStateSetSO : ReactiveEntitySetSO<UnitState>
    {
    }
}

namespace ProjectName.Unit
{
    /// <summary>
    /// GOOD EXAMPLE: Unit using ReactiveEntity base class
    ///
    /// Benefits:
    /// - Automatic Register/Unregister lifecycle
    /// - EntityId from GetInstanceID()
    /// - State property for read/write
    /// </summary>
    public class Unit : ReactiveEntity<UnitState>
    {
        [Header("ReactiveEntitySet")]
        [SerializeField] private UnitStateSetSO unitSet;

        [Header("Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int teamId = 0;

        protected override ReactiveEntitySetSO<UnitState> Set => unitSet;

        protected override UnitState InitialState => new UnitState
        {
            health = maxHealth,
            maxHealth = maxHealth,
            teamId = teamId,
            isStunned = false,
            stunEndTime = 0f
        };

        /// <summary>
        /// Take damage - modifies state in RES
        /// </summary>
        public void TakeDamage(int damage)
        {
            // Use pure function to calculate new state
            State = UnitStateCalculator.ApplyDamage(State, damage);

            if (UnitStateCalculator.IsDead(State))
            {
                Die();
            }
        }

        /// <summary>
        /// Apply stun effect
        /// </summary>
        public void ApplyStun(float duration)
        {
            State = UnitStateCalculator.ApplyStun(State, duration);
        }

        /// <summary>
        /// Heal unit
        /// </summary>
        public void Heal(int amount)
        {
            State = UnitStateCalculator.ApplyHeal(State, amount);
        }

        private void Die()
        {
            // State persists in RES until Unregister
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (unitSet == null)
                Debug.LogWarning($"[Unit] unitSet is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}

namespace ProjectName.UI
{
    /// <summary>
    /// GOOD EXAMPLE: Health bar subscribes to specific entity
    ///
    /// Benefits:
    /// - Per-entity subscription (not global)
    /// - Only updates when this entity changes
    /// - Efficient for many entities
    /// </summary>
    public class UnitHealthBar : MonoBehaviour
    {
        [Header("ReactiveEntitySet")]
        [SerializeField] private UnitStateSetSO unitSet;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Image fillImage;
        [SerializeField] private GameObject stunIndicator;

        private int targetEntityId;
        private bool isSubscribed;

        /// <summary>
        /// Assign this health bar to track a specific unit
        /// </summary>
        public void AssignToEntity(int entityId)
        {
            // Unsubscribe from previous entity
            if (isSubscribed)
            {
                unitSet.UnsubscribeFromEntity(targetEntityId, OnStateChanged);
            }

            targetEntityId = entityId;

            // Subscribe to new entity
            unitSet.SubscribeToEntity(entityId, OnStateChanged);
            isSubscribed = true;

            // Initial update
            if (unitSet.TryGetData(entityId, out var state))
            {
                UpdateDisplay(state);
            }
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            if (isSubscribed)
            {
                unitSet.UnsubscribeFromEntity(targetEntityId, OnStateChanged);
                isSubscribed = false;
            }
        }

        private void OnStateChanged(UnitState oldState, UnitState newState)
        {
            UpdateDisplay(newState);
        }

        private void UpdateDisplay(UnitState state)
        {
            // Update health bar
            fillImage.fillAmount = UnitStateCalculator.GetHealthPercent(state);

            // Update stun indicator
            stunIndicator.SetActive(UnitStateCalculator.IsStunned(state));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (unitSet == null)
                Debug.LogWarning($"[UnitHealthBar] unitSet is not assigned.", this);

            if (fillImage == null)
                Debug.LogWarning($"[UnitHealthBar] fillImage is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.Combat
{
    /// <summary>
    /// GOOD EXAMPLE: Damage system accesses RES directly
    ///
    /// Benefits:
    /// - Direct ID-based access without Find
    /// - UpdateData for functional updates
    /// - Works with any entity in the set
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        [Header("ReactiveEntitySet")]
        [SerializeField] private UnitStateSetSO unitSet;

        /// <summary>
        /// Deal damage to entity by ID
        /// </summary>
        public void DealDamage(int targetEntityId, int damage)
        {
            if (!unitSet.Contains(targetEntityId))
                return;

            // Functional update pattern
            unitSet.UpdateData(targetEntityId, state =>
                UnitStateCalculator.ApplyDamage(state, damage));
        }

        /// <summary>
        /// Deal area damage
        /// </summary>
        public void DealAreaDamage(int[] targetEntityIds, int damage)
        {
            foreach (var id in targetEntityIds)
            {
                DealDamage(id, damage);
            }
        }

        /// <summary>
        /// Apply stun effect by ID
        /// </summary>
        public void ApplyStun(int targetEntityId, float duration)
        {
            if (!unitSet.Contains(targetEntityId))
                return;

            unitSet.UpdateData(targetEntityId, state =>
                UnitStateCalculator.ApplyStun(state, duration));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (unitSet == null)
                Debug.LogWarning($"[DamageSystem] unitSet is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.Score
{
    /// <summary>
    /// GOOD EXAMPLE: Score system listens to entity removal
    ///
    /// Benefits:
    /// - EventChannel notifications from RES
    /// - Decoupled from Unit lifecycle
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        [Header("ReactiveEntitySet")]
        [SerializeField] private UnitStateSetSO unitSet;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onEntityRemoved;

        [Header("Variables")]
        [SerializeField] private IntVariableSO playerScore;

        [Header("Settings")]
        [SerializeField] private int pointsPerKill = 100;

        private void OnEnable()
        {
            // Subscribe to RES OnItemRemoved EventChannel
            if (onEntityRemoved != null)
                onEntityRemoved.OnEventRaised += HandleEntityRemoved;
        }

        private void OnDisable()
        {
            if (onEntityRemoved != null)
                onEntityRemoved.OnEventRaised -= HandleEntityRemoved;
        }

        private void HandleEntityRemoved(int entityId)
        {
            // Add score when enemy dies
            playerScore.Value += pointsPerKill;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (unitSet == null)
                Debug.LogWarning($"[ScoreSystem] unitSet is not assigned.", this);

            if (onEntityRemoved == null)
                Debug.LogWarning($"[ScoreSystem] onEntityRemoved is not assigned.", this);

            if (playerScore == null)
                Debug.LogWarning($"[ScoreSystem] playerScore is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.Core
{
    /// <summary>
    /// GOOD EXAMPLE: Standalone pattern (no MonoBehaviour)
    ///
    /// Benefits:
    /// - Pure data management
    /// - Works for simulations, networking
    /// - Entities exist without GameObjects
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        [Header("ReactiveEntitySet")]
        [SerializeField] private UnitStateSetSO unitSet;

        [Header("Settings")]
        [SerializeField] private int simulatedUnitCount = 1000;

        private void Start()
        {
            // Register entities with int IDs (no GameObjects needed)
            for (int i = 0; i < simulatedUnitCount; i++)
            {
                unitSet.Register(i, new UnitState
                {
                    health = 100,
                    maxHealth = 100,
                    teamId = i % 4,
                    isStunned = false
                });
            }
        }

        private void Update()
        {
            // Process all entities by ID
            unitSet.ForEach((id, state) =>
            {
                // Simulation logic here
                // Example: Clear expired stuns
                if (state.isStunned && Time.time >= state.stunEndTime)
                {
                    unitSet.SetData(id, UnitStateCalculator.ClearStun(state));
                }
            });
        }

        private void OnDestroy()
        {
            // Clean up all entities
            unitSet.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (unitSet == null)
                Debug.LogWarning($"[SimulationManager] unitSet is not assigned.", this);
        }
#endif
    }
}
