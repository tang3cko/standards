---
name: unity-input-system
description: Unity Input System integration. InputReader pattern, Action Maps, EventChannel bridging. Use when handling input in .cs files.
---

# Input System integration

## Purpose

Define patterns for integrating Unity's Input System with ScriptableObject-based architecture using the InputReader pattern for decoupled, testable input handling.

## Checklist

- [ ] Use Input System Package (not legacy Input Manager)
- [ ] Organize Action Maps by context (Player, UI, Vehicle)
- [ ] Generate C# class from Input Action Asset
- [ ] Create InputReader ScriptableObject for input decoupling
- [ ] Expose input events through C# events
- [ ] Use EventChannels for cross-system input communication

---

## Input Action Asset organization - P1

Action Maps group actions that contextually belong together. Separate by game context to enable clean state switching.

### Recommended organization

```
PlayerInputActions.inputactions
├── Player (Action Map)      <- Gameplay: movement, combat
│   ├── Move (Vector2)
│   ├── Look (Vector2)
│   ├── Jump (Button)
│   └── Fire (Button)
├── UI (Action Map)          <- Menu: navigation
│   ├── Navigate (Vector2)
│   ├── Submit (Button)
│   └── Cancel (Button)
└── Vehicle (Action Map)     <- Driving: separate controls
    ├── Steer (Vector2)
    ├── Accelerate (Button)
    └── Brake (Button)
```

### Switching Action Maps

Only one Action Map should be active at a time for a given context:

```csharp
// Using PlayerInput component
playerInput.SwitchCurrentActionMap("UI");

// Or manual switching
inputActions.Player.Disable();
inputActions.UI.Enable();
```

### Generate C# class (required)

Always enable "Generate C# Class" in the Input Action Asset inspector:
- Provides type-safe access to Actions
- Enables IntelliSense autocomplete
- Prevents runtime typos

---

## InputReader pattern - P1

The InputReader pattern wraps Input System actions in a ScriptableObject, decoupling input detection from game behavior.

### Benefits

- **Testability**: Mock inputs without Input System
- **Decoupling**: Game logic doesn't depend on input implementation
- **Flexibility**: Swap input readers for different contexts (gameplay, menu, cutscene)
- **Inspector**: Assign different readers via Inspector

---

## Basic InputReader - P1

### InputReader ScriptableObject

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectName.Input
{
    /// <summary>
    /// ScriptableObject that wraps Input System actions.
    /// Decouples input detection from game behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
    public class InputReaderSO : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        // Events for game systems to subscribe
        public event Action<Vector2> OnMoveEvent;
        public event Action<Vector2> OnLookEvent;
        public event Action OnJumpEvent;
        public event Action OnJumpCanceledEvent;
        public event Action OnFireEvent;
        public event Action OnInteractEvent;

        private PlayerInputActions inputActions;

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Player.Disable();
        }

        // IPlayerActions implementation
        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            OnLookEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnJumpEvent?.Invoke();
            if (context.phase == InputActionPhase.Canceled)
                OnJumpCanceledEvent?.Invoke();
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnFireEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnInteractEvent?.Invoke();
        }

        // Enable/disable input maps
        public void EnablePlayerInput() => inputActions?.Player.Enable();
        public void DisablePlayerInput() => inputActions?.Player.Disable();
    }
}
```

### Usage in MonoBehaviour

```csharp
using UnityEngine;

namespace ProjectName.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private float moveSpeed = 5f;

        private Vector2 moveInput;

        private void OnEnable()
        {
            inputReader.OnMoveEvent += HandleMove;
            inputReader.OnJumpEvent += HandleJump;
        }

        private void OnDisable()
        {
            inputReader.OnMoveEvent -= HandleMove;
            inputReader.OnJumpEvent -= HandleJump;
        }

        private void HandleMove(Vector2 input)
        {
            moveInput = input;
        }

        private void HandleJump()
        {
            // Jump logic
            Debug.Log("Jump!");
        }

        private void Update()
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }
}
```

---

## EventChannel integration - P1

For cross-system communication, bridge InputReader events to EventChannels.

### InputEventBridge

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Input
{
    /// <summary>
    /// Bridges InputReader events to EventChannels for cross-system communication.
    /// </summary>
    public class InputEventBridge : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("Event Channels")]
        [SerializeField] private Vector2EventChannelSO onMoveInput;
        [SerializeField] private VoidEventChannelSO onJumpInput;
        [SerializeField] private VoidEventChannelSO onFireInput;
        [SerializeField] private VoidEventChannelSO onInteractInput;

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnMoveEvent += HandleMove;
                inputReader.OnJumpEvent += HandleJump;
                inputReader.OnFireEvent += HandleFire;
                inputReader.OnInteractEvent += HandleInteract;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnMoveEvent -= HandleMove;
                inputReader.OnJumpEvent -= HandleJump;
                inputReader.OnFireEvent -= HandleFire;
                inputReader.OnInteractEvent -= HandleInteract;
            }
        }

        private void HandleMove(Vector2 input) => onMoveInput?.RaiseEvent(input);
        private void HandleJump() => onJumpInput?.RaiseEvent();
        private void HandleFire() => onFireInput?.RaiseEvent();
        private void HandleInteract() => onInteractInput?.RaiseEvent();
    }
}
```

---

## Testing with mock input - P2

### Mock InputReader for tests

```csharp
using System;
using UnityEngine;

namespace ProjectName.Input
{
    /// <summary>
    /// Mock InputReader for testing without Input System.
    /// </summary>
    [CreateAssetMenu(fileName = "MockInputReader", menuName = "Input/Mock Input Reader")]
    public class MockInputReaderSO : InputReaderSO
    {
        // Manually trigger events for testing
        public void SimulateMove(Vector2 input) => InvokeMove(input);
        public void SimulateJump() => InvokeJump();
        public void SimulateFire() => InvokeFire();
    }
}
```

### Edit Mode test

```csharp
using NUnit.Framework;
using UnityEngine;

namespace ProjectName.Tests
{
    public class PlayerControllerTests
    {
        [Test]
        public void Player_OnMoveInput_UpdatesPosition()
        {
            // Arrange
            var mockInput = ScriptableObject.CreateInstance<MockInputReaderSO>();
            var player = new GameObject().AddComponent<PlayerController>();
            // Inject mockInput via reflection or public property

            // Act
            mockInput.SimulateMove(Vector2.right);

            // Assert
            // Verify player position changed
        }
    }
}
```

---

## Input context switching - P2

### Multiple Input Readers

```csharp
using UnityEngine;

namespace ProjectName.Input
{
    /// <summary>
    /// Manages switching between different input contexts.
    /// </summary>
    public class InputContextManager : MonoBehaviour
    {
        [SerializeField] private InputReaderSO gameplayInput;
        [SerializeField] private InputReaderSO menuInput;

        private InputReaderSO currentInput;

        public void SwitchToGameplay()
        {
            menuInput?.DisablePlayerInput();
            gameplayInput?.EnablePlayerInput();
            currentInput = gameplayInput;
        }

        public void SwitchToMenu()
        {
            gameplayInput?.DisablePlayerInput();
            menuInput?.EnablePlayerInput();
            currentInput = menuInput;
        }
    }
}
```

---

## Project setup - P1

### Installation

1. Install Input System Package:
   - Window > Package Manager > Input System
   - Or add to `manifest.json`:
   ```json
   "com.unity.inputsystem": "1.7.0"
   ```

2. Enable new Input System:
   - Edit > Project Settings > Player
   - Active Input Handling: "Input System Package (New)"
   - Or "Both" for gradual migration

3. Create Input Action Asset:
   - Right-click > Create > Input Actions
   - Define action maps and actions
   - Generate C# class

### Directory structure

```
Assets/
├── Settings/
│   └── PlayerInputActions.inputactions
├── ScriptableObjects/
│   └── Input/
│       └── InputReader.asset
└── Scripts/
    └── Input/
        ├── InputReaderSO.cs
        └── InputEventBridge.cs
```
