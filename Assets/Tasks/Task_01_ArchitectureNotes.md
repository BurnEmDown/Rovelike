# Interaction System Architecture

## Overview
The interaction system decouples Unity's input events from gameplay logic using an event-based architecture. This separation allows game logic to remain independent of Unity's input system, improving testability and maintainability.

## Key Components

### PointerClickUserInteractionSource (UnityCoreKit)
- **Type**: Unity MonoBehaviour implementing `IPointerClickHandler`
- **Purpose**: Converts Unity pointer events → `UserInteractionEvent`
- **Initialization**: Requires `Init(IUserInteractions, IUserInteractionTarget)` call
- **Location**: Attached to interactive GameObjects (e.g., TileView prefab)

**Requirements:**
- EventSystem in scene
- Collider2D (for Physics2DRaycaster) or UI Image (for GraphicRaycaster)

### IUserInteractionTarget (Interface)
- **Implemented by**: Interactive objects (TileView, etc.)
- **Properties**:
  - `InteractionKey`: String identifier for filtering ("Tile", "UI Button", etc.)
  - `Model`: The underlying data model (e.g., `IReadOnlyModuleTile`)

**Purpose**: Provides a uniform interface for any object that can be interacted with.

### UserInteractionEvent (Struct)
- **Type**: Value type (struct) for performance
- **Properties**:
  - `Type`: Enum (Click, Drag, LongPress, etc.)
  - `Target`: The `IUserInteractionTarget` that was interacted with
  - `WorldPosition`: World-space position of the interaction
- **Published via**: `IUserInteractions` service

### IUserInteractions (Service)
- **Type**: Global service (singleton pattern)
- **Purpose**: Central event hub for all user interactions
- **Key Feature**: Ownership-based subscriptions for automatic cleanup

## Design Patterns

### Ownership-Based Subscription Cleanup
Prevents memory leaks by automatically cleaning up subscriptions when owners are destroyed:

```csharp
// Subscribe with 'this' as owner
var interactions = CoreServices.Get<IUserInteractions>();
interactions.Subscribe(this, OnTileClicked);

// In OnDestroy, cleanup all subscriptions owned by 'this'
interactions?.UnsubscribeAll(this);
```

**Benefits:**
- No manual tracking of individual subscriptions
- Prevents memory leaks from orphaned event handlers
- Simple, foolproof pattern for MonoBehaviours

### Self-Initialization Pattern (TileView)
TileView manages its own interaction initialization rather than relying on external code:

```csharp
// In TileView.OnEnable()
if (interactionSource != null)
{
    var interactions = CoreServices.Get<IUserInteractions>();
    interactionSource.Init(interactions, this);
}
```

**Benefits:**
- No expensive GetComponent calls (uses Inspector-assigned reference)
- Single responsibility (TileView owns its setup)
- Automatic pooling support (re-initializes on enable)
- Decouples BoardPresenter from interaction system

### Separation of Concerns

**View Layer (TileView)**
- Only reports interactions, doesn't handle logic
- Implements `IUserInteractionTarget`
- Forwards events through `PointerClickUserInteractionSource`

**Controller Layer (BoardController, future: TileSelectionController)**
- Subscribes to interaction events
- Implements game logic in response to interactions
- Modifies game state

**Engine Layer (BoardState, MovementCalculator)**
- No knowledge of Unity input system
- Pure C# logic, fully testable
- Receives commands from controllers, not directly from input

## Initialization Flow

```
1. [Design Time] TileView prefab created with:
   - PointerClickUserInteractionSource component
   - BoxCollider2D component
   - TileView.interactionSource field assigned in Inspector

2. [Runtime] BoardPresenter.SpawnTileView() gets TileView from pool
   ↓
3. TileView.OnEnable() is called
   ↓
4. TileView initializes its interactionSource:
   interactionSource.Init(interactions, this)
   ↓
5. [User Action] Player clicks on tile
   ↓
6. Unity EventSystem → Physics2DRaycaster detects click on BoxCollider2D
   ↓
7. PointerClickUserInteractionSource.OnPointerClick() called
   ↓
8. interactionSource.Publish(new UserInteractionEvent(Click, this, worldPos))
   ↓
9. All subscribed controllers receive the event
   ↓
10. BoardController.OnTileClicked() logs the click
    (Future: TileSelectionController selects the tile)
```

## Usage Example

```csharp
public class TileSelectionController : MonoBehaviour
{
    private IUserInteractions interactions;
    
    private void Awake()
    {
        // Get the global interactions service
        interactions = CoreServices.Get<IUserInteractions>();
        
        // Subscribe to click events (passing 'this' as owner for cleanup)
        interactions.Subscribe(this, HandleTileClick);
    }

    private void OnDestroy()
    {
        // Cleanup all subscriptions owned by this controller
        interactions?.UnsubscribeAll(this);
    }

    private void HandleTileClick(UserInteractionEvent evt)
    {
        // Filter for tile interactions
        if (evt.Target is TileView tileView)
        {
            // Access the tile model
            var tile = tileView.Tile;
            
            // Implement selection logic
            SelectTile(tileView);
            
            Debug.Log($"Selected: {tile.TypeKey}");
        }
    }
    
    private void SelectTile(TileView tileView)
    {
        // Selection logic here...
    }
}
```

## Event Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ Unity Input Layer                                           │
│ - EventSystem                                               │
│ - Physics2DRaycaster                                        │
└────────────────┬────────────────────────────────────────────┘
                 │ Detects click on BoxCollider2D
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ PointerClickUserInteractionSource (on TileView)            │
│ - Receives OnPointerClick from Unity                       │
│ - Creates UserInteractionEvent                             │
└────────────────┬────────────────────────────────────────────┘
                 │ Publishes event
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ IUserInteractions Service (Global)                         │
│ - Maintains subscription list                              │
│ - Distributes events to all subscribers                    │
└────────────┬────────────────────┬───────────────────────────┘
             │                    │
             ▼                    ▼
┌──────────────────────┐  ┌─────────────────────────────────┐
│ BoardController      │  │ TileSelectionController         │
│ OnTileClicked()      │  │ HandleTileClick()               │
│ - Logs click         │  │ - Selects tile                  │
└──────────────────────┘  │ - Shows move preview            │
                          └─────────────────────────────────┘
```

## Advantages of This Architecture

### 1. **Decoupling**
- Input system changes don't affect game logic
- Can add new interaction types without modifying existing code
- Easy to support multiple input methods (mouse, touch, gamepad)

### 2. **Testability**
- Game logic can be tested by publishing mock events
- No need to simulate Unity input in tests
- Controllers can be tested in isolation

### 3. **Extensibility**
- New interaction types (Drag, LongPress) can be added easily
- Multiple subscribers can react to the same event
- Event filtering by `InteractionKey` allows fine-grained control

### 4. **Memory Safety**
- Ownership-based cleanup prevents memory leaks
- No manual subscription tracking needed
- Automatic cleanup on object destruction

### 5. **Performance**
- Events use value types (struct) for efficiency
- No GetComponent calls in hot paths
- Inspector-assigned references cached at design time

## Future Extensions

### Additional Interaction Types
```csharp
public enum UserInteractionType
{
    Click,
    Drag,        // Future: for dragging tiles
    LongPress,   // Future: for context menus
    Hover,       // Future: for tooltips
    DoubleClick  // Future: for quick actions
}
```

### Interaction Routing
Add priority/blocking system for UI overlays:
```csharp
// Block interactions when UI is open
interactions.SetInputEnabled(false);
```

### Touch Gesture Support
```csharp
// Future: Pinch to zoom, swipe to rotate
interactions.Subscribe(UserInteractionType.Pinch, OnPinchGesture, this);
```

### Input Recording/Replay
```csharp
// Future: Record player inputs for replay/debugging
interactionRecorder.StartRecording();
```

## Common Patterns

### Pattern 1: Type-Based Event Filtering
```csharp
private void HandleInteraction(UserInteractionEvent evt)
{
    switch (evt.Target)
    {
        case TileView tileView:
            HandleTileClick(tileView);
            break;
        case UIButton button:
            HandleButtonClick(button);
            break;
    }
}
```

### Pattern 2: InteractionKey-Based Filtering
```csharp
private void HandleInteraction(UserInteractionEvent evt)
{
    if (evt.Target.InteractionKey == "Tile")
    {
        // Handle tile interactions
    }
}
```

### Pattern 3: Conditional Subscription
```csharp
// Only subscribe when in gameplay state
public void EnterGameplayState()
{
    interactions.Subscribe(this, OnTileClicked);
}

public void ExitGameplayState()
{
    interactions.UnsubscribeAll(this);
}
```

## Troubleshooting

### "Clicks not detected"
- ✅ Check EventSystem exists in scene
- ✅ Check Physics2DRaycaster on EventSystem
- ✅ Check BoxCollider2D on TileView
- ✅ Verify interactionSource field assigned in Inspector
- ✅ Check TileView.OnEnable() is being called

### "Memory leaks"
- ✅ Always call UnsubscribeAll(this) in OnDestroy
- ✅ Pass 'this' as owner when subscribing
- ✅ Don't subscribe in Update/FixedUpdate

### "Events not received"
- ✅ Verify subscription happened before event published
- ✅ Check event type matches (Click vs. Drag)
- ✅ Verify InteractionKey matches if filtering

## Related Code Files

- `/Assets/UnityCoreKit/Runtime/UserInteractions/Unity/PointerClickUserInteractionSource.cs`
- `/Assets/UnityCoreKit/Runtime/UserInteractions/IUserInteractions.cs`
- `/Assets/UnityCoreKit/Runtime/UserInteractions/IUserInteractionTarget.cs`
- `/Assets/UnityCoreKit/Runtime/UserInteractions/UserInteractionEvent.cs`
- `/Assets/Scripts/Gameplay/Presentation/Tiles/TileView.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs`
