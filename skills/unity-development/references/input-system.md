# Input System integration

## Action Maps organization

```
PlayerInputActions.inputactions
├── Player (Action Map)      <- Gameplay
│   ├── Move (Vector2)
│   ├── Look (Vector2)
│   ├── Jump (Button)
│   └── Fire (Button)
├── UI (Action Map)          <- Menu
│   ├── Navigate (Vector2)
│   ├── Submit (Button)
│   └── Cancel (Button)
└── Vehicle (Action Map)     <- Driving
    ├── Steer (Vector2)
    └── Accelerate (Button)
```

### Switching Action Maps

```csharp
playerInput.SwitchCurrentActionMap("UI");

// Or manual
inputActions.Player.Disable();
inputActions.UI.Enable();
```

## InputReader pattern

### InputReader ScriptableObject

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectName.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
    public class InputReaderSO : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        public event Action<Vector2> OnMoveEvent;
        public event Action<Vector2> OnLookEvent;
        public event Action OnJumpEvent;
        public event Action OnFireEvent;

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

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnJumpEvent?.Invoke();
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                OnFireEvent?.Invoke();
        }

        public void EnablePlayerInput() => inputActions?.Player.Enable();
        public void DisablePlayerInput() => inputActions?.Player.Disable();
    }
}
```

### Usage in MonoBehaviour

```csharp
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

    private void HandleMove(Vector2 input) => moveInput = input;
    private void HandleJump() => Debug.Log("Jump!");

    private void Update()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}
```

## EventChannel bridge

```csharp
public class InputEventBridge : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReaderSO inputReader;

    [Header("Event Channels")]
    [SerializeField] private Vector2EventChannelSO onMoveInput;
    [SerializeField] private VoidEventChannelSO onJumpInput;

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

    private void HandleMove(Vector2 input) => onMoveInput?.RaiseEvent(input);
    private void HandleJump() => onJumpInput?.RaiseEvent();
}
```

## Testing with mock input

```csharp
[CreateAssetMenu(fileName = "MockInputReader", menuName = "Input/Mock Input Reader")]
public class MockInputReaderSO : InputReaderSO
{
    public void SimulateMove(Vector2 input) => InvokeMove(input);
    public void SimulateJump() => InvokeJump();
}
```

## Project setup

1. Install Input System Package
2. Enable in Project Settings > Player > Active Input Handling
3. Create Input Action Asset
4. Enable "Generate C# Class"

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
