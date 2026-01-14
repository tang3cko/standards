using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Actions
{
    /// <summary>
    /// GOOD EXAMPLE: Simple parameterless Action
    ///
    /// Benefits:
    /// - Inspector-configurable behavior
    /// - Reusable across systems
    /// - Automatic caller tracking for debugging
    /// </summary>
    [CreateAssetMenu(
        fileName = "PlaySound",
        menuName = "ProjectName/Actions/Play Sound")]
    public class PlaySoundAction : ActionSO
    {
        [Header("Settings")]
        [SerializeField] private AudioClip clip;
        [SerializeField] private float volume = 1f;

        public override void Execute(
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);
            }

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Played {clip?.name ?? "null"}");
#endif
        }
    }
}

namespace ProjectName.Actions
{
    /// <summary>
    /// GOOD EXAMPLE: Action that spawns a prefab
    ///
    /// Benefits:
    /// - Prefab and offset configurable in Inspector
    /// - Designer can tweak without code
    /// </summary>
    [CreateAssetMenu(
        fileName = "SpawnEffect",
        menuName = "ProjectName/Actions/Spawn Effect")]
    public class SpawnEffectAction : ActionSO
    {
        [Header("Settings")]
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float duration = 2f;

        public override void Execute(
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            if (effectPrefab != null)
            {
                var instance = Object.Instantiate(effectPrefab);
                instance.transform.position += offset;

                if (duration > 0)
                {
                    Object.Destroy(instance, duration);
                }
            }

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Spawned {effectPrefab?.name ?? "null"}");
#endif
        }
    }
}

namespace ProjectName.Actions
{
    /// <summary>
    /// GOOD EXAMPLE: Generic Action with parameter
    ///
    /// Benefits:
    /// - Runtime parameter (damage amount)
    /// - Inspector-configurable VFX/SFX
    /// </summary>
    [CreateAssetMenu(
        fileName = "DealDamage",
        menuName = "ProjectName/Actions/Deal Damage")]
    public class DealDamageAction : ActionSO<int>
    {
        [Header("Effects")]
        [SerializeField] private GameObject hitVFX;
        [SerializeField] private AudioClip hitSound;

        public override void Execute(int damage,
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            // The actual damage logic would be elsewhere
            // This action handles the presentation

            if (hitVFX != null)
            {
                var vfx = Object.Instantiate(hitVFX);
                Object.Destroy(vfx, 2f);
            }

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, Vector3.zero);
            }

            Debug.Log($"Damage action executed: {damage} damage");

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Damage: {damage}");
#endif
        }
    }
}

namespace ProjectName.Actions
{
    /// <summary>
    /// GOOD EXAMPLE: Action with Vector3 parameter
    ///
    /// Benefits:
    /// - Spawn position provided at runtime
    /// - Prefab configured in Inspector
    /// </summary>
    [CreateAssetMenu(
        fileName = "SpawnAtPosition",
        menuName = "ProjectName/Actions/Spawn At Position")]
    public class SpawnAtPositionAction : ActionSO<Vector3>
    {
        [Header("Settings")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Quaternion rotation = Quaternion.identity;

        public override void Execute(Vector3 position,
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            if (prefab != null)
            {
                Object.Instantiate(prefab, position, rotation);
            }

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Spawned at {position}");
#endif
        }
    }
}

namespace ProjectName.Actions
{
    /// <summary>
    /// GOOD EXAMPLE: Composite Action (Sequence)
    ///
    /// Benefits:
    /// - Compose complex behaviors from simple actions
    /// - Single responsibility per action
    /// </summary>
    [CreateAssetMenu(
        fileName = "ActionSequence",
        menuName = "ProjectName/Actions/Sequence")]
    public class SequenceAction : ActionSO
    {
        [Header("Actions to Execute")]
        [SerializeField] private ActionSO[] actions;

        public override void Execute(
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            foreach (var action in actions)
            {
                action?.Execute();
            }

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Executed {actions.Length} actions");
#endif
        }
    }
}

namespace ProjectName.Actions
{
    /// <summary>
    /// GOOD EXAMPLE: Conditional Action
    ///
    /// Benefits:
    /// - Branch based on Variable state
    /// - No code changes needed for different conditions
    /// </summary>
    [CreateAssetMenu(
        fileName = "ConditionalAction",
        menuName = "ProjectName/Actions/Conditional")]
    public class ConditionalAction : ActionSO
    {
        [Header("Condition")]
        [SerializeField] private BoolVariableSO condition;

        [Header("Actions")]
        [SerializeField] private ActionSO trueAction;
        [SerializeField] private ActionSO falseAction;

        public override void Execute(
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            if (condition != null && condition.Value)
            {
                trueAction?.Execute();
            }
            else
            {
                falseAction?.Execute();
            }

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Condition: {condition?.Value ?? false}");
#endif
        }
    }
}

namespace ProjectName.Quest
{
    /// <summary>
    /// GOOD EXAMPLE: Quest with configurable reward Actions
    ///
    /// Benefits:
    /// - Rewards defined in Inspector (data-driven)
    /// - Same quest class, different rewards per asset
    /// - Easy for designers to configure
    /// </summary>
    [CreateAssetMenu(
        fileName = "Quest",
        menuName = "ProjectName/Data/Quest")]
    public class QuestSO : ScriptableObject
    {
        [Header("Quest Info")]
        public string questName;
        public string description;
        public int requiredCount;

        [Header("Rewards (Actions)")]
        [SerializeField] private ActionSO[] rewardActions;

        /// <summary>
        /// Execute all reward actions when quest completes
        /// </summary>
        public void GiveRewards()
        {
            foreach (var action in rewardActions)
            {
                action?.Execute();
            }
        }
    }
}

namespace ProjectName.Quest
{
    /// <summary>
    /// GOOD EXAMPLE: Quest system uses Actions for rewards
    ///
    /// Benefits:
    /// - Rewards are data-driven (configurable per quest)
    /// - No switch statements for reward types
    /// - New reward types don't require code changes
    /// </summary>
    public class QuestSystem : MonoBehaviour
    {
        [Header("Current Quest")]
        [SerializeField] private QuestSO currentQuest;

        private int currentProgress;

        public void AddProgress(int amount)
        {
            currentProgress += amount;

            if (currentProgress >= currentQuest.requiredCount)
            {
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            // Give rewards via Actions
            currentQuest.GiveRewards();

            Debug.Log($"Quest completed: {currentQuest.questName}");
        }
    }
}

namespace ProjectName.Dialogue
{
    /// <summary>
    /// GOOD EXAMPLE: Dialogue with Action-based responses
    ///
    /// Benefits:
    /// - Response effects defined in data
    /// - Same dialogue system handles any response type
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        public string text;
        public ActionSO[] onChosenActions;
    }

    public class DialogueUI : MonoBehaviour
    {
        [Header("Current Dialogue")]
        [SerializeField] private DialogueChoice[] currentChoices;

        public void OnChoiceSelected(int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= currentChoices.Length)
                return;

            var choice = currentChoices[choiceIndex];

            // Execute all actions for this choice
            foreach (var action in choice.onChosenActions)
            {
                action?.Execute();
            }
        }
    }
}

namespace ProjectName.Events
{
    /// <summary>
    /// GOOD EXAMPLE: Event table with Action triggers
    ///
    /// Benefits:
    /// - Events mapped to Actions in Inspector
    /// - Data-driven event responses
    /// - Easy to add/modify events
    /// </summary>
    [CreateAssetMenu(
        fileName = "EventTable",
        menuName = "ProjectName/Data/Event Table")]
    public class EventTableSO : ScriptableObject
    {
        [System.Serializable]
        public class EventEntry
        {
            public string eventId;
            public ActionSO[] actions;
        }

        [SerializeField] private EventEntry[] entries;

        /// <summary>
        /// Trigger event by ID
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            var entry = System.Array.Find(entries, e => e.eventId == eventId);

            if (entry != null)
            {
                foreach (var action in entry.actions)
                {
                    action?.Execute();
                }
            }
        }
    }
}

namespace ProjectName.UI
{
    /// <summary>
    /// GOOD EXAMPLE: Button triggers Action
    ///
    /// Benefits:
    /// - Action assigned in Inspector
    /// - Button doesn't know what action does
    /// - Same button code, different behaviors
    /// </summary>
    public class ActionButton : MonoBehaviour
    {
        [Header("Action")]
        [SerializeField] private ActionSO action;

        /// <summary>
        /// Call this from Unity Button OnClick event
        /// </summary>
        public void OnClick()
        {
            // Always use null-conditional operator
            action?.Execute();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (action == null)
                Debug.LogWarning($"[ActionButton] action is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
