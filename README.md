# "Meteoroids" an Asteroids-Clone
A Unity/C# remake of the classic Asteroids arcade game, featuring movement, shooting, and scoring systems with a modern 3D art style.

Built as part of a Unity/C# course project. I implemented the core systems below and packaged a playable build.

## Download
- Windows build: (link here after creating a GitHub Release)

## Gameplay
Pilot a ship in open space, destroy incoming asteroid waves, and survive as long as possible. As your score increases, the game escalates with faster waves, special abilities, and an aggressive alien enemy ship.

## Features
- Ship movement with thrust and rotation
- Projectile-based shooting
- Asteroids that split into smaller fragments when destroyed
- Wave-based asteroid spawning with increasing difficulty
- Three-life system with game over and restart flow
- Hyperspace ability:
  - Earned after reaching a score threshold
  - Destroys nearby asteroids
  - Teleports the player to a random location on the screen
- Secondary alien enemy ship:
  - Spawns after the player reaches a secondary score threshold
  - Actively targets and fires at the player
  - Unique visuals and sound effects
- Screen wrap (objects loop seamlessly across screen edges)
- Controller support via Unity’s integrated input systems

## Controls
- Keyboard:
  - W / Up Arrow: Thrust
  - A / Left Arrow: Rotate left
  - D / Right Arrow: Rotate right
  - Space: HyperSpace
  - Left Mouse Buttone: Shoot
- Controller:
  - Left stick: Movement and Rotation
  - trigger: Shoot
  - Face Buttons: Hyperspace
  - (Controller bindings may vary)

## What I built / skills demonstrated
- C# gameplay scripting (movement, shooting, collisions, enemy behavior)
- State-driven systems (lives, score thresholds, wave progression)
- Enemy AI behavior (alien ship targeting and firing logic)
- Ability design and balance (hyperspace mechanic tied to score progression)
- Audio and visual polish beyond course requirements (custom explosion sounds and color changes)
- Input handling with both keyboard and controller support
- Build and packaging, including a Windows installer

## Tech
- Engine: Unity (version:2021.3.12f1)
- Language: C#
- Platform: Windows

## Screenshots
![Gameplay Action](Media/Meteoroids Action Screenshot.png)
![Alien Enemy Encounter](Media/Meteoroids Alien Screenshot.png)
![Main Menu](Media/Meteoroids Main Menu Screenshot.png)

## What I would improve if I were to continue this project
- Additional enemy types or behaviors
- Expanded player abilities or power-ups
- High-score saving / leaderboard
- Visual polish such as screen shake and more particle effects.
