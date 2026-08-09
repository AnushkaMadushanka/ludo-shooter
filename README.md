# Ludo Shooter

**A wave shooter where reloading is a gamble.** The board pieces are trying to kill you. Every time you reload, the dice rolls — and whatever face it lands on rewrites the rules of the fight.

[![Play](https://img.shields.io/badge/▶-Play%20on%20itch.io-fa5c5c)](https://anushka-madushanka.itch.io/ludo-shooter)
[![Video](https://img.shields.io/badge/▶-Watch%20gameplay-red)](https://youtu.be/VAIWMgyLycM)
![Unity](https://img.shields.io/badge/Unity-2021.1-black)
![C#](https://img.shields.io/badge/C%23-URP-239120)

<img src="https://raw.githubusercontent.com/AnushkaMadushanka/anushkamadushanka.github.com/gh-pages/projects/ludo_shooter.webp" alt="Ludo Shooter" width="600">

Solo project — concept, systems, code and release.

---

## The design idea

Reloading in a shooter is dead time. Ludo Shooter turns it into the most interesting moment in the loop: the reload triggers a physical dice roll, and the face it settles on fires a modifier that changes the encounter — time slows, every enemy on the board dies, everything freezes.

The tension is that you *need* to reload, but you don't get to choose what happens when you do.

## How the dice system works

The mechanic is data-driven rather than a switch statement, which is what makes it tunable without a rebuild:

```csharp
[System.Serializable]
public class Side {
    public Material material;   // what shows on the face
    public UnityEvent action;   // what happens when it lands
}
```

Each wave, `DiceRollScript.InitializeDice()` draws a fresh random subset of `Side`s from the pool **without replacement** and paints them onto the die's submeshes — so the player can read the current roster of possible outcomes off the die itself before committing to a reload. Rolling spins the transform through several full revolutions, DOTween-eases it into the resting rotation for the chosen face, and invokes that side's `UnityEvent`.

Adding a new modifier is an inspector job: drop a material in, wire the event to a method. `GameManager` exposes them as plain public methods — `SlowDown()`, `DestroyAllEnemies()`, `FreezeAllEnemies()`.

## Budget-based wave generation

Waves aren't authored by hand. `WaveSpawner` gives each wave a point budget (`wave × 10`) and each enemy type a cost, then fills the budget with random affordable picks:

```
waveValue = currWave * 10
while budget remains:
    pick a random enemy the budget can still afford
    subtract its cost
```

Cheap enemies swarm early, expensive ones start appearing as the budget grows, and difficulty scales without a single hand-tuned wave table. Spawns are then drip-fed from random spawn points on an interval rather than dumped at once. Highest wave reached persists via `PlayerPrefs`.

## Built with

| | |
|---|---|
| Engine | Unity 2021.1.23f1, Universal Render Pipeline |
| Input | Unity Input System (`PlayerInput` actions for move / jump / shoot / pause) |
| Camera | Cinemachine, with `SwitchVCam` for aim-mode blending |
| Animation | Animation Rigging, DOTween |
| Effects | VFX Graph, custom projectile VFX |
| UI | TextMeshPro |

Shooting is raycast-based with a miss-distance fallback, muzzle and hit VFX, and a bullet counter driving the reload that triggers the roll.

## Running it

```bash
git clone https://github.com/AnushkaMadushanka/ludo-shooter.git
```

Open with **Unity 2021.1.23f1**, load `Assets/Scenes/MainMenu.unity` and press Play. Or just [play it in the browser on itch.io](https://anushka-madushanka.itch.io/ludo-shooter).

## Project layout

```
Assets/Scripts/
├── GameManager.cs          wave flow, countdown, pause, dice modifier effects
├── DiceRollScript.cs       face selection, roll animation, modifier dispatch
├── WaveSpawner.cs          budget-based wave composition and spawning
├── PlayerController.cs     movement, aiming, raycast shooting, reload
├── EnemyPieceController.cs board-piece enemy behaviour
├── EnemyDiscController.cs  disc enemy behaviour
├── HealthScript.cs         shared damage/death handling
└── SwitchVCam.cs           Cinemachine aim-mode switching
```

---

Built by [Anushka Madushanka](https://anushkamadushanka.github.io) · [More projects](https://anushkamadushanka.github.io/#projects)
