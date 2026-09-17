# 2D Roguelike

A learning project based on the official Unity Learn course "2D Roguelike Tutorial".

A turn-based 2D roguelike with procedurally generated levels, a food resource, destructible walls, and win/lose conditions.

## Features

- **Procedural level generation** — every level is built from scratch.
- **Controllable player character** — grid-based movement with arrow keys.
- **Turn system** — every step counts as a turn and consumes food.
- **Food resource** — decreases each turn, replenished by picking up food.
- **Walls** — block movement and can be destroyed after 3 hits.
- **Exit cell** — entering it generates a new level.
- **Game Over** — shown when food runs out.
- **Restart** — press Enter to start a new game.

## Controls

| Key | Action |
|---|---|
| ↑ ↓ ← → | Move the player |
| Enter | Restart after Game Over |

## Tech Stack

- **Unity 6** (6000.5.10f1)
- **C#**
- **UI Toolkit** (UXML/USS)
- **Tilemap + Tile Palette**
- **Input System** (new)

## Project Structure
