## Rovelike – Development Notes & Roadmap

This document tracks what has been implemented so far, key architectural decisions, and the planned next steps for the project.

The current phase is focused on creating a mechanics-faithful, engine-clean clone of ROVE as a programming and architecture exercise.
No original mechanics or theming decisions are introduced yet. After the game is finished, a private fork may explore original ideas.

---

### 1. Project Goal (Current Phase)

- Create a mechanically faithful ROVE-like engine
- Emphasize:
  - Clean architecture
  - SOLID principles
  - Separation of engine / game / presentation
- No monetization, publishing, or licensed assets
- Open-source friendly, modular, testable

A **private fork** may later introduce original mechanics and theming.

---

### 2. High-Level Architecture

#### Assemblies / Layers

- **Engine**
  - Pure C# logic
  - No Unity dependencies
  - Fully testable

- **Game**
  - Game-specific configuration and orchestration
  - Factories, definitions, services

- **Presentation**
  - Unity MonoBehaviours
  - Views, presenters, input forwarding
- **UnityCoreKit**
  - Shared infrastructure (events, pooling, interactions, services)

---

### 3. Engine – Implemented Systems
#### Board System

- **BoardState**
  - Holds board dimensions
  - Tracks tile positions
  - Enforces bounds and placement rules
- **CellPos**
  - Value struct for board coordinates
- Board is authoritative; tiles are unaware of the board

#### Tile Model

- **Tile** (base)
- ModuleTile
  - Composed of behaviors:
    - IMovementBehavior
    - IAbilityBehavior
- Read-only interface:
  - IReadOnlyModuleTile
- Tiles are **pure data + behavior**, no Unity logic

#### Movement System

- **MovementRules**
  - Max steps
  - Orthogonal / diagonal
  - Obstacle pass rules
- **MovementCalculator**
  - Stateless
  - Computes legal move destinations
- Supports future extensions (push, conditional movement)

---

### 4. Game Layer - Implemented Systems

#### Definitions
- **TileDefinition** (ScriptableObject)
  - TypeKey
  - Movement rules
  - Visual references (sprite)
- **TileLibrarySO**
  - Central collection of all tile definitions

#### Factories
- **TileFactory**
  - Creates Tile instances from TileDefinitions
- **EngineTileFactory**
  - Creates engine tiles using configs
- Factory logic is isolated and replaceable

#### Services
- TileLibraryService
  - Runtime access to tile definitions
- Registered via GameServices
- Designed to support future loading strategies (Addressables, Remote Config)

---

### 5. Presentation Layer - Implemented Systems

#### TileView
- Unity Monobehavior
- Holds:
  - **IReadOnlyModuleTile**
  - Board Position
- Responsible only for:
  - Rendering
  - Forwarding interaction
- Cannot mutate engine state

#### BoardController (Temporary)
- Creates tiles
- Creates board
- Randomly places tiles (ROVE-style puzzle generation)
- Used for early testing / validation

---

### 6. User Interaction System (UnityCoreKit)

#### UserInteractions
- Decouples Unity input from gameplay intent
- Based on:
  - **UserInteractionEvent**
  - **UserInteractionType** (Click, future: Drag, LongPress, etc.)
- Uses:
  - **IEventsManager**
  - **IEventListenerManager** (owner-based cleanup)

#### Key Principles
- Views emit interactions
- Gameplay / presentation reacts elsewhere
- Ownership-based subscription cleanup
- No direct coupling between input source and effect logic

---

### 7. Explicit Architectural Decisions

- Tiles **do not know about the board**
- Views **do not modify engine state**
- Engine code is Unity-free
- Composition over inheritance
- ScriptableObjects are data only
- Lifecycle ownership is explicit
- No over-engineering until needed (e.g. no interaction router yet)

---

### 8. What is *Not* Implemented Yet

- Tile abilities
- Tile movement execution (only calculation exists)
- Visual feedback (highlights, animations)
- UI
- Win / loss conditions
- Undo / reset
- Level progression
- Save / load
- Original mechanics or theme

---

### 9. Recommended Next Steps (In Order)

#### Step 1 - BoardPresenter (Presentation)

**Goal:** Visualize the board cleanly
- Create **BoardPresenter**
- Responsibilities:
  - Spawn **TileView** instances (via PoolManager)
  - Position views from CellPos
  - Sync engine -> view state
- No interaction logic yet

---

#### Step 2 - Tile Interaction Source

**Goal:** Detect clicks on tiles
- Add **InteractionSource** component
- Uses Unity input (**IPointerClickHandler**)
- Publishes **UserInteractionEvent**
- No game logic inside the view

---

#### Step 3 - Tile Selection System

**Goal:** First meaningful interaction
- Subscribe to tile click events
- Track:
  - Selected tile
  - Deselection on second click
- No movement yet
- Add minimal visual feedback (outline, tint)

---

#### Step 4 - Move Preview

**Goal:** Connect engine -> presentation
- On tile selection:
  - Query **GetAvailableMoves**
  - Visualize valid destinations
- Still no movement execution

---

#### Step 5 - Move Execution

**Goal:** First real gameplay loop
- Click destination cell
- Apply move via BoardState
- Update TileView positions
- Clear selection

---

#### Step 6 - Abilities (Later)

- Ability contexts
- Ability preview
- Ability execution
- Cooldowns / one-off rules

---

### 10. When to Introduce More Abstractions

Introduce **only when needed:*
- Interaction routing -> when effects multiply
- Command pattern -> when undo/reset is added
- State machines -> when game modes expand
- Addressables -> when content grows

---

### 11. Success Criteria for This Phase

- Board works visually
- Tiles can be selected
- Legal moves are previewed
- Tiles can be moved
- Architecture remains clean and testable
- No "just make it work" shortcuts

---

### 12. Notes to Future Self

- Don't rush originality
- Let architecture guide mechanics, not the other way around
- If something feels over-engineered, pause
- If something feels messy, refactor early

---