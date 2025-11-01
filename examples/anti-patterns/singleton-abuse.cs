using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.AntiPatterns
{
    /// <summary>
    /// ANTI-PATTERN: Singleton abuse
    ///
    /// Problems:
    /// - Hidden dependencies (not visible in Inspector)
    /// - Difficult to test (global state)
    /// - Tight coupling across codebase
    /// - Parallel development conflicts (same file edited)
    /// - No IDE refactoring support
    /// - Designers cannot see dependencies
    /// </summary>
    public class GameManagerSingletonBad : MonoBehaviour
    {
        public static GameManagerSingletonBad Instance { get; private set; }

        // Hidden state - not visible in Inspector
        public int currentScore;
        public float timeRemaining;
        public bool isGameActive;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// ANTI-PATTERN: UI directly accessing Singleton
    ///
    /// Problems:
    /// - Hidden dependency on GameManagerSingletonBad
    /// - Cannot test without GameManagerSingletonBad
    /// - Tight coupling
    /// - No Inspector warning if GameManagerSingletonBad missing
    /// </summary>
    public class ScoreUIBad : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI scoreText;

        // ❌ BAD: Update every frame from Singleton
        private void Update()
        {
            // Hidden dependency - not visible anywhere
            if (GameManagerSingletonBad.Instance != null)
            {
                scoreText.text = $"Score: {GameManagerSingletonBad.Instance.currentScore}";
            }
        }

        // No OnValidate to warn about missing dependencies
    }

    /// <summary>
    /// ANTI-PATTERN: Multiple systems accessing same Singleton
    ///
    /// Problems:
    /// - GameManagerSingletonBad becomes a "god object"
    /// - Difficult to refactor (used everywhere)
    /// - Merge conflicts when multiple developers edit
    /// - Impact scope unclear
    /// </summary>
    public class QuestSystemBad : MonoBehaviour
    {
        private void CompleteQuest()
        {
            // ❌ BAD: Direct Singleton access
            GameManagerSingletonBad.Instance.currentScore += 100;
            GameManagerSingletonBad.Instance.isGameActive = false;
        }
    }

    public class EnemyBad : MonoBehaviour
    {
        private void Die()
        {
            // ❌ BAD: Direct Singleton access
            GameManagerSingletonBad.Instance.currentScore += 10;
        }
    }

    // ============================================================
    // GOOD ALTERNATIVE: EventChannel + SerializeField
    // ============================================================

    /// <summary>
    /// GOOD: ScriptableObject for state management
    ///
    /// Benefits:
    /// - Visible in Inspector
    /// - Easy to test
    /// - No global state
    /// </summary>
    [CreateAssetMenu(fileName = "GameState", menuName = "ProjectName/Data/GameState")]
    public class GameStateSO : ScriptableObject
    {
        public int currentScore;
        public float timeRemaining;
        public bool isGameActive;

        public void AddScore(int points)
        {
            currentScore += points;
        }

        public void Reset()
        {
            currentScore = 0;
            timeRemaining = 0;
            isGameActive = false;
        }
    }

    /// <summary>
    /// GOOD: UI with EventChannel subscription
    ///
    /// Benefits:
    /// - Dependencies visible in Inspector
    /// - EventChannel decoupling
    /// - Easy to test
    /// - Update only when score changes (not every frame)
    /// </summary>
    public class ScoreUIGood : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreChanged;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            // ✅ GOOD: Subscribe to EventChannel
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised += UpdateScore;
        }

        private void OnDisable()
        {
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised -= UpdateScore;
        }

        private void UpdateScore(int newScore)
        {
            // Update only when EventChannel fires
            scoreText.text = $"Score: {newScore}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ✅ GOOD: Validate dependencies
            if (onScoreChanged == null)
                Debug.LogWarning($"[ScoreUIGood] onScoreChanged is not assigned.", this);

            if (scoreText == null)
                Debug.LogWarning($"[ScoreUIGood] scoreText is not assigned.", this);
        }
#endif
    }

    /// <summary>
    /// GOOD: Quest system raises EventChannel
    ///
    /// Benefits:
    /// - No dependency on GameState directly
    /// - EventChannel decoupling
    /// - Easy to test
    /// </summary>
    public class QuestSystemGood : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAdded;
        [SerializeField] private VoidEventChannelSO onQuestCompleted;

        private void CompleteQuest()
        {
            // ✅ GOOD: Raise EventChannel
            onScoreAdded?.RaiseEvent(100);
            onQuestCompleted?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onScoreAdded == null)
                Debug.LogWarning($"[QuestSystemGood] onScoreAdded is not assigned.", this);

            if (onQuestCompleted == null)
                Debug.LogWarning($"[QuestSystemGood] onQuestCompleted is not assigned.", this);
        }
#endif
    }

    /// <summary>
    /// GOOD: GameStateManager subscribes to EventChannels
    ///
    /// Benefits:
    /// - Centralized state management
    /// - EventChannel decoupling
    /// - Easy to test
    /// - Dependencies visible
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private GameStateSO gameState;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAdded;
        [SerializeField] private IntEventChannelSO onScoreChanged;

        private void OnEnable()
        {
            if (onScoreAdded != null)
                onScoreAdded.OnEventRaised += HandleScoreAdded;
        }

        private void OnDisable()
        {
            if (onScoreAdded != null)
                onScoreAdded.OnEventRaised -= HandleScoreAdded;
        }

        private void HandleScoreAdded(int points)
        {
            // Update ScriptableObject
            gameState.AddScore(points);

            // Notify subscribers
            onScoreChanged?.RaiseEvent(gameState.currentScore);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameState == null)
                Debug.LogWarning($"[GameStateManager] gameState is not assigned.", this);

            if (onScoreAdded == null)
                Debug.LogWarning($"[GameStateManager] onScoreAdded is not assigned.", this);

            if (onScoreChanged == null)
                Debug.LogWarning($"[GameStateManager] onScoreChanged is not assigned.", this);
        }
#endif
    }
}
