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

## SpecKit Workflow

All new features must follow the SpecKit (Spec-Driven Development) workflow:

1. **`/specify`** - Define feature requirements (`docs/00_spec/`)
2. **`/clarify`** - Resolve ambiguities (`docs/00_spec/clarifications.md`)
3. **`/plan`** - Create technical implementation plan (`docs/98_plans/`)
4. **`/tasks`** - Break down into implementation tasks (`docs/98_plans/tasks/`)
5. **`/implement`** - Execute tasks and implement

**Reference:** https://github.com/github/spec-kit

### Workflow Example

```
1. Create docs/00_spec/inventory-system-spec.md
2. Update docs/00_spec/clarifications.md with design decisions
3. Create docs/98_plans/inventory-system-plan.md
4. Create docs/98_plans/tasks/inventory-system-tasks.md
5. Implement tasks sequentially
6. Update documentation after completion
```

---

## Pattern 1: Adding New ScriptableObject

### Immutable Data Definition

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

### Runtime Data (When Needed)

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

### CreateAssetMenu Naming Conventions

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

## Pattern 2: Adding New EventChannel

### EventChannel Class Definition

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Item
{
    /// <summary>
    /// EventChannel for Item-related events
    /// </summary>
    [CreateAssetMenu(fileName = "ItemEventChannel", menuName = "ProjectName/Events/Item Event Channel")]
    public class ItemEventChannelSO : EventChannelSO<ItemSO>
    {
        // Tang3cko.EventChannels base class provides all functionality
    }
}
```

### ScriptableObject Asset Creation

Create assets in the appropriate directory:

```
Assets/_Project/ScriptableObjects/Events/Item/
├── OnItemAcquired.asset (ItemEventChannelSO)
├── OnItemUsed.asset (ItemEventChannelSO)
└── OnInventoryChanged.asset (VoidEventChannelSO)
```

### Publisher and Subscriber Implementation

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

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

## Pattern 3: Adding New RuntimeSet

### RuntimeSet Class Definition

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Enemy RuntimeSet for dynamic tracking
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
    public class EnemyRuntimeSetSO : ScriptableObject
    {
        private List<EnemyController> items = new List<EnemyController>();

        /// <summary>
        /// Read-only access to all enemies
        /// </summary>
        public IReadOnlyList<EnemyController> Items => items;

        /// <summary>
        /// Add an enemy to the set
        /// </summary>
        public void Add(EnemyController enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[EnemyRuntimeSet] Attempted to add null enemy");
                return;
            }

            if (!items.Contains(enemy))
            {
                items.Add(enemy);
            }
        }

        /// <summary>
        /// Remove an enemy from the set
        /// </summary>
        public void Remove(EnemyController enemy)
        {
            items.Remove(enemy);
        }

        /// <summary>
        /// Clear all enemies
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Get count of active enemies
        /// </summary>
        public int Count => items.Count;
    }
}
```

### Registration Pattern

```csharp
using UnityEngine;

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

    // Usage example
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyRuntimeSetSO enemySet;

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

## Pattern 4: Adding New UI (UI Toolkit)

### UXML Structure

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

### USS Styling

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

### C# Controller

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.EventChannels;

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

## Pattern 5: Extending Enum-Based Systems

### Adding New Quest Type Example

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

### Create Quest Asset

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

- [ScriptableObject Pattern](scriptableobject.md)
- [Event Channels](event-channels.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [Dependency Management](dependency-management.md)
- [SpecKit Workflow](https://github.com/github/spec-kit)
