# UXML Structure and Organization

## Purpose

Define clear structure for UI Toolkit files, naming conventions, and integration patterns with EventChannels and ScriptableObjects for maintainable and scalable UI implementation.

## Checklist

- [ ] Separate UXML, USS, and C# files into appropriate directories
- [ ] Use PascalCase for name attributes
- [ ] Use kebab-case for class attributes
- [ ] Include RequireComponent(typeof(UIDocument))
- [ ] Query elements in OnEnable using Q<T>()
- [ ] Subscribe to EventChannels in OnEnable
- [ ] Unsubscribe from EventChannels in OnDisable
- [ ] Reference USS files at top of UXML
- [ ] Use ScriptableObjects for data management

---

## Directory Structure

Separate UI structure (UXML), styles (USS), and logic (C#) into dedicated directories:

```
Assets/_Project/
├── UI/
│   ├── UXML/                    # UI structure definitions
│   │   ├── Quest/
│   │   │   ├── QuestProgressHUD.uxml
│   │   │   └── QuestSelectionPanel.uxml
│   │   ├── Menu/
│   │   │   ├── TitleScreen.uxml
│   │   │   └── SettingsMenu.uxml
│   │   └── Common/
│   │       └── NotificationPanel.uxml
│   │
│   └── USS/                     # Style definitions
│       ├── Common.uss           # Common styles (colors, fonts, buttons)
│       ├── QuestUI.uss          # Quest-related styles
│       └── MenuUI.uss           # Menu-related styles
│
└── Scripts/
    └── UI/                      # UI logic
        ├── Quest/
        │   ├── QuestProgressUI.cs
        │   └── QuestSelectionUI.cs
        └── Menu/
            └── TitleScreenUI.cs
```

---

## Naming Conventions

### UXML name Attribute

Use PascalCase for `name` attributes (used for C# queries):

```xml
<ui:UXML>
    <ui:VisualElement name="QuestProgressPanel" class="panel">
        <ui:Label name="QuestNameText" class="quest-title" />
        <ui:Label name="ProgressText" class="quest-progress" />
        <ui:Label name="TimerText" class="quest-timer" />
    </ui:VisualElement>
</ui:UXML>
```

### USS class Attribute

Use kebab-case for `class` attributes (BEM naming):

```xml
<ui:VisualElement class="quest-result__panel">
    <ui:Label class="quest-result__title" />
    <ui:Label class="quest-result__status quest-result__status--success" />
</ui:VisualElement>
```

---

## Basic UXML Structure

### UXML File Template

```xml
<!-- Assets/_Project/UI/UXML/Quest/QuestProgressHUD.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <!-- Reference USS files -->
    <Style src="../../USS/Common.uss" />
    <Style src="../../USS/QuestUI.uss" />

    <!-- Root container -->
    <ui:VisualElement name="QuestProgressPanel" class="quest-progress-panel">
        <!-- Title -->
        <ui:Label name="QuestNameText" class="quest-title" text="Quest Name" />

        <!-- Progress -->
        <ui:Label name="ProgressText" class="quest-progress" text="0/5" />

        <!-- Timer -->
        <ui:Label name="TimerText" class="quest-timer" text="05:00" />
    </ui:VisualElement>
</ui:UXML>
```

### USS Reference

Always reference USS files at the top of UXML:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="../../USS/Common.uss" />
    <Style src="../../USS/QuestUI.uss" />

    <!-- UI elements follow -->
</ui:UXML>
```

---

## C# Controller Pattern

### Basic Controller Template

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Quest progress HUD controller
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class QuestProgressUI : MonoBehaviour
    {
        [Header("Event Channels - Input")]
        [SerializeField] private VoidEventChannelSO onQuestStarted;
        [SerializeField] private IntEventChannelSO onQuestProgressUpdated;
        [SerializeField] private FloatEventChannelSO onTimeRemainingUpdated;

        private UIDocument uiDocument;
        private Label questNameText;
        private Label progressText;
        private Label timerText;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            // Get root element
            var root = uiDocument.rootVisualElement;

            // Query elements by name attribute
            questNameText = root.Q<Label>("QuestNameText");
            progressText = root.Q<Label>("ProgressText");
            timerText = root.Q<Label>("TimerText");

            // Subscribe to EventChannels
            onQuestStarted.OnEventRaised += HandleQuestStarted;
            onQuestProgressUpdated.OnEventRaised += HandleProgressUpdated;
            onTimeRemainingUpdated.OnEventRaised += HandleTimeUpdated;
        }

        private void OnDisable()
        {
            // Unsubscribe from EventChannels
            onQuestStarted.OnEventRaised -= HandleQuestStarted;
            onQuestProgressUpdated.OnEventRaised -= HandleProgressUpdated;
            onTimeRemainingUpdated.OnEventRaised -= HandleTimeUpdated;
        }

        private void HandleQuestStarted()
        {
            questNameText.text = "Collect 5 Mushrooms";
        }

        private void HandleProgressUpdated(int currentProgress)
        {
            progressText.text = $"{currentProgress}/5";
        }

        private void HandleTimeUpdated(float timeRemaining)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
```

---

## EventChannel Integration

### Button Click to EventChannel

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestSelectionUI : MonoBehaviour
    {
        [Header("Event Channels - Output")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;

        private Button startButton;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            startButton = root.Q<Button>("StartButton");

            // Convert button click to EventChannel
            startButton.clicked += () =>
            {
                onQuestSelected?.RaiseEvent();
            };
        }
    }
}
```

### Bidirectional EventChannel Communication

```csharp
namespace ProjectName.UI
{
    /// <summary>
    /// Settings menu with bidirectional EventChannel communication
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SettingsUI : MonoBehaviour
    {
        [Header("Event Channels - Input")]
        [SerializeField] private FloatEventChannelSO onVolumeChanged;

        [Header("Event Channels - Output")]
        [SerializeField] private FloatEventChannelSO onVolumeAdjusted;

        private Slider volumeSlider;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            volumeSlider = root.Q<Slider>("VolumeSlider");

            // Listen to slider changes
            volumeSlider.RegisterValueChangedCallback(HandleSliderChanged);

            // Listen to external volume changes
            onVolumeChanged.OnEventRaised += HandleVolumeChanged;
        }

        private void OnDisable()
        {
            volumeSlider.UnregisterValueChangedCallback(HandleSliderChanged);
            onVolumeChanged.OnEventRaised -= HandleVolumeChanged;
        }

        private void HandleSliderChanged(ChangeEvent<float> evt)
        {
            // Raise EventChannel when slider changes
            onVolumeAdjusted?.RaiseEvent(evt.newValue);
        }

        private void HandleVolumeChanged(float volume)
        {
            // Update slider when external volume changes
            volumeSlider.SetValueWithoutNotify(volume);
        }
    }
}
```

---

## Data Binding with ScriptableObjects

### Dynamic List Rendering

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.EventChannels;
using System.Collections.Generic;

namespace ProjectName.UI
{
    /// <summary>
    /// Quest selection panel with dynamic list rendering
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class QuestSelectionUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private QuestProgressSO questProgress;
        [SerializeField] private List<QuestSO> availableQuests;

        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;

        private ScrollView questListContainer;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            questListContainer = root.Q<ScrollView>("QuestListContainer");

            // Render quest list
            RenderQuestList();
        }

        private void RenderQuestList()
        {
            questListContainer.Clear();

            foreach (var quest in availableQuests)
            {
                // Create button for each quest
                var questButton = new Button(() => SelectQuest(quest))
                {
                    text = quest.questName
                };
                questButton.AddToClassList("quest-button");

                questListContainer.Add(questButton);
            }
        }

        private void SelectQuest(QuestSO quest)
        {
            // Initialize quest progress
            questProgress.Initialize(quest);

            // Raise EventChannel
            onQuestSelected?.RaiseEvent(quest);
        }
    }
}
```

### UXML for Dynamic List

```xml
<!-- QuestSelectionPanel.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="../../USS/Common.uss" />
    <Style src="../../USS/QuestUI.uss" />

    <ui:VisualElement name="QuestSelectionPanel" class="quest-selection__panel">
        <ui:Label text="Select a Quest" class="text-title" />

        <!-- Container for dynamically generated buttons -->
        <ui:ScrollView name="QuestListContainer" class="quest-selection__list">
            <!-- Buttons created in C# -->
        </ui:ScrollView>

        <ui:Button name="CancelButton" text="Cancel" class="button-primary" />
    </ui:VisualElement>
</ui:UXML>
```

### USS for Quest List

```css
/* QuestUI.uss */
.quest-selection__panel {
    width: 600px;
    height: 500px;
    background-color: var(--color-background);
    border-radius: var(--border-radius-large);
    padding: var(--padding-large);
}

.quest-selection__list {
    flex-grow: 1;
    margin: var(--margin-medium) 0;
}

.quest-button {
    background-color: rgba(30, 144, 255, 0.2);
    color: var(--color-text);
    font-size: var(--font-size-medium);
    padding: var(--padding-medium);
    margin-bottom: var(--padding-small);
    border-radius: var(--border-radius);
    border-width: 2px;
    border-color: var(--color-secondary);
}

.quest-button:hover {
    background-color: rgba(30, 144, 255, 0.4);
    border-color: #4169E1;
}
```

---

## Element Query Best Practices

### Query by Type and Name

```csharp
// ✅ Good: Type-safe query
Label titleLabel = root.Q<Label>("TitleLabel");
Button okButton = root.Q<Button>("OKButton");
ScrollView itemList = root.Q<ScrollView>("ItemListContainer");

// ❌ Bad: Untyped query
VisualElement titleLabel = root.Q("TitleLabel");
```

### Query with Null Check

```csharp
private void OnEnable()
{
    var root = uiDocument.rootVisualElement;

    questNameText = root.Q<Label>("QuestNameText");

    if (questNameText == null)
    {
        Debug.LogError($"[{GetType().Name}] QuestNameText not found in UXML", this);
        return;
    }
}
```

### Query by Class (Multiple Elements)

```csharp
// Query all elements with a specific class
UQueryBuilder<Label> allLabels = root.Query<Label>(className: "quest-item__label");

// Execute query and iterate
allLabels.ForEach(label =>
{
    label.style.color = new StyleColor(Color.white);
});
```

---

## Performance Optimization

### Cache Element References

```csharp
// ✅ Good: Cache in OnEnable
private Label statusText;

private void OnEnable()
{
    var root = uiDocument.rootVisualElement;
    statusText = root.Q<Label>("StatusText");
}

private void UpdateStatus(string status)
{
    statusText.text = status;
}

// ❌ Bad: Query every time
private void UpdateStatus(string status)
{
    var root = uiDocument.rootVisualElement;
    var statusText = root.Q<Label>("StatusText"); // Expensive!
    statusText.text = status;
}
```

### Event-Driven Updates

```csharp
// ✅ Good: Update only when EventChannel fires
private void OnEnable()
{
    onProgressUpdated.OnEventRaised += UpdateProgress;
}

private void UpdateProgress(int progress)
{
    progressText.text = $"{progress}/10";
}

// ❌ Bad: Poll every frame
private void Update()
{
    progressText.text = $"{gameManager.Progress}/10";
}
```

---

## References

- [BEM Naming](bem-naming.md)
- [Design Tokens](design-tokens.md)
- [USS Responsive Design](uss-responsive.md)
- [Event Channels](../../architecture/event-channels.md)
- [ScriptableObject Pattern](../../architecture/scriptableobject.md)
