---
name: unity-extension-patterns
description: Extension patterns for ScriptableObject architecture. Adding new SO, EventChannel, RuntimeSet. Use when extending Unity projects.
---

# Extension Patterns

## Purpose

Define consistent implementation patterns for adding new features using ScriptableObject-driven architecture.

## Checklist

- [ ] Use EventChannels for decoupled communication
- [ ] Manage data with ScriptableObjects
- [ ] Follow CreateAssetMenu naming conventions
- [ ] Separate immutable data from runtime state
- [ ] Subscribe to events in OnEnable, unsubscribe in OnDisable

---

## Pattern 1: Adding new ScriptableObject - P1

### Immutable data definition

```csharp
using UnityEngine;

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

### CreateAssetMenu naming conventions

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
    [CreateAssetMenu(fileName = "ItemEventChannel", menuName = "ProjectName/Events/Item Event Channel")]
    public class ItemEventChannelSO : EventChannelSO<ItemSO>
    {
    }
}
```

### Publisher and subscriber

```csharp
// Publisher
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    [SerializeField] private ItemEventChannelSO onItemAcquired;

    public void Pickup()
    {
        onItemAcquired?.RaiseEvent(item);
    }
}

// Subscriber
public class InventoryManager : MonoBehaviour
{
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
    }
}
```

---

## Pattern 3: Adding new RuntimeSet - P1

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "Reactive SO/Runtime Sets/Enemy")]
    public class EnemyRuntimeSetSO : RuntimeSetSO<EnemyController>
    {
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
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyRuntimeSetSO enemySet;

    private void OnEnable()
    {
        enemySet?.Add(this);
    }

    private void OnDisable()
    {
        enemySet?.Remove(this);
    }
}
```

---

## Pattern 4: Adding new UI (UI Toolkit) - P1

### C# controller

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class InventoryUI : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private InventoryStateSO inventoryState;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onInventoryOpened;

        private UIDocument uiDocument;
        private VisualElement inventoryPanel;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;

            inventoryPanel = root.Q<VisualElement>("InventoryPanel");
            onInventoryOpened.OnEventRaised += HandleInventoryOpened;

            inventoryPanel.style.display = DisplayStyle.None;
        }

        private void OnDisable()
        {
            onInventoryOpened.OnEventRaised -= HandleInventoryOpened;
        }

        private void HandleInventoryOpened()
        {
            inventoryPanel.style.display = DisplayStyle.Flex;
            RenderItemList();
        }
    }
}
```
