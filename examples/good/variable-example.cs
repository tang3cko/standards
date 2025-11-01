using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Core
{
    /// <summary>
    /// GOOD EXAMPLE: Game state management with Variables
    ///
    /// Benefits:
    /// - Centralized state (single source of truth)
    /// - Automatic event notification on value changes
    /// - Persistent state across scene reloads (Play mode)
    /// - Inspector-visible state for debugging
    /// - Decoupled communication via EventChannels
    /// </summary>
    public class GameStateManagerGoodExample : MonoBehaviour
    {
        [Header("Variables - State Management")]
        [SerializeField] private IntVariableSO currentLevel;
        [SerializeField] private IntVariableSO playerScore;
        [SerializeField] private BoolVariableSO isGameActive;
        [SerializeField] private FloatVariableSO gameTimeRemaining;

        [Header("Settings")]
        [SerializeField] private float levelTimeLimit = 300f;

        private void Start()
        {
            // Initialize game state
            InitializeGame();
        }

        private void Update()
        {
            if (isGameActive.Value)
            {
                // Decrement timer
                gameTimeRemaining.Value -= Time.deltaTime;

                // Check time limit
                if (gameTimeRemaining.Value <= 0)
                {
                    EndGame();
                }
            }
        }

        public void InitializeGame()
        {
            // Reset all Variables to initial state
            currentLevel.ResetToInitial();
            playerScore.ResetToInitial();
            isGameActive.Value = false;
            gameTimeRemaining.Value = levelTimeLimit;
        }

        public void StartGame()
        {
            isGameActive.Value = true;
            // Automatically raises onGameActiveChanged event
        }

        public void EndGame()
        {
            isGameActive.Value = false;
            // Automatically raises onGameActiveChanged event
        }

        public void AddScore(int points)
        {
            // Setting value automatically raises onScoreChanged event
            playerScore.Value += points;
        }

        public void NextLevel()
        {
            currentLevel.Value++;
            // Automatically raises onLevelChanged event

            // Reset timer for next level
            gameTimeRemaining.Value = levelTimeLimit;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (currentLevel == null)
                Debug.LogWarning($"[GameStateManagerGoodExample] currentLevel is not assigned.", this);

            if (playerScore == null)
                Debug.LogWarning($"[GameStateManagerGoodExample] playerScore is not assigned.", this);

            if (isGameActive == null)
                Debug.LogWarning($"[GameStateManagerGoodExample] isGameActive is not assigned.", this);

            if (gameTimeRemaining == null)
                Debug.LogWarning($"[GameStateManagerGoodExample] gameTimeRemaining is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.UI
{
    /// <summary>
    /// GOOD EXAMPLE: UI subscribes to state changes via EventChannels
    ///
    /// Benefits:
    /// - No direct reference to GameStateManager
    /// - Event-driven UI updates (no polling in Update)
    /// - Can be enabled/disabled independently
    /// - Easy to test with mock EventChannels
    /// </summary>
    public class GameUIGoodExample : MonoBehaviour
    {
        [Header("Event Channels - State Change Notifications")]
        [SerializeField] private IntEventChannelSO onLevelChanged;
        [SerializeField] private IntEventChannelSO onScoreChanged;
        [SerializeField] private BoolEventChannelSO onGameActiveChanged;
        [SerializeField] private FloatEventChannelSO onTimeRemainingChanged;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI levelText;
        [SerializeField] private TMPro.TextMeshProUGUI scoreText;
        [SerializeField] private TMPro.TextMeshProUGUI timerText;
        [SerializeField] private GameObject gameOverPanel;

        private void OnEnable()
        {
            // Subscribe to EventChannels
            if (onLevelChanged != null)
                onLevelChanged.OnEventRaised += UpdateLevelDisplay;

            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised += UpdateScoreDisplay;

            if (onGameActiveChanged != null)
                onGameActiveChanged.OnEventRaised += UpdateGameActiveDisplay;

            if (onTimeRemainingChanged != null)
                onTimeRemainingChanged.OnEventRaised += UpdateTimerDisplay;
        }

        private void OnDisable()
        {
            // IMPORTANT: Always unsubscribe to prevent memory leaks
            if (onLevelChanged != null)
                onLevelChanged.OnEventRaised -= UpdateLevelDisplay;

            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised -= UpdateScoreDisplay;

            if (onGameActiveChanged != null)
                onGameActiveChanged.OnEventRaised -= UpdateGameActiveDisplay;

            if (onTimeRemainingChanged != null)
                onTimeRemainingChanged.OnEventRaised -= UpdateTimerDisplay;
        }

        private void UpdateLevelDisplay(int level)
        {
            levelText.text = $"Level {level}";
        }

        private void UpdateScoreDisplay(int score)
        {
            scoreText.text = $"Score: {score:N0}";
        }

        private void UpdateGameActiveDisplay(bool isActive)
        {
            // Show/hide game over panel
            gameOverPanel.SetActive(!isActive);
        }

        private void UpdateTimerDisplay(float timeRemaining)
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // Warning color when time is low
            if (timeRemaining <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onLevelChanged == null)
                Debug.LogWarning($"[GameUIGoodExample] onLevelChanged is not assigned.", this);

            if (onScoreChanged == null)
                Debug.LogWarning($"[GameUIGoodExample] onScoreChanged is not assigned.", this);

            if (levelText == null)
                Debug.LogWarning($"[GameUIGoodExample] levelText is not assigned.", this);

            if (scoreText == null)
                Debug.LogWarning($"[GameUIGoodExample] scoreText is not assigned.", this);

            if (timerText == null)
                Debug.LogWarning($"[GameUIGoodExample] timerText is not assigned.", this);

            if (gameOverPanel == null)
                Debug.LogWarning($"[GameUIGoodExample] gameOverPanel is not assigned.", this);
        }
#endif
    }
}

namespace ProjectName.Audio
{
    /// <summary>
    /// GOOD EXAMPLE: Audio system reacts to state changes
    ///
    /// Benefits:
    /// - Decoupled from game logic
    /// - Event-driven audio playback
    /// - Multiple systems can subscribe to same events
    /// </summary>
    public class GameAudioGoodExample : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreChanged;
        [SerializeField] private BoolEventChannelSO onGameActiveChanged;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip scoreSound;
        [SerializeField] private AudioClip gameStartSound;
        [SerializeField] private AudioClip gameOverSound;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised += HandleScoreChanged;

            if (onGameActiveChanged != null)
                onGameActiveChanged.OnEventRaised += HandleGameActiveChanged;
        }

        private void OnDisable()
        {
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised -= HandleScoreChanged;

            if (onGameActiveChanged != null)
                onGameActiveChanged.OnEventRaised -= HandleGameActiveChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            // Play sound when score increases
            if (scoreSound != null)
            {
                audioSource.PlayOneShot(scoreSound);
            }
        }

        private void HandleGameActiveChanged(bool isActive)
        {
            if (isActive)
            {
                // Game started
                if (gameStartSound != null)
                {
                    audioSource.PlayOneShot(gameStartSound);
                }
            }
            else
            {
                // Game ended
                if (gameOverSound != null)
                {
                    audioSource.PlayOneShot(gameOverSound);
                }
            }
        }
    }
}

namespace ProjectName.Data
{
    /// <summary>
    /// GOOD EXAMPLE: Achievement system reads Variables directly
    ///
    /// Benefits:
    /// - Can check current state without EventChannel
    /// - Useful for one-time checks or queries
    /// - No subscription overhead if not needed
    /// </summary>
    public class AchievementSystemGoodExample : MonoBehaviour
    {
        [Header("Variables - Direct Access")]
        [SerializeField] private IntVariableSO playerScore;
        [SerializeField] private IntVariableSO currentLevel;

        [Header("Event Channels - Notifications")]
        [SerializeField] private IntEventChannelSO onScoreChanged;

        private bool achievementUnlocked = false;

        private void OnEnable()
        {
            // Subscribe only when we need reactive behavior
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised += CheckAchievements;
        }

        private void OnDisable()
        {
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised -= CheckAchievements;
        }

        private void CheckAchievements(int newScore)
        {
            // Read Variables directly for current state
            if (!achievementUnlocked && playerScore.Value >= 10000 && currentLevel.Value >= 5)
            {
                UnlockAchievement("Master Player");
                achievementUnlocked = true;
            }
        }

        private void UnlockAchievement(string achievementName)
        {
            Debug.Log($"Achievement Unlocked: {achievementName}");
        }
    }
}
