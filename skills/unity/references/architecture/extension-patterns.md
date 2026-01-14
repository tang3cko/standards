# Extension Patterns

## Purpose

Define consistent implementation patterns for adding new features to projects using ScriptableObject-driven architecture. These patterns ensure development efficiency and code quality through SpecKit workflow integration.

## Checklist

- [ ] Follow SpecKit workflow (specify → clarify → plan → tasks → implement)
- [ ] Use EventChannels for decoupled communication
- [ ] Manage data with ScriptableObjects
- [ ] Follow CreateAssetMenu naming conventions
- [ ] Separate immutable data from runtime state
- [ ] Include XML documentation comments
- [ ] Add Tooltip attributes for Inspector guidance
- [ ] Subscribe to events in OnEnable, unsubscribe in OnDisable
- [ ] Use appropriate namespace for new components

---

## SpecKit workflow - P1

All new features must follow the SpecKit (Spec-Driven Development) workflow:

1. **`/specify`** - Define feature requirements (`docs/00_spec/`)
2. **`/clarify`** - Resolve ambiguities (`docs/00_spec/clarifications.md`)
3. **`/plan`** - Create technical implementation plan (`docs/98_plans/`)
4. **`/tasks`** - Break down into implementation tasks (`docs/98_plans/tasks/`)
5. **`/implement`** - Execute tasks and implement

**Reference:** https://github.com/github/spec-kit

### Workflow example

```
1. Create docs/00_spec/inventory-system-spec.md
2. Update docs/00_spec/clarifications.md with design decisions
3. Create docs/98_plans/inventory-system-plan.md
4. Create docs/98_plans/tasks/inventory-system-tasks.md
5. Implement tasks sequentially
6. Update documentation after completion
```

---

## Pattern 1: Adding new ScriptableObject - P1

### Immutable data definition

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Item
{
    /// <summary>
    /// Item data definition (immutable)
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "ProjectName/Data/Item/ItemData")]
    public class ItemSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique item identifier")]
        public string itemID;

        [Tooltip("Item display name")]
        public string itemName;

        [Tooltip("Item description")]
        [TextArea(3, 5)]
        public string itemDescription;

        [Header("Properties")]
        [Tooltip("Item category")]
        public ItemType itemType;

        [Tooltip("Maximum stack size")]
        public int maxStack = 99;

        [Tooltip("Item icon")]
        public Sprite icon;
    }

    public enum ItemType
    {
        Consumable,
        Equipment,
        Material,
        QuestItem
    }
}
```

### Runtime data (when needed)

```csharp
namespace ProjectName.Item
{
    /// <summary>
    /// Inventory state (runtime)
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryState", menuName = "ProjectName/Data/Item/InventoryState")]
    public class InventoryStateSO : ScriptableObject
    {
        [System.Serializable]
        public class InventorySlot
        {
            public ItemSO item;
            public int count;
        }

        public List<InventorySlot> slots = new List<InventorySlot>();

        public void AddItem(ItemSO item, int count = 1)
        {
            // Find existing slot or create new one
            var existingSlot = slots.Find(s => s.item == item);

            if (existingSlot != null)
            {
                existingSlot.count += count;
            }
            else
            {
                slots.Add(new InventorySlot { item = item, count = count });
            }
        }

        public void RemoveItem(ItemSO item, int count = 1)
        {
            var slot = slots.Find(s => s.item == item);

            if (slot != null)
            {
                slot.count -= count;

                if (slot.count <= 0)
                {
                    slots.Remove(slot);
                }
            }
        }

        public void Clear()
        {
            slots.Clear();
        }
    }
}
```

### CreateAssetMenu naming conventions

Follow these patterns consistently:

- **Data ScriptableObjects**: `"ProjectName/Data/Category/Specific"`
- **RuntimeSet ScriptableObjects**: `"ProjectName/RuntimeSet/TypeName"`
- **EventChannel ScriptableObjects**: `"ProjectName/Events/TypeName Event Channel"`

```csharp
// Data
[CreateAssetMenu(fileName = "Item", menuName = "ProjectName/Data/Item/ItemData")]
[CreateAssetMenu(fileName = "Enemy", menuName = "ProjectName/Data/Enemy/EnemyData")]

// RuntimeSet
[CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]

// EventChannel
[CreateAssetMenu(fileName = "ItemEventChannel", menuName = "ProjectName/Events/Item Event Channel")]
```

---

## Pattern 2: Adding new EventChannel - P1

### EventChannel class definition

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Item
{
    /// <summary>
    /// EventChannel for Item-related events
    /// </summary>
    [CreateAssetMenu(fileName = "ItemEventChannel", menuName = "ProjectName/Events/Item Event Channel")]
    public class ItemEventChannelSO : EventChannelSO<ItemSO>
    {
        // Tang3cko.ReactiveSO base class provides all functionality
    }
}
```

### ScriptableObject asset creation

Create assets in the appropriate directory:

```
Assets/_Project/ScriptableObjects/Events/Item/
├── OnItemAcquired.asset (ItemEventChannelSO)
├── OnItemUsed.asset (ItemEventChannelSO)
└── OnInventoryChanged.asset (VoidEventChannelSO)
```

### Publisher and subscriber implementation

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Item
{
    // Publisher
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemSO item;

        [Header("Event Channels")]
        [SerializeField] private ItemEventChannelSO onItemAcquired;

        public void Pickup()
        {
            // Raise event - publisher doesn't know who receives it
            onItemAcquired?.RaiseEvent(item);
        }
    }

    // Subscriber
    public class InventoryManager : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private InventoryStateSO inventoryState;

        [Header("Event Channels")]
        [SerializeField] private ItemEventChannelSO onItemAcquired;

        private void OnEnable()
        {
            onItemAcquired.OnEventRaised += HandleItemAcquired;
        }

        private void OnDisable()
        {
            onItemAcquired.OnEventRaised -= HandleItemAcquired;
        }

        private void HandleItemAcquired(ItemSO item)
        {
            inventoryState.AddItem(item, 1);
            Debug.Log($"[InventoryManager] Acquired: {item.itemName}");
        }
    }
}
```

---

## Pattern 3: Adding new RuntimeSet - P1

### RuntimeSet class definition

Use the `RuntimeSetSO<T>` base class for custom component tracking:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Enemy RuntimeSet for dynamic tracking
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "Reactive SO/Runtime Sets/Enemy")]
    public class EnemyRuntimeSetSO : RuntimeSetSO<EnemyController>
    {
        // Base class provides:
        // - Items (IReadOnlyList<T>)
        // - Count
        // - Add(T item)
        // - Remove(T item)
        // - Clear()
        // - Contains(T item)
        // Inspector-assignable EventChannels:
        // - onItemsChanged (VoidEventChannelSO)
        // - onCountChanged (IntEventChannelSO)

        /// <summary>
        /// Get the closest enemy to a position
        /// </summary>
        public EnemyController GetClosestTo(Vector3 position)
        {
            EnemyController closest = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in Items)
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
}
```

### Registration pattern

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("RuntimeSet")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        // Register on enable
        private void OnEnable()
        {
            enemySet?.Add(this);
        }

        // Unregister on disable
        private void OnDisable()
        {
            enemySet?.Remove(this);
        }
    }

    // Usage example with EventChannels
    public class EnemySpawner : MonoBehaviour
    {
        [Header("RuntimeSet")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onEnemiesChanged;
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;

        private void OnEnable()
        {
            // Subscribe to EventChannels assigned to the RuntimeSet
            onEnemiesChanged.OnEventRaised += HandleEnemiesChanged;
            onEnemyCountChanged.OnEventRaised += HandleCountChanged;
        }

        private void OnDisable()
        {
            onEnemiesChanged.OnEventRaised -= HandleEnemiesChanged;
            onEnemyCountChanged.OnEventRaised -= HandleCountChanged;
        }

        private void HandleEnemiesChanged()
        {
            Debug.Log($"Enemies changed. Current count: {enemySet.Count}");
        }

        private void HandleCountChanged(int newCount)
        {
            Debug.Log($"Enemy count: {newCount}");
        }

        public void ProcessAllEnemies()
        {
            // Iterate over all active enemies
            foreach (var enemy in enemySet.Items)
            {
                // Process enemy
            }
        }

        public int GetActiveEnemyCount()
        {
            return enemySet.Count;
        }
    }
}
```

---

## Pattern 4: Adding new UI (UI Toolkit) - P1

### UXML structure

```xml
<!-- Assets/_Project/UI/UXML/InventoryPanel.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="InventoryPanel" class="panel inventory-panel">
        <ui:Label name="InventoryTitle" text="Inventory" class="text-title" />

        <ui:ScrollView name="ItemListContainer" class="item-list">
            <!-- Items dynamically generated in C# -->
        </ui:ScrollView>

        <ui:Button name="CloseButton" text="Close" class="button-primary" />
    </ui:VisualElement>
</ui:UXML>
```

### USS styling

```css
/* Assets/_Project/UI/USS/InventoryUI.uss */
.inventory-panel {
    width: 600px;
    height: 400px;
    position: absolute;
    top: 50%;
    left: 50%;
    translate: -50% -50%;
    background-color: rgba(0, 0, 0, 0.9);
    border-radius: 8px;
    padding: var(--padding-large);
}

.item-list {
    flex-grow: 1;
    background-color: rgba(0, 0, 0, 0.5);
    padding: var(--padding-medium);
    border-radius: 4px;
    margin: var(--margin-medium) 0;
}

.item-button {
    background-color: rgba(50, 50, 50, 0.8);
    margin-bottom: 4px;
    padding: 8px;
    border-radius: 4px;
    transition-duration: 0.2s;
}

.item-button:hover {
    background-color: rgba(80, 80, 80, 0.8);
}
```

### C# controller

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    /// <summary>
    /// Inventory UI controller
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InventoryUI : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private InventoryStateSO inventoryState;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onInventoryOpened;
        [SerializeField] private ItemEventChannelSO onItemSelected;

        private UIDocument uiDocument;
        private VisualElement inventoryPanel;
        private ScrollView itemListContainer;
        private Button closeButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;

            // Query UI elements
            inventoryPanel = root.Q<VisualElement>("InventoryPanel");
            itemListContainer = root.Q<ScrollView>("ItemListContainer");
            closeButton = root.Q<Button>("CloseButton");

            // Register callbacks
            closeButton.clicked += CloseInventory;
            onInventoryOpened.OnEventRaised += HandleInventoryOpened;

            // Initial state
            inventoryPanel.style.display = DisplayStyle.None;
        }

        private void OnDisable()
        {
            closeButton.clicked -= CloseInventory;
            onInventoryOpened.OnEventRaised -= HandleInventoryOpened;
        }

        private void HandleInventoryOpened()
        {
            inventoryPanel.style.display = DisplayStyle.Flex;
            RenderItemList();
        }

        private void RenderItemList()
        {
            itemListContainer.Clear();

            foreach (var slot in inventoryState.slots)
            {
                var itemButton = new Button(() => SelectItem(slot.item))
                {
                    text = $"{slot.item.itemName} x{slot.count}"
                };
                itemButton.AddToClassList("item-button");

                itemListContainer.Add(itemButton);
            }
        }

        private void SelectItem(ItemSO item)
        {
            onItemSelected?.RaiseEvent(item);
        }

        private void CloseInventory()
        {
            inventoryPanel.style.display = DisplayStyle.None;
        }
    }
}
```

---

## Pattern 5: Extending enum-based systems - P1

### Adding new quest type example

```csharp
namespace ProjectName.Quest
{
    // Extend existing enum
    public enum QuestType
    {
        Gather,   // Existing
        Deliver,  // Existing
        Hunt,     // New: Defeat enemies
        Escort    // New: Protect NPC
    }

    public enum ObjectiveType
    {
        CollectItem,
        DeliverItem,
        DefeatEnemy,   // New
        ProtectNPC,    // New
        ReachLocation
    }
}
```

### Create quest asset

```
Assets/_Project/ScriptableObjects/Data/Quest/
└── Quest_EnemyHunt5.asset
    questID: "quest_enemy_hunt_5"
    questName: "Defeat 5 Enemies"
    questType: Hunt
    objectives: [DefeatEnemy, 5]
    timeLimit: 300
```

---

## References

- [Event Channels](event-channels.md)
- [Variables System](variables.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [ReactiveEntitySet Pattern](reactive-entity-sets.md)
- [Design Principles](design-principles.md)
- [Dependency Management](dependency-management.md)
- [SpecKit Workflow](https://github.com/github/spec-kit)
