# Deterministic Roulette

A Unity 6000.x case-study prototype focused on deterministic roulette outcomes, full betting flow, round-based result evaluation, and clean UI/gameplay synchronization.

## Project Summary
This project implements a 3D single-player roulette prototype where the player can either:

- select the next winning outcome manually (deterministic mode), or
- continue with standard round-based roulette gameplay.

The implementation emphasizes:

- reliable gameplay mechanics,
- modular architecture (service + facade + presenter flow),
- animation-driven round lifecycle,
- and clear player-facing state/statistics tracking.

## Tech Stack
- Engine: Unity 6000.x
- Language: C#
- UI: Unity UI + TextMeshPro
- Audio: Event-based audio service + audio event bank
- Version Control: Git

## Setup and Run
1. Open the project in Unity 6000.x.
2. Open the main scene:
   - Assets/Scenes/Roulette.unity
3. Wait for script compilation/import.
4. Press Play in the Unity Editor.

## Gameplay Controls and Flow
### Round Flow
1. Select stake/chip amount.
2. Place one or more bets on the board.
3. Optionally select a deterministic next outcome.
4. Press Spin.
5. Wait for wheel/ball animation to complete.
6. Review result number, won/lost amount, and updated statistics.
7. Continue to the next round.

### Main UI Actions
- Spin Button: Executes one round.
- Clear Bets Button: Clears active bets.
- Outcome Selection: Sets deterministic next result.
- Bet Board: Places/removes valid roulette bets.
- History Panel: Shows round-by-round history entries.

## Implemented Features
### 1) Roulette Wheel and Animation
- Deterministic outcome selection is supported.
- Wheel spin + ball animation lifecycle is implemented.
- Round result presentation is synchronized with spin lifecycle flow.

### 2) Statistics and Historical Record
- Spin count and win/loss tracking are implemented.
- Per-round result history is shown in UI.
- History list rebuild from existing state is supported.

### 3) Roulette Rules and Mechanics
- Standard inside and outside bet types are supported.
- Payout evaluation per round is implemented.
- Multi-round gameplay loop is fully supported.

## Architecture Overview
The project is structured to keep game rules, UI behavior, and presentation concerns separated.

### Core Layers
- Data Models:
  - round result and game state containers
- Gameplay Services:
  - outcome selection, bet management, payout calculation, round execution, stats updates
- Facade Layer:
  - GameUiFacade as UI entry point to gameplay logic
- UI Layer:
  - GameUIController for orchestration
  - RoundResultPresenter for result/state labels
  - RoundHistoryListPresenter for history UI
  - SpinLifecycleController for animation-result synchronization
- Animation Layer:
  - RouletteWheelAnimator handles wheel/ball animation and lifecycle events
- Audio Layer:
  - GameAudioService + GameAudioEventBank for centralized event playback

### Design Patterns Used
- Facade Pattern:
  - GameUiFacade simplifies UI-to-gameplay integration.
- Observer / Event-Driven Pattern:
  - round and animation lifecycle updates are event-based.
- Presenter Pattern:
  - UI rendering logic is isolated in presenter components.
- Service-Oriented Composition:
  - betting, payout, outcome, and state logic are split into focused units.

### OOP/SOLID Alignment
- Single Responsibility:
  - logic is split across dedicated components/services.
- Open/Closed:
  - features like audio events and UI reactions are extensible.
- Practical Dependency Inversion:
  - UI uses a facade/contracts boundary instead of direct deep coupling.

## Repository Structure (High-Level)
- Assets/Scripts/Core
- Assets/Scripts/Gameplay
- Assets/Scripts/UI
- Assets/Scripts/Audio
- Assets/Scripts/Data

## Demo
- Gameplay Video: TBD
- Optional Build Link: TBD

## Known Issues / Future Improvements
### Planned Audio Improvements
- Add dedicated Ball Drop SFX
- Add Round Won SFX
- Add Round Lost SFX

### Planned Visual Improvements
- Add win VFX (confetti/chip sparkle)
- Add richer highlight effects for winning outcomes

### Optional Extensions
- Save/Load game state (auto-save + resume)
- American roulette mode (double-zero)
- Additional automated play-mode test coverage

## Notes
- The project uses Unity-native systems and avoids external gameplay plugin dependency.
- Focus is on deterministic gameplay validation, maintainability, and clear architecture communication for technical review.