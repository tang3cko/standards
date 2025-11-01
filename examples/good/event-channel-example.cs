using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    /// <summary>
    /// GOOD EXAMPLE: Quest system with EventChannel integration
    ///
    /// Benefits:
    /// - Complete decoupling between QuestManager and UI
    /// - Multiple subscribers can listen (UI, audio, effects)
    /// - Easy to test and modify independently
    /// - Clear separation of concerns
    /// </summary>
    public class QuestManagerGoodExample : MonoBehaviour
    {
        [Header("Quest Data")]
        [SerializeField] private QuestSO currentQuest;
        [SerializeField] private QuestProgressSO questProgress;

        [Header("Event Channels - Output")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;
        [SerializeField] private IntEventChannelSO onProgressUpdated;
        [SerializeField] private VoidEventChannelSO onQuestCompleted;
        [SerializeField] private VoidEventChannelSO onQuestFailed;

        [Header("Event Channels - Input")]
        [SerializeField] private CollectableEventChannelSO onItemCollected;

        private void OnEnable()
        {
            // Subscribe to EventChannels in OnEnable
            if (onItemCollected != null)
                onItemCollected.OnEventRaised += HandleItemCollected;
        }

        private void OnDisable()
        {
            // IMPORTANT: Always unsubscribe in OnDisable to prevent memory leaks
            if (onItemCollected != null)
                onItemCollected.OnEventRaised -= HandleItemCollected;
        }

        public void SelectQuest(QuestSO quest)
        {
            currentQuest = quest;
            questProgress.Initialize(quest);

            // Raise EventChannel - UI subscribes to this
            onQuestSelected?.RaiseEvent(quest);
        }

        private void HandleItemCollected(CollectableType item)
        {
            // Validate quest is active
            if (questProgress.state != QuestState.InProgress)
                return;

            // Validate correct item
            if (item != questProgress.targetType)
                return;

            // Update progress
            questProgress.currentProgress++;

            // Notify subscribers via EventChannel
            onProgressUpdated?.RaiseEvent(questProgress.currentProgress);

            // Check completion
            if (questProgress.currentProgress >= questProgress.requiredCount)
            {
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            questProgress.state = QuestState.Success;

            // Raise EventChannel - multiple systems can subscribe:
            // - UI (show completion screen)
            // - Audio (play success sound)
            // - Effects (spawn particles)
            onQuestCompleted?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate EventChannels are assigned
            if (onQuestSelected == null)
                Debug.LogWarning($"[QuestManagerGoodExample] onQuestSelected is not assigned on {gameObject.name}.", this);

            if (onProgressUpdated == null)
                Debug.LogWarning($"[QuestManagerGoodExample] onProgressUpdated is not assigned on {gameObject.name}.", this);

            if (onQuestCompleted == null)
                Debug.LogWarning($"[QuestManagerGoodExample] onQuestCompleted is not assigned on {gameObject.name}.", this);

            if (onItemCollected == null)
                Debug.LogWarning($"[QuestManagerGoodExample] onItemCollected is not assigned on {gameObject.name}.", this);
        }
#endif
    }

    /// <summary>
    /// GOOD EXAMPLE: Quest UI subscribes to EventChannels
    ///
    /// Benefits:
    /// - No direct reference to QuestManager
    /// - Can be enabled/disabled independently
    /// - Easy to test with mock EventChannels
    /// </summary>
    public class QuestUIGoodExample : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;
        [SerializeField] private IntEventChannelSO onProgressUpdated;
        [SerializeField] private VoidEventChannelSO onQuestCompleted;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI questNameText;
        [SerializeField] private TMPro.TextMeshProUGUI progressText;

        private void OnEnable()
        {
            // Subscribe to EventChannels
            if (onQuestSelected != null)
                onQuestSelected.OnEventRaised += HandleQuestSelected;

            if (onProgressUpdated != null)
                onProgressUpdated.OnEventRaised += HandleProgressUpdated;

            if (onQuestCompleted != null)
                onQuestCompleted.OnEventRaised += HandleQuestCompleted;
        }

        private void OnDisable()
        {
            // Unsubscribe
            if (onQuestSelected != null)
                onQuestSelected.OnEventRaised -= HandleQuestSelected;

            if (onProgressUpdated != null)
                onProgressUpdated.OnEventRaised -= HandleProgressUpdated;

            if (onQuestCompleted != null)
                onQuestCompleted.OnEventRaised -= HandleQuestCompleted;
        }

        private void HandleQuestSelected(QuestSO quest)
        {
            questNameText.text = quest.questName;
            progressText.text = $"0/{quest.requiredCount}";
        }

        private void HandleProgressUpdated(int progress)
        {
            // Update only when EventChannel fires (not every frame)
            progressText.text = $"{progress}/5";
        }

        private void HandleQuestCompleted()
        {
            // Show completion screen
            ShowCompletionScreen();
        }

        private void ShowCompletionScreen()
        {
            // Implementation
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onQuestSelected == null)
                Debug.LogWarning($"[QuestUIGoodExample] onQuestSelected is not assigned.", this);

            if (onProgressUpdated == null)
                Debug.LogWarning($"[QuestUIGoodExample] onProgressUpdated is not assigned.", this);

            if (questNameText == null)
                Debug.LogWarning($"[QuestUIGoodExample] questNameText is not assigned.", this);

            if (progressText == null)
                Debug.LogWarning($"[QuestUIGoodExample] progressText is not assigned.", this);
        }
#endif
    }
}
